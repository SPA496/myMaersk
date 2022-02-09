$(function () {

    // http: //blog.lysender.com/2011/08/setting-session-only-cookie-via-javascript/
    function invalidateBFCache() {
        document.cookie = "invalidatebfcache=1; path=/";
    }

    // Error dialog
    $("#dashboard-error-dialog").dialog({
        autoOpen: false,
        title: 'An error has occured',
        modal: true,
        resizable: false,
        draggable: false,
        minHeight: 0
    });

    $("#widgetlist").dialog({
        autoOpen: false,
        title: 'Add New Widget',
        modal: true,
        resizable: false,
        draggable: false,
        dialogClass: 'MyDamcoDialog'
    });
    $("#addwidgetlink").click(function () {
        var w = 926;
        var h = 96 + Math.floor((2 + $("#widget-add-widget-list").children().length) / 3) * 89; // 366

        $('#widgetlist').dialog("option", {
            width: w,
            height: h
        }).dialog('open');
        return false;
    });

    // KV: TODO: See if "helper" can help us show dragged element.
    $('.widget-add').sortable({
        connectWith: ".dashboardcolumn",
        //handle: ".widget-add-drag-head",
        placeholder: 'widget-head newwidget-placeholder',
        forcePlaceholderSize: true,
        cursor: 'move',
        start: function (event, ui) {
            var img = '<img src=""/>';
            ui.placeholder.html('<center>Add ' + $("span", ui.item).text() + '</center>');
            $("#widgetlist").dialog("close");
        }

    });

    // Make items on the dashboard sortable/dragable    
    $('.dashboardcolumn').sortable({
        connectWith: ".dashboardcolumn",
        handle: ".widget-head",
        cancel: "a,input",
        forcePlaceholderSize: true,
        placeholder: 'widget-placeholder',
        opacity: 0.5,
        revert: true,

        // TODO: Test and enable code again
        //Cancel any renaming of widgets that may be in progress during widget re-ordering.
        /* stop: function (event, ui) {
        $(this).find(".widget-head input").trigger('blur');
        }, */

        // Invoked when the user reorganizes the widgets.
        // KV: Invoked once in list where a widget is moved from, and once in list where it is moved to.
        update: function (event, ui) {


            if (ui.item.data("newwidgetid") != undefined) { // KV: Newly added widget!
                var placeholder = $('<div id=""/>');
                ui.item.after(placeholder);
                $(this).addWidget(ui.item.data("newwidgetid"), placeholder);
                ui.item.detach();
                ui.sender.append(ui.item)

                return;
            }

            var widgetIdList = $(this).sortable('toArray').join(",").match(/[0-9]+/g);
            var widgetId = ui.item.attr("id").match("\\d+$")[0];

            // Skip if this is the source list as we don't need to update that
            if ($.inArray(widgetId, widgetIdList) == -1)
                return;

            $(this).SendNewColumnOrder();


            // Cancel any renaming of widgets that may be in progress during widget re-ordering.
            // TODO: Enable and test code
            //$(this).find(".widget-head input").trigger('blur');
        },
        stop: function (event, ui) {
            if (ui.item.data("uid") == "performancechart")
                getChartObject(ui.item.attr('id')).updateData();
        }
    });

    $.fn.showErrorDialog = function (response) {
        var response = $.parseJSON(response);
        if (response == null) response = { Title: "Internal Error", Description: "Sorry, an unxpected error occurred."}; // parseJSON returns null if e.g. response = null. (can happen on page unload)
        var obj = $(this);
        obj.dialog("option", "title", response.Title)
            .children("#error-description")
            .empty()
            .append(response.Description)
            .parent()
            .dialog("open");
    }

    $.fn.SendNewColumnOrder = function () {
        var dashboardColumnId = this.data("column");
        widgetIdList = $(this).sortable('toArray').join(",").match(/[0-9]+/g);

        invalidateBFCache();
        $.ajax({
            type: "POST",
            url: "Dashboard/SetWidgetOrder",
            traditional: true,
            data: {
                dashboardColumn: dashboardColumnId,
                widgetList: widgetIdList
            }
        });
    }

    // TODO: Make sure we have added all script and resource tags to LoadedStyleSheetResources and LoadedScriptsResources, fx. static widget might have something we don't know about
    /* var isThere = false;
    $("script").each(function() {
    if ($(this).attr("src") == sn) {
    isThere = true;  
    // this works also with injected scripts!!
    }
    }); */

    $.fn.initWidget = function () {
        var element = this;

        var configurationDiv = $("div.configuration", element);
        var configurationLink = $("a.configuration", element);

        configurationLink.unbind("click");
        configurationLink.click(function () {
            configurationDiv.toggle();
        });

        var deleteLink = $("a.delete", element);
        deleteLink.unbind("click");
        deleteLink.click(function () {
            var instanceId = widgetNameToId(element.attr("id"));
            $.ajax({
                'url': "Dashboard/RemoveWidgetInstance/" + instanceId,
                'success': function () {
                    var elm = $(element);
                    elm.fadeOut(500, function () { elm.remove(); });                  
                    if (typeof(PiwikUtil) === "object") PiwikUtil.widget.trackRemoveWidget(instanceId, element.data("uid")); // track the event
                },
                'error': function (xhr) {
                    $("#dashboard-error-dialog").showErrorDialog(xhr.responseText);
                }
            });
        });
    }

    $.fn.addWidget = function (widgetId, afterElement) {
        var obj = $(this);
        $.ajax({
            'type': "POST",
            'url': "Dashboard/AddWidget",
            'traditional': true,
            'data': {
                widgetId: widgetId,
                column: obj.data("column")
            },
            'success': function (data, textStatus, xhr) {

                // Append the element to the document.
                afterElement.after(data.data);
                afterElement.remove();

                // JQuery query for the newly created element.
                var element = "$(\"#widget" + data.id + "\")";

                // Initalizes the widget on the dashboard - e.g. add links for configuraiton and deletion.
                eval(element).initWidget();

                // Calls the specific initalization function for the added widget. For example element.widget_rsssnews() for rssnews-widget.
                var fun = element + ".widget_" + data.widgetTemplateName.toLowerCase() + "();";
                eval(fun);

                obj.SendNewColumnOrder();

                // track the event
                if (typeof(PiwikUtil) === "object") PiwikUtil.widget.trackAddWidget(data.id, eval(element).data("uid"));
            },
            'error': function (xhr, textStatus, errorThrown) {
                $("#dashboard-error-dialog").showErrorDialog(xhr.responseText);
            }
        });
    }

    $("div.widget").each(function (index, element) {
        $(element).initWidget();
    });

    // TODO: Is this used anywhere?
    $(".widgetAddLink").each(function (index, element) {

        $(element).click(function () {
            $("#dashboardcolumn0").addWidget(element.id);
            return false;
        });
    });
});


function widgetNameToId(str) {
    var id = str;
    if (typeof str == "string") {
        id = parseInt(str.replace("widget", ""))
    }
    return id;
}