$(function() {
    

    // Init the JQuery UI elements.
    function initTabs() {
        $("#tabs").tabs();

        $("#tabs").css("visibility", "visible");

        $("#edittab").hide();
    }
    initTabs();
    
    // Setup listeners 
    // (NOTE: The entire tab-div (including the edit dialog) is reloaded, which is why we have to attach on a parent container (or body) and not directly on the target DOM elements (because 
    //        these DOM elements will be deleted after a reload))
    {
        var container = $("#tabsAndTablesContainer");

        container.on("click", ".edit-row", function(e) { // Edit button click listener
            var elem = $(this);
            var dashboardTemplateId = elem.closest('tr').data('id');
            EditDialog.openEditDialog(dashboardTemplateId);
            e.preventDefault();
        });

        container.on("click", ".delete-row", function (e) { // Delete button click listener
            var elem = $(this);
            var dashboardTemplateId = elem.closest('tr').data('id');
            deleteDashboardTemplate(dashboardTemplateId);
            e.preventDefault();
        });

        $("body").on("click", "#create-new", function (e) { // Create button click listener
            if (!$("#create-new").hasClass("disabled")) {
                EditDialog.openCreateDialog();
            }
            e.preventDefault();
        });

        $("body").on("click", "#refresh-cached-names-button", function (e) { // Refresh button click listener
            if (!$("#refresh-cached-names-button").hasClass("disabled")) {
                refreshCachedNamesFromUAM();
            }
            e.preventDefault();
        });

        container.on("change", ".editDialog #loginNameTextfield", function (e) { // Edit-Dialog: textfield listener
            var textfield = $(this);
            var loginId = textfield.val();
            EditDialog.populateRoleSelector(loginId);
        });

        container.on("keydown", ".editDialog #loginNameTextfield", function (e) { // Edit-Dialog: textfield listener
            EditDialog.loginNameTextFieldKeyDownHandler();
            if (e.which == 13) e.preventDefault(); // <- IE automatically clicks one of the <button>s when pressing return in the textfield. This is to prevent that.
        });
        
        container.on("change", ".editDialog #roleSelector", function (e) { // Edit-Dialog: selector listener
            EditDialog.updateUserDashboardPreview();
        });

        container.on("click", ".editDialog #createTemplateButton", function (e) { // Edit-Dialog: "create new template" button
            EditDialog.createDashboardTemplate();
        });

        container.on("click", ".editDialog #updateTemplateButton", function (e) { // Edit-Dialog: "edit new template" button
            EditDialog.updateDashboardTemplate();
        });

        container.on("click", ".editDialog #cancelEditButton", function (e) { // Edit-Dialog: "cancel" button
            EditDialog.cancelEditDialog();
        });
    }
   
    function deleteDashboardTemplate(dashboardTemplateId) {
        if (confirm("This will delete the dashboard template.")) {
            $.post('DashboardTemplate/Delete', {
                id: dashboardTemplateId
            }).done(function (data) {
                // remember currently selected tab (its orgId)
                var tabOrgId = getCurrentlySelectedTabOrgId();
                // reload all tabs
                reloadTabsAndTables(tabOrgId);
            }).fail(function (errorJson) {
                alert("error deleting the template.");
            });
        }
    }
    
    function getCurrentlySelectedTabOrgId() { // only works if we are not in edit-mode
        var tabIndex = $("#tabs").tabs('option', 'selected');
        var tabOrgId = $('#tabs ul li a').eq(tabIndex).data("orgid");
        return tabOrgId;
    }
    
    function reloadTabsAndTables(orgIdOfTabToSelect, roleIdToBlink) { // parameters are optional
        var tabContainer = $("#tabsAndTablesContainer");
        tabContainer.css("opacity", "0.5");
        $.ajax({
            type: "GET",
            url: "DashboardTemplate/TabsAndTables",
            data: {},
            success: function (data, textStatus, xhr) {
                tabContainer.empty().html(data);
                tabContainer.css("opacity", "1");
                initTabs();

                // Go to tab of selected organization id (unless none was selected)
                var tabIndex = $('#tabs a[href="#orgtab-' + orgIdOfTabToSelect + '"]').parent().index();    // -1 if not exists (param not given or after a delete or if a role has changed organization)
                $("#tabs").tabs("select", tabIndex);                                                        // doesn't do anything if index = -1 (stays at first tab)
                
                // blink row, if roleId is given
                if (roleIdToBlink) {
                    $("#tabs tr[data-roleid=" + roleIdToBlink + "]").css("background-color", "lightgreen").animate({ 'background-color': "#FFFFFF" }, 1000);
                }

                // enable create new button again
                enableCreateNewButton();
            },
            error: function (xhr) {
                $("#tabsAndTablesContainer").empty();
                enableCreateNewButton();
                
                var errormsg = "failure loading the tabs and tables";
                if (xhr.status == 401) errormsg += " - perhaps you changed to a non-admin role in another tab?";
                alert(errormsg); // TODO
            }
        });
    }
    
    function refreshCachedNamesFromUAM() {
        disableCreateNewButton();
        $("#refresh-cached-names-button").addClass("working");
        $("#tabsAndTablesContainer").css("opacity", "0.5");

        $.ajax({
            type: "GET",
            url: "DashboardTemplate/RefreshCachedNamesFromUAM",
            data: {},
            dataType: "text",
            success: function (data, textStatus, xhr) {
                $("#refresh-cached-names-button").removeClass("working");
                reloadTabsAndTables(getCurrentlySelectedTabOrgId());
                //showSimpleDialog("Success", "<p>Successfully refreshed role names, organization names and organization ids from UAM.</p>");
                if (data != "") {
                    showSimpleDialog("Success", data);
                }
            },
            error: function (xhr) {
                $("#refresh-cached-names-button").removeClass("working");
                reloadTabsAndTables(getCurrentlySelectedTabOrgId());
                var errormsg = "Failed refreshing the cached names from UAM";
                if (xhr.status == 401) errormsg += " - perhaps you changed to a non-admin role in another tab?";
                showSimpleDialog("Error", errormsg);
            }
        });
        
    }
    
    // create a simple throw-away-dialog with some html
    function showSimpleDialog(title, html) {
        title = title.replace(/[\\"']/g, '\\$&').replace(/\u0000/g, '\\0'); // escape ",'
        html = html.replace(/[\\"']/g, '\\$&').replace(/\u0000/g, '\\0'); // escape ",'
        $("<div title='"+title+"'>"+html+"</div>").dialog({
            autoOpen: true,
            modal: true,
            //width: 700,
            //height: 300,
            buttons: {
                Ok: function() {
                    $(this).dialog("close");
                    $(this).remove(); // <- clean up the dom
                }
            }
        });
    }

    function disableCreateNewButton() {
        $(".top-button").addClass("disabled");
        $(".top-button-grayout-overlay").show();
        $(".top-button").blur();
    }
    
    function enableCreateNewButton() {
        $(".top-button").removeClass("disabled");
        $(".top-button-grayout-overlay").hide();
    }
    
    function parseAjaxErrorJSON(data) {
        var json;
        try {
            json = $.parseJSON(data);
            if (json == null) throw {}; // parseJSON can return null - for example if data=null (can f.e. happen due to canceled ajax calls on page unload)
        } catch (e) { // response was not json
            json = { title: "Internal Error", description: "Sorry, an unxpected error occurred.", detailedmessage: "" };
        }
        return json;
    }
    
    // Module containing the functions for the edit dialog (+ create dialog).
    var EditDialog = (function() {

        function openEditDialog(dashboardTemplateId) {
            $.ajax({
                type: "GET",
                url: "DashboardTemplate/EditDialog",
                data: {
                    id: dashboardTemplateId
                },
                success: function (data, textStatus, xhr) {
                    createEditDialog(data, "Edit");
                    
                    // TODO: This is duplicated. Make a populateRoleSelector() with no arguments?
                    var textfield = $(".editDialog #loginNameTextfield");
                    var loginId = textfield.val();
                    populateRoleSelector(loginId);
                },
                error: function (xhr) {
                    var errormsg = "failure loading the edit dialog";
                    if (xhr.status == 401) errormsg += " - perhaps you changed to a non-admin role in another tab?";
                    alert(errormsg); // TODO
                }
            });


        }

        function openCreateDialog() {
            // TODO: Make ajax
            //window.location.href = 'DashboardTemplate/CreateDialog';
            
            $.ajax({
                type: "GET",
                url: "DashboardTemplate/CreateDialog",
                data: {},
                success: function (data, textStatus, xhr) {
                    createEditDialog(data, "Create");
                    emptyRoleSelector();
                    emptyUserDashboardPreview();
                },
                error: function (xhr) {
                    var errormsg = "failure loading the create dialog";
                    if (xhr.status == 401) errormsg += " - perhaps you changed to a non-admin role in another tab?";
                    alert(errormsg); // TODO
                }
            });
        }

        var lastShownTab = -1;

        // does NOT reload the tabs and tables - only closes the dialog.
        function cancelEditDialog() {           
            $.event.trigger({ type: "editDialogClose" }); // custom event when dialog is being closed

            // enable all tabs again, and hide the edit tab.
            $("#tabs").tabs({ disabled: [] });
            $("#tabs").tabs("select", lastShownTab); // Change to last tab, from before entering the edit dialog.
            $("#edittab").hide();

            enableCreateNewButton();
        }
        
        function createEditDialog(html, tabText) {
            lastShownTab = $("#tabs").tabs('option', 'selected'); 

            // create dialog and show tab
            $("#editdialog").empty().html(html);
            $("#edittab a").text(tabText);
            $("#edittab").show();

            disableCreateNewButton();

            // select tab and disable all other tabs
            var tablength = $("#tabs").tabs('length');
            var disableTabs = [];
            for (var i = 0; i < tablength - 1; i++) {
                disableTabs.push(i);
            }
            $("#tabs").tabs("select", tablength - 1); // Change to last tab, which is the edit tab.
            $("#tabs").tabs({ disabled: disableTabs });            
        }

        function populateRoleSelector(loginId) {
            var currentlyShownLoginId = $("#roleSelector").data("loginId");
            if (loginId == currentlyShownLoginId) // Abort if we are already showing the roles for this loginId (To avoid an annoying (for the user) roleselector-reload, when the textfield changelistener fires, 
                return;                           // even though the roleselector already was showing the roles for that loginId. That extra reload made it ignore e.g. clicking a role when leaving the textfield.)

            var loadingSpinner = $(".editDialog #roleLoadingSpinner").show();
            loadingSpinner.show();
            emptyRoleSelector();

            getRolesForUser(loginId, function (json) {
                loadingSpinner.hide();

                // Create <selector> elements 
                var selectorElem = $("<select size='12' id='roleSelector' style='min-width:250px;_width:250px;'/>");
                selectorElem.data("loginId", loginId);
                $.each(json, function(i, org) {                      
                    var optgroupElem = $("<optgroup />", { label: org.orgName, 'data-orgid': org.orgId, style: "font-style:normal;" });
                    optgroupElem.appendTo(selectorElem);

                    $.each(org.roles, function (j, role) {
                        var optionElem = $("<option />", { value: role.roleId, text: role.roleName });
                        optionElem.appendTo(optgroupElem);
                    });
                });
                    
                // Add <selector> to document
                $(".editDialog #roleSelectorContainer").empty();
                $(".editDialog #roleSelectorContainer").append(selectorElem);
                
                // When in edit-mode, if we select the original username, this code also auto-select the original role (instead of selecting none). Hack. (TODO: Custom Events instead). ("roleSelectorPopulated") (+"roleSelected" to update the dashboardpreview)
                var originalRoleId = $(".editDialog #roleCopiedFrom").val();
                var originalLogin = $(".editDialog #loginCopiedFrom").val();
                var currentLogin = $(".editDialog #loginNameTextfield").val();
                var isOriginalLoginCurrent = originalLogin == currentLogin;
                
                if (isOriginalLoginCurrent) {
                    $("#roleSelector optgroup option[value=" + originalRoleId + "]").attr('selected', 'true'); // select the original role if the username is the original one
                    updateUserDashboardPreview(); // since we selected a role above, we also need to update the dashboard preview.
                }

            }, function (errorJson) {
                loadingSpinner.hide();
                //alert(errorJson.Title+"\n"+errorJson.Description); // TODO (don't open an alert, as it happens while you type (except on dev))
                emptyRoleSelector();
            });
        }
        
        function emptyRoleSelector() {
            $(".editDialog #roleSelectorContainer").html("<select size='12' id='roleSelector' style='min-width:250px;_width:250px;'></select>");
        }

        function getRolesForUser(loginId, success, failure) {
            $.ajax({
                type: "GET",
                url: "DashboardTemplate/GetRolesForUser",
                datatype: "json",
                data: {
                    loginId: loginId
                },
                success: function (json, textStatus, xhr) {
                    success(json);
                },
                error: function (xhr) {
                    failure(parseAjaxErrorJSON(xhr.responseText));
                }
            });
        }
        
        function updateDashboardTemplate() {
            var selectedRoleId = $("#roleSelector").val();
            if (selectedRoleId == null) {
                alert("you must select a role for the dashboard template");
                return;
            }

            var selectedRoleOrgId = $("#roleSelector option[value=" + selectedRoleId + "]").closest("optgroup").data("orgid");

            var loginId = $("#loginNameTextfield").val();
            var dashboardTemplateId = $("#dashboardTemplateId").val();
            var description = $("#descriptionTextarea").val();

            //alert("loginId: " + loginId + "\nroleid: " + selectedRoleId + "\ndashboardTemplateId: " + dashboardTemplateId);

            $.ajax({
                type: "POST",
                url: "DashboardTemplate/Update",
                data: {
                    id: dashboardTemplateId,
                    newLoginId: loginId,
                    newRoleId: selectedRoleId,
                    description: description
                },
                success: function (data, textStatus, xhr) {
                    $.event.trigger({ type: "editDialogClose" }); // custom event when dialog is being closed
                    
                    // Reload all data (all rows and tabs) to get a 100% updated state + pick the correct tab
                    reloadTabsAndTables(selectedRoleOrgId, selectedRoleId);
                },
                error: function (xhr) {
                    if (xhr.status == 401) {
                        alert("You do not have access previleges to edit a template - perhaps you changed to a non-admin role in another tab?");
                    } else {
                        var errorJson = parseAjaxErrorJSON(xhr.responseText);
                        alert(errorJson.Title + "\n" + errorJson.Description); // TODO
                    }
                }
            });

        }
        
        function createDashboardTemplate() {
            var selectedRoleId = $("#roleSelector").val();
            if (selectedRoleId == null) {
                alert("you must select a role for the dashboard template");
                return;
            }
            
            var selectedRoleOrgId = $("#roleSelector option[value=" + selectedRoleId + "]").closest("optgroup").data("orgid");

            var loginId = $("#loginNameTextfield").val();
            var description = $("#descriptionTextarea").val();

            //alert("loginId: " + loginId + "\nroleid: " + selectedRoleId);

            $.ajax({
                type: "POST",
                url: "DashboardTemplate/Create",
                data: {
                    loginId: loginId,
                    roleId: selectedRoleId,
                    description: description
                },
                success: function (data, textStatus, xhr) {
                    $.event.trigger({ type: "editDialogClose" }); // custom event when dialog is being closed
                    
                    // Reload all data (all rows and tabs) to get a 100% updated state + pick the correct tab
                    reloadTabsAndTables(selectedRoleOrgId, selectedRoleId);
                },
                error: function (xhr) {
                    if (xhr.status == 401) {
                        alert("You do not have access previleges to create a template  - perhaps you changed to a non-admin role in another tab?");
                    } else {
                        var errorJson = parseAjaxErrorJSON(xhr.responseText);
                        alert(errorJson.Title + "\n" + errorJson.Description);
                    }
                }
            });
        }
        
        // Inner module with keyDown-logic for the login-name textfield.
        // To save server resources, we do not load roles immediately on each keypress, but wait a little after a key has been pressed, in case the user is writing more chars. If he is we can abort the
        // request for the old-name, that the user wasn't gonna use for anything anyway. (in that case the web-request, UAM-webservice request and work done by the server  would have been for nothing).
        var LoginNameTextFieldKeyDown = (function() {
            var currentTimeoutId = null;
            function keyDownHandler() {
                if (currentTimeoutId != null) {
                    clearTimeout(currentTimeoutId);
                }
                currentTimeoutId = setTimeout(timeoutHandler, 500);
            }

            var oldTextfieldValue = null;
            function timeoutHandler() {
                currentTimeoutId = null;

                var loginId = $("#loginNameTextfield").val();
                if (loginId == oldTextfieldValue) return; // don't reload the data we're already displaying
                oldTextfieldValue = loginId;

                emptyUserDashboardPreview(); // also empty the dashboard preview
                populateRoleSelector(loginId); // TODO: Concurrency bug? Hanve a sequence id within this function?
            }
            
            // clean up when edit dialog is being closed
            $(document).on("editDialogClose", function() {
                if (currentTimeoutId != null) clearTimeout(currentTimeoutId);
                currentTimeoutId = null;
                oldTextfieldValue = null;
            });

            // the public function
            return { keyDownHandler: keyDownHandler};
        }());
        
        // Handle change events in the role selector: Load a dashboard preview (if the user exists - but he should exist if the role-selector shows any entries (small room for error due to timeout, but should be ok))
        // TODO: Sequence to avoid concurrency errors!
        function updateUserDashboardPreview() {
            var selectedRoleId = $("#roleSelector").val();
            var selectedLoginId = $("#loginNameTextfield").val();

            if (selectedRoleId == null || selectedLoginId == "") { emptyUserDashboardPreview(); return; }

            var previewContainer = $("#userDashboardPreview");
            previewContainer.empty();
            
            $.ajax({
                type: "GET",
                url: "DashboardTemplate/UserDashboardPreview",
                data: {
                    loginId: selectedLoginId,
                    roleId: selectedRoleId
                },
                success: function (data, textStatus, xhr) {
                    previewContainer.empty();
                    previewContainer.html(data);
                },
                error: function (xhr) {
                    // TODO
                    previewContainer.html("<center style='color:red'>Error showing dashboard preview</center>");
                }
            });
        }
        
        function emptyUserDashboardPreview() {
            var previewContainer = $("#userDashboardPreview");
            previewContainer.empty();
            previewContainer.html("<center><i>To show a preview of a dashboard, select a user and a role.</i></center>");
        }

        // public functions
        return {
            openEditDialog: openEditDialog,
            openCreateDialog: openCreateDialog,
            cancelEditDialog: cancelEditDialog,
            populateRoleSelector: populateRoleSelector,
            updateDashboardTemplate: updateDashboardTemplate,
            createDashboardTemplate: createDashboardTemplate,
            loginNameTextFieldKeyDownHandler: LoginNameTextFieldKeyDown.keyDownHandler,
            updateUserDashboardPreview: updateUserDashboardPreview
        };
    }());

});