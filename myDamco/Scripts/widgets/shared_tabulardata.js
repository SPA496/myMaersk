/**Setup a widget displaying tabular data. This code is shared by multiple widget types that displays tabular data.
 * The widget-type specific configuration is given to the function in the "config" parameter and the DOM-element of the widget is given by the implicit "this" parameter.
 *
 * @param this   The DOM-element of the widget.
 * @param config The config object must/can contain the following fields: 
 *                   Required fields (and expected types): 
 *                      textNoEntriesFound (string)     = Text displayed if there are no entries to show
 *                      textInvalidData (string)        = Text displayed if the data received from the server is not as expected
 *                      expectedDataLength (number)     = number of <td>s expected per <tr> in the data received from the server. An error is shown if the data from the server doesn't match.
 *                      createRowDisplayData (function) = a callback function, which is called each time a row is to be created in the DOM, from the Render() function below. 
 *                                                        It is called for each row in the data received from the server, and converts this row-data into another format, which is used in the Render
 *                                                        function to display the data. (This other format contains more widget/display specific information, such as link-urls, 1 elem per cell, etc)
 *
 *                                                        The callback gets the array of <td>s for the current <tr> from the server-data as parameter (the input-row). (optionally also previous row)
 *                                                        It must return an array, with one element per output-row for the given input-row ("recent searches" outputs multiple rows per input row).
 *                                                        Each element (row) in this array is another array, representing an output-row. This array has one element per cell for that row, on the 
 *                                                        form [{text:"xxx", href:"http://yyy"}, ..., {text:"zzz", href:"http://www"}].
 *
 *                                                        (The href can be null (or omitted/undefined) if the cell should not be displayed as a link)
 *                                                        (A cell can also have a th:boolean field, to make the cell render as <th> instead of <td>)
 *                                                        (Can return multiple output rows per input row, which is why it returns an array of arrays. This is used by "recent searches".)
 *                   Optional fields (and expected types): 
 *                      rowCssClass (string)            = css-class to add to each row element in the table
 */
window.widget_shared_tabulardata = function (config) {

    var $this = $(this);
    var uid = $this.data("uid");
    var baseconf = $this.data("baseconf");
    var serviceUrl = $this.data("serviceurl");
    var targetUrl = null;
    var widgetInstanceId = $this.attr("id");

    if (baseconf.hasOwnProperty("targeturl")) targetUrl = baseconf.targeturl;

    // Render rows with the data
    function Render(data) {
        var container = $(".tableBody", $this);

        // remove old content
        removeContent();

        var isDataEmpty = data == null || !data.widget || !data.widget.tr;
        if (!isDataEmpty) {

            // Output response time (if it exists in the data)
            var responseTime = data.widget["@response-time"];
            if (responseTime) {
                $(".responseTime", $this).html(responseTime);
                $(".responseTimeContainer", $this).show();
            }

            // Output main content
            var prevRowData = null; // used by recent searches
            var N = 0; // Row counter
            $.each(data.widget.tr, function (index, val) {
                var dataIsValid = val && val.td && val.td.length >= config.expectedDataLength;

                if (dataIsValid) {

                    // Get row-data to display, by calling the callback. (Converts the row-data received from the server (val.td) into a different format.)
                    var rowsDisplayData = config.createRowDisplayData(val.td, prevRowData); 
                    prevRowData = val.td;

                    for (var outrow = 0; outrow < rowsDisplayData.length; outrow++) { // the "recent searches" widget can return multiple output rows per input row
                        var rowDisplayData = rowsDisplayData[outrow];

                        // Create row element and cell elements from the data returned by the callback.
                        var row = $("<tr>");
                        if (config.rowCssClass) row.addClass(config.rowCssClass);

                        for (var i = 0; i < rowDisplayData.length; i++) {
                            var cellDisplayData = rowDisplayData[i];

                            if (cellDisplayData.even && N % 2 == 1 || cellDisplayData.odd && N % 2 == 0) {
                                // Add hidden row to ensure even/odd row position for formatting
                                var dummyRow = $("<tr>");
                                dummyRow.addClass("hidden");
                                container.append(dummyRow);
                                N++;
                            }

                            var cell = $(cellDisplayData.th ? "<th>" : "<td>");
                            if (cellDisplayData.href) { // If there is a href, create the text as a link
                                cell.append('<a href="' + cellDisplayData.href + '">' + cellDisplayData.text + '</a>');
                            } else {
                                cell.append(cellDisplayData.text);
                            }

                            row.append(cell);
                        }

                        container.append(row);
                        N++;
                    }
                } else {
                    removeContent();
                    displayError(config.textInvalidData);
                    return false; // break out of $.each loop.
                }
            });
        } else {
            displayError(config.textNoEntriesFound);
        }

        $(".loadingspinner", $this).hide();
    }

    // Remove the content from the widget.
    function removeContent() {
        $(".tableBody", $this).children()
                              .not(".template")
                              .remove();
        $(".serviceError", $this).hide();
        $(".responseTimeContainer", $this).hide();
    }

    function displayError(message) {
        $(".serviceError", $this)
            .empty()
            .append(message)
            .show();
    }

    // Load data, call render if successful
    var loadEntries = function (refresh) {
        // Parameter if service cache should be refreshed.
        var data = {};
        if (refresh) data = { "refresh": true };

        $(".loadingspinner", $this).show();
        $.getJSON(serviceUrl + "/" + uid, data, Render)
            .fail(function (data) {
                var response;
                try {
                    response = $.parseJSON(data.responseText);
                    if (response == null) throw {}; // happens when navigating away from the page on unload, while the ajax-requiest is in progress. This avoids bloating the elmah-log with null pointer exceptions.
                } catch (e) { // response was not json (Perhaps the service controller failed in its constructor)
                    response = { title: "Internal Error", description: "Sorry, an unxpected error occurred.", detailedmessage: "" };
                }
                displayError("Service failed with the following error description: " + response.description);
                $(".loadingspinner", $this).hide();
            });
    }

    // Add refresh button listener
    $(".refresh", $this).click(function () {
        removeContent();
        loadEntries(true);
    });
    
    // Add link click listener (for tracking)
    $this.on("click", ".tableBody a", function () { // Using "delegate event" instead of "direct event" to conserve memory (only 1 listener) and since the links have not been added to the DOM yet.
        // TODO? Using "mousedown" event instead of "click", to also detect middle clicks (should also detect mouse up). But what about "return", context menu etc? Do we have a "follow link" event? No...
        // TODO: http://stackoverflow.com/questions/8927208/catching-event-when-following-a-link
        // TODO: "event detect follow link  click middle keyboard"
        var $link = $(this);
        var widgetName = $(".widget-head .widget-title", $this).text(); // Maybe get this from config instead? Could be unexpected that the title in the HTML is used as the title for tracking too.
        var reportName = $link.text();
        var widgetUId = $this.data("uid");
        if (typeof(PiwikUtil) === "object") PiwikUtil.widget.tabular.trackLinkClickEvent(widgetInstanceId, widgetUId, widgetName, reportName);
    });

    cache = $this.data("fromcache");
    if (cache != "") {
        Render(cache);
    } else {
        loadEntries();
    }
};
