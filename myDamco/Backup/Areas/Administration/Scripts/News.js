// Globals. Row is set if we are currently editing a Newsitem. Category is set if we are currently creating a Newsitem.
var row;
var category;
var DescChangedByUser = false;


function AddError(errorMsg) {
    var error = $("<div class='adminerror ui-state-error ui-corner-all'>"+ errorMsg+"</div>");
    error.click(function () { $(this).fadeOut(300, function () { $(this).remove(); }); });
    $("#newserrors").append(error);
}

$(function () {
    /* Set data-values of a HTML-row (not necessarily in the DOM).
       From, to, createdAt, updatedAt should be in miliseconds (not seconds) after the UNIX Timestamp epoch.
       (CreatedBy, CreatedAt, UpdatedBy, UpdatedAt are optional) */
    function setRow(row, title, from, to, description, content, downtimes, createdBy, createdAt, updatedBy, updatedAt) {

        row.data("title", title);
        row.data("from", from);
        row.data("to", to);
        row.data("description", description);
        row.data("content", content);
        row.data("downtimes", downtimes);

        row.data("createdby", createdBy);
        row.data("createdat", createdAt);
        row.data("updatedby", updatedBy);
        row.data("updatedat", updatedAt);
        
        updateRowFromData(row);
    }

    /* Update the HTML-row from another HTML-row (not necessarily in the DOM) */
    function updateRowFromData(row) {

        $("#title", row).text(row.data("title"));
        $("#from", row).text(dateToDisplay(new Date(row.data("from"))));
        $("#to", row).text(dateToDisplay(new Date(row.data("to"))));
        $("#description", row).text(row.data("description"));

        $("#createdby", row).text(row.data("createdby") ? row.data("createdby") : "");
        $("#updatedby", row).text(row.data("updatedby") ? row.data("updatedby") : "");
        $("#createdat", row).text(row.data("createdat") ? dateToDisplay(new Date(row.data("createdat"))) : "");
        $("#updatedat", row).text(row.data("updatedat") ? dateToDisplay(new Date(row.data("updatedat"))) : "");
    }

    function setEditor(title, from, to, description, content, downtime, downtimes) {
        $("#editnewsitem .title").val(title);
        $("#editnewsitem #summary_field").val(description);
        tinyMCE.activeEditor.setContent(content);

        DescChangedByUser = description != getSummary();
        setColorsForSummaryText();

        if (from == "") {
            $("#editnewsitem #from_picker").datepicker('setDate', UnixTimeToUTCTime(new Date().getTime()));
        } else {
            $("#editnewsitem #from_picker").datepicker('setDate', UnixTimeToUTCTime(from));
        }
        if (to == "")
            $("#editnewsitem #to_picker").val("");
        else 
            $("#editnewsitem #to_picker").datepicker('setDate', UnixTimeToUTCTime(to));

        $("#editnewsitem #downtime tbody tr").not(".template").remove();
        if (downtime) {
            for (var i = 0; i < downtimes.length; i++) {
                addDowntimeToEditor(downtimes[i].From, downtimes[i].To, downtimes[i].UAMApplication, downtimes[i].UAMFunction, downtimes[i].Message);
            }
            setDowntimeButtons();
            $("#downtime").show();
        }
        else
            $("#downtime").hide();

    }

    // Init the JQuery elements.
    {
        $("#tabs").tabs();

        $("#editnewsitem #from_picker").datetimepicker({
            dateFormat: "dd-mm-yy",
            timeFormat: "HH:mm",
            stepMinute: 5,
            useLocalTimezone: false,
            defaultTimezone: '+0000'
        });
        $("#editnewsitem #to_picker").datetimepicker({
            dateFormat: "dd-mm-yy",
            timeFormat: "HH:mm",
            stepMinute: 5,
            useLocalTimezone: false,
            defaultTimezone: '+0000'
        });

        tinyMCE.init({
            mode: "textareas",
            editor_selector: "mceEditor",
            theme: "simple",
            setup: function (ed) {
                ed.onKeyUp.add(function (ed, e) {
                    if (!DescChangedByUser) {
                        $("#summary_field").val(getSummary());
                        setColorsForSummaryText();
                    }
                });
            }
        });

        $("#editnewstab").hide();
    }

    $("#summary_field").keyup(function () {
        DescChangedByUser = true;
        if ($("#summary_field").val() == "") {
            $("#summary_field").val(getSummary());
            DescChangedByUser = false;
        }
        setColorsForSummaryText();
    });

    function setColorsForSummaryText() {
        if (DescChangedByUser)
            $("#summary_field").css("color", "black");
        else
            $("#summary_field").css("color", "grey");
    }

    function getSummary() {
        text = strip(tinyMCE.activeEditor.getContent());
        if (text.length > 300)
            text = jQuery.trim(text.substring(0, 297)) + "...";
        return text;
    }

    // KV: http://stackoverflow.com/questions/822452/strip-html-from-text-javascript
    function strip(html) {
        var tmp = document.createElement("DIV");
        tmp.innerHTML = html;
        return tmp.textContent || tmp.innerText || "";
    }

    // Click events for news overview. Function also used after adding new newsitems.
    function InitNewsOverviewEvents() {
        $(".newsitemrow").each(function () { updateRowFromData($(this)) });

        $(".edit-row").unbind('click')
        $(".edit-row").click(function () {
            enableEditTab();

            row = $(this.parentElement.parentElement);
            category = null;

            var downtime = $(this).closest("div").data("downtime") == "True";
            var downtimes = row.data("downtimes");

            if (downtimes == null)
                downtimes = [];

            setEditor(row.data("title"), row.data("from"), row.data("to"), row.data("description"), row.data("content"), downtime, downtimes);
        });

        $(".createNewNewsItem").unbind('click')
        $(".createNewNewsItem").click(function () {
            enableEditTab();

            category = $(this.parentElement.parentElement);
            row = null;

            var downtime = $(this).parent().parent().data("downtime") == "True";
            setEditor("", "", "", "", "", downtime, []);
        });

        $(".delete-row").unbind('click')
        $(".delete-row").click(function () {
            if (!confirm("This will delete the news item."))
                return;

            var row = $(this.parentElement.parentElement);

            var dts = row.data("downtimes");
            if(dts != undefined && dts.length != 0)
            if (!confirm("There are scheduled downtimes associated with this announcement. Please confirm that you also want to delete these."))
                return;

            var tbody = $(this.parentElement.parentElement.parentElement);
            $.ajax({
                type: "POST",
                url: "News/DeleteNewsItem",
                traditional: true,
                data: {
                    id: row.data("pkey")
                },
                success: function () {
                    row.remove();
                    if ($("tr", tbody).length == 2) {
                        $("#noitems", tbody).show();
                    }
                },
                error: function () {
                    AddError("Deletion failed unexpectedly. Please verify that the news-item is actually deleted.");
                }
            });
        });
    }
    InitNewsOverviewEvents();

    // Click events for editor
    {
        $("#canceledit").click(function () {
            disableEditTab();
        });

        $("#savenewsitem").click(function () {
            if (saveRow())
                disableEditTab();

        });

        $("#clear_to_picker").click(function () {
            $("#to_picker").val("");
        });
    }

    // User has clicked "save" in the editor. Now insert or update it in database and on layout. 
    function saveRow() {

        var from = $("#editnewsitem #from_picker").datetimepicker("getDate");
        if (from == null || from == undefined) {
            alert("No from time filled out");
            return false;
        }
        from = UTCTimeToUnixTime(from);

        var isofrom = ISODateString(from);

        var to = "";
        if ($("#editnewsitem #to_picker").val() != "")
            to = UTCTimeToUnixTime($("#editnewsitem #to_picker").datetimepicker("getDate"));
        var isoto = ISODateString(to);

        if ($("#editnewsitem .title").val() == "") {
            alert("Please fill out the title");
            return false;
        }

        if ($("#editnewsitem #summary_field").val() == "") {
            alert("Please fill out the summary field");
            return false;
        }
        
        if (tinyMCE.activeEditor.getContent() == "") {
            alert("Please fill out the content of the news item.");
            return false;
        }

        var dt = getDowntimes();

        var downTimesISO = $.map(dt, function (x) {
            return {
                From: ISODateString(x.From),
                To: ISODateString(x.To),
                UAMApplication: x.UAMApplication,
                UAMFunction: x.UAMFunction,
                Message: x.Message
            };
        });

        if (row != null) { // Update the row
            var key = row.data("pkey");

            // to make the UI update seem snappy: do a quick display of the row (without the values from the server yet) before doing the ajax request. Can be removed if wished.
            setRow(row, $("#editnewsitem .title").val(), from, to, $("#editnewsitem #summary_field").val(), tinyMCE.activeEditor.getContent(), dt);

            $.ajax({
                type: "POST",
                url: "News/UpdateNewsItem",
                contentType: "application/json",
                data: JSON.stringify({
                    id: key,
                    title: $("#editnewsitem .title").val(),
                    from: isofrom,
                    to: isoto,
                    description: $("#editnewsitem #summary_field").val(),
                    body: tinyMCE.activeEditor.getContent(),
                    downtime: downTimesISO
                }),
                success: function(data) {
                    setRow(row, $("#editnewsitem .title").val(), from, to, $("#editnewsitem #summary_field").val(), tinyMCE.activeEditor.getContent(), dt, row.data("createdby"), row.data("createdat"), data.updatedBy, data.updatedAt);
                },
                error: function () {
                    AddError("Insertion failed unexpectedly. Please check if the news-item was added.");
                }
            });
        }
        else { // Insert the row
            var categoryKey = category.data("key");

            $.ajax({
                type: "POST",
                url: "News/InsertNewsItem",
                contentType: "application/json",
                data: JSON.stringify({
                    NewsCategory_Id: categoryKey,
                    title: $("#editnewsitem .title").val(),
                    from: isofrom,
                    to: isoto,
                    description: $("#editnewsitem #summary_field").val(),
                    body: tinyMCE.activeEditor.getContent(),
                    downtime: downTimesISO
                }),
                success: function (data) {
                    var tbody = $("tbody", category);
                    var template = $("#template", category);
                    var newRow = template.clone();
                    newRow.attr("id", "");
                    newRow.data("pkey", data.id);
                    tbody.append(newRow);

                    setRow(newRow, $("#editnewsitem .title").val(), from, to, $("#editnewsitem #summary_field").val(), tinyMCE.activeEditor.getContent(), dt, data.createdBy, data.createdAt, undefined, undefined);
                    newRow.show();

                    $("#noitems", tbody).hide();
                    InitNewsOverviewEvents();
                },
                error: function () {
                    AddError("Insertion failed unexpectedly. Please check if the news-item was added.");
                }

            });
        }
        return true;

    }

    // Tab magic below
    {
        var lastShownTab = -1;
        var enableEditTab = function () {
            lastShownTab = $("#tabs").tabs('option', 'selected');

            $("#editnewstab").show();

            var tablength = $("#tabs").tabs('length');
            var disableTabs = [];
            for (var i = 0; i < tablength - 1; i++) {
                disableTabs.push(i);
            }
            $("#tabs").tabs("select", tablength - 1); // Change to last tab, which is the edit news tab.
            $("#tabs").tabs({ disabled: disableTabs });

        }

        var disableEditTab = function () {
            $("#tabs").tabs({ disabled: [] });
            $("#tabs").tabs("select", lastShownTab); // Change to last tab, which is the edit news tab.
            $("#editnewstab").hide();
        }
    }

    // Downtime magic
    {

        $("#editnewsitem #downtime_from").datetimepicker({
            dateFormat: "dd-mm-yy",
            timeFormat: "HH:mm",
            stepMinute: 5,
            useLocalTimezone: false,
            defaultTimezone: '+0000'
        });
        $("#editnewsitem #downtime_to").datetimepicker({
            dateFormat: "dd-mm-yy",
            timeFormat: "HH:mm",
            stepMinute: 5,
            useLocalTimezone: false,
            defaultTimezone: '+0000'
        });

        $("#app").autocomplete({ source: $("#app").data("source") });
        $("#func").autocomplete({ source: $("#func").data("source") });


        $("#adddowntimedialog").dialog({ title: "Downtime", autoOpen: false, width: 400 });
        $("#adddowntime").click(function () {
            $("#adddowntimedialog").dialog('open');
            $("#adddowntimedialog").data("editrow", null);
            setDownTimeDialog(new Date(), new Date(), "", "", "This item is currently disabled due to scheduled maintenance. It is scheduled to be back online [[timeleft]].\n\nFrom: [[from]]\nTo: [[to]]"); // New downtime here.
            return false;
        });



        $("#adddowntimedialog #savedowntime").click(function () {
            // TODO 
            var toDate = UTCTimeToUnixTime($("#downtime_to").datepicker("getDate"));
            var fromDate = UTCTimeToUnixTime($("#downtime_from").datepicker("getDate"));
            var message = $("#downtime_message").val();

            if ($("#adddowntimedialog").data("editrow") == null) {
                addDowntimeToEditor(fromDate, toDate, $("#adddowntimedialog #app").val(), $("#adddowntimedialog #func").val(), message);
            }
            else {
                setDowntimeRow($("#adddowntimedialog").data("editrow"), fromDate, toDate, $("#adddowntimedialog #app").val(), $("#adddowntimedialog #func").val(), message)
            }
            $("#adddowntimedialog").dialog('close');
            setDowntimeButtons();
        });
    }

    // Datepicker "Now" button magic
    {
        forceDatePickerNowButtonToUseUTC();
    }

    function setDownTimeDialog(from, to, app, func, message) {
        $("#app").val(app);
        $("#func").val(func);
        $("#downtime_message").val(message);
        $("#downtime_from").datepicker('setDate', UnixTimeToUTCTime(from));
        $("#downtime_to").datepicker('setDate', UnixTimeToUTCTime(to));
    }

    function setDowntimeButtons() {
        $(".edit-downtime").unbind();
        $(".edit-downtime").click(function () {
            var row = $(this).closest("tr");
            var cols = $("td", row);
            var uam = $(cols[3]).text().split(":");
            setDownTimeDialog(row.data("from"), row.data("to"), uam[0], uam[1], row.data("message"));
            $("#adddowntimedialog").data("editrow", row);
            $("#adddowntimedialog").dialog('open');
        });

        $(".delete-downtime").unbind();
        $(".delete-downtime").click(function () {
            if (!confirm("Do you want to delete?"))
                return;

            $(this).closest("tr").remove();
        });
    }

    function addDowntimeToEditor(fromDate, toDate, uamapp, uamfunc, message) {
        var newRow = $("#downtime .template").clone();

        setDowntimeRow(newRow, fromDate, toDate, uamapp, uamfunc, message);
        newRow.attr({ 'class': 'new' });
        newRow.show();
        $("#downtime tbody").append(newRow);
    }

    function setDowntimeRow(row, fromDate, toDate, uamapp, uamfunc, message) {
        var uam = uamapp + ":" + uamfunc

        row.data("from", fromDate);
        row.data("to", toDate);
        row.data("message", message);

        var cols = $("td", row);

        $(cols[1]).text(dateToDisplay(new Date(fromDate)));
        $(cols[2]).text(dateToDisplay(new Date(toDate)));
        $(cols[3]).text(uam);
    }

    function getDowntimes() {
        var downtimesDOM = $("#downtime tbody tr");
        var downtimes = [];
        var downtimesData = [];

        for (var i = 1; i < downtimesDOM.length; i++) {
            var downtimeDOM = downtimesDOM[i];
            var cols = $("td", downtimesDOM[i]);

            var uam = $(cols[3]).text().split(":");

            var fromDate = $(downtimeDOM).data("from");
            var toDate = $(downtimeDOM).data("to");
            var message = $(downtimeDOM).data("message");

            downtimes.push({
                From: fromDate,
                To: toDate,
                UAMApplication: uam[0],
                UAMFunction: uam[1],
                Message: message
        });
       }

        return downtimes;
    }
});