
$(function () {


    // KV: http://stackoverflow.com/questions/726334/asp-net-mvc-jsonresult-date-format
    function MVCDateToDateObject(date) {
        var rv = new Date(parseInt(date.replace("/Date(", "").replace(")/", ""), 10));
        return rv;
    }

    var disabledFunctionsIntervalID = setInterval(disableFunctions, 1000 * 60 * 15);
    function disableFunctions() {

        var url = mydamcoBasepath + 'Navigation/DisabledFunctions';

        // DisabledFunctions is called using AJAX every 15 minutes. Users are logged out of ADFS after 1 hour of inactivity.
        //   If user has been idling for 1 hour and has thus been logged out of ADFS, each of these AJAX requests to DisabledFunctions will create a *new* cookie for myDamco (The ADFS-Module creates the cookie, and tries to redirect to ADFS login 
        // page, but due to same-origin policy, this redirect is blocked by the browser - and the cookie is never deleted again). These cookies will pile up over time, and after enough hours of idling, all requests (which includes all cookies) to 
        // myDamco will be too big for the server to handle, resulting in "400 Bad Request - Request too long" for all requests.
        //   This function is used to stop sending these AJAX requests if the user has been logged out and thereby stop the cookies from piling up. (It stops the timer. Is called when an error occurs after trying to send JSONP - might be due to same-origin
        // violation but might also be because of something else). This won't delete the cookies, but will prevent them from piling up indefinitely. (Cookies names starts with _AdfsWctx) 
        function stopTimer() {
            clearInterval(disabledFunctionsIntervalID);
        }

        function fallBackToJsonp(jqXHR, textStatus, errorThrown) {
            $.ajax({
                dataType: "jsonp",
                jsonp: "callback",
                url: url,
                cache: false,
                success: processDisabledFunctionsResponse,
                error: stopTimer
            });
        }

        // Note: This code can be invoked from the external menu (cross domain). It first attempts to do a CORS cross-domain ajax request.
        //       If the cross-domain AJAX request fails (CORS is not supported by all browsers + can fail for localhost), it falls back to JSONP (supported by all browsers).
        $.ajax({
            dataType: "json",
            url: url,
            cache: false,
            xhrFields: {
                withCredentials: true // cookies are not normally sent along with cross-domain AJAX requests, so this is needed to prove you are logged in. (Only sends/receives cookies to the remote domain (=MyDamco))
            },
            success: processDisabledFunctionsResponse,
            error: fallBackToJsonp
        });
    }

    disableFunctions();
    
    function processDisabledFunctionsResponse(json) {

        // KV: Lets reset everything and then readd whatever the service tells us is needed ATM.
        $(".downtimewarning").remove();
        $(".downtimedisabled").removeClass("downtimedisabled");
        $("a.downtimedisabled").unbind("click");
        $(".widget-add").sortable({ disabled: false });
        $(".widget .widget-content").show();

        $(json).each(function (i, downtime) {
            var appFilter = function (_, x) { return $(x).data("uamapp") == downtime.App; };
            var funcFilter = function (_, x) { return $(x).data("uamfunc") == downtime.Func; };

            var totalFilter = function (_, x) { return appFilter(_, x) && funcFilter(_, x); };
            if (downtime.Func == "")
                totalFilter = function (_, x) { return appFilter(_, x) };


            var link = $(".MyDamcoMenu_topbarnavigation a").filter(totalFilter);
            var message = downtime.Message;
            if (message == null)
                message = "Disabled for unknown reasons.";

            var from = MVCDateToDateObject(downtime.From);
            var to = MVCDateToDateObject(downtime.To);

            var dateformatUtc = "dddd, MMMM Do, H:mm";       // used when displaying dates in UTC
            var dateformatLocal = "dddd, MMMM Do, H:mm UTCZ";  // used when displaying dates in local time (in mouseovers)

            var warningReplaced = message.replace(/\n/g, '<br />')
                                            .replace("[[timeleft]]", "<b>" + moment(to).utc().fromNow() + "</b>")
                                            .replace("[[from]]", "<span title='Local time: " + moment(from).format(dateformatLocal) + "'><b>" + moment(from).utc().format(dateformatUtc) + " UTC</b></span>")
                                            .replace("[[to]]", "<span title='Local time: " + moment(to).format(dateformatLocal) + "'><b>" + moment(to).utc().format(dateformatUtc) + " UTC</b></span>");

            var warningSpan = "<span class='downtimewarning'>" + warningReplaced + "</span>";

            link.addClass("downtimedisabled");
            link.append(warningSpan);


            // KV: NOTE: This way of disabling links asumes that there are not already click-bindings on the menu-links. If we make those, we need to make special handling
            link.click(function () {
                return false;
            });

            var widgets = $(".widget-add").filter(totalFilter);
            widgets.each(function (_, widget) {
                var w = $(widget);
                w.sortable({ disabled: true });

                var draghead = $(".widget-add-drag-head", w);
                draghead.addClass("downtimedisabled");
                draghead.append(warningSpan);

            });

            var instances = $(".widget").filter(totalFilter);
            instances.each(function (_, instance) {
                var content = $(".widget-content", instance);
                content.hide();

                $(instance).append(warningSpan);
            });
        });
    }

    $.fn.dialog_changeRole = function () {
        return this.each(function () {
            var $this = $(this);


            var renderRoles = function (roles) {
                var template = $(".rolechange-template", $this);
                var container = $(".role-accordion", $this);

                // Group by organization
                var orgGroups = {};
                $.each(roles, function (key, val) {
                    var group = orgGroups[val.Organization];
                    if (group) {
                        group[group.length] = val;
                    }
                    else {
                        orgGroups[val.Organization] = [val];
                    }
                });

                // Render
                $.each(orgGroups, function (orgName, orgRoles) {
                    // Company header
                    var header = $(".organization-header", template).clone();
                    header.html(orgName);

                    // Role buttons
                    var content = $(".roles-content", template).clone();
                    $.each(orgRoles, function (index, role) {
                        content.append(
                            $(".template", content).clone()
                                .html(role.Name)
                                .attr("class", "role-button")
                                .click(function () {
                                    if (typeof(PiwikUtil) === "object" && PiwikUtil.trackChangeRoleEvent) {
                                        PiwikUtil.trackChangeRoleEvent(role.Id, role.Name, orgName);
                                    }
                                    var url = mydamcoBasepath + "Dashboard/UAMChangeRole/" + role.Id;
                                    var script = document.createElement('script');
                                    script.type = 'text/javascript';
                                    script.src = url;
                                    $("body").append(script);
                                })
                        );
                    });
                    content.children(".template").remove();

                    container.append(header)
                        .append(content);
                });

                $(".role-button", $this).button();
                $(".role-accordion", $this).accordion({ autoHeight: false });


                $this.dialog({
                    autoOpen: false,
                    title: 'Change Role',
                    modal: true,
                    resizable: false,
                    draggable: false,
                    width: 290,
                    position: {
                        my: 'top',
                        at: 'top',
                        of: $('#MyDamcoMenu_changerolelink')
                    },
                    close: function (event, ui) {
                        // Hack so the dialog is now shown behind the two frames in reporting
                        if ($.browser.msie && $("#folder_frame").length > 0) {
                            $("table.columns").show();
                        }
                    },
                    dialogClass: 'MyDamcoDialog'
                });
            }

            $("#MyDamcoMenu_changerolelink").click(function () {
                // Hack so the dialog is now shown behind the two frames in reporting
                if ($.browser.msie && $("#folder_frame").length > 0) {
                    $("table.columns").hide();
                }
                $this.dialog('open');
            });

            renderRoles($this.data("roles").roles);
        });
    };

    $("#MyDamcoMenu_Logout_message").dialog({
        autoOpen: false,
        modal: true,
        closeOnEscape: false,
        resizable: false,
        width: 410,
        height: 60,
        draggable: false,
        open: function (event, ui) {
            $("a.ui-dialog-titlebar-close", event.target.parentNode).hide();
        }
    });

    $("#MyDamcoMenu_topbar_logout_button").click(function () {

        $("#MyDamcoMenu_Logout_message").dialog("open");

    });

    $("#MyDamcoMenu_roledialog").dialog_changeRole();

    /* IE6 fix, which supports :hover only on links */
    $('ul#MyDamcoMenu_navigation li a').hover(function () {
        $(this).addClass ("hover");
    }, function () {
        $(this).removeClass ("hover");
    });

    $('ul#MyDamcoMenu_navigation li a:not(.downtimedisabled)').click(function() {
        var linkName = $(this).text();
        if (typeof(PiwikUtil) === "object" && PiwikUtil.trackNavigationLinkClickEvent) {
            PiwikUtil.trackNavigationLinkClickEvent(linkName);
        }
    });
});