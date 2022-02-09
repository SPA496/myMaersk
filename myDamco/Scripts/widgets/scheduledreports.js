$(function () {

    var config = {
        textNoEntriesFound: "No reports found.",
        textInvalidData: "The reporting application returned invalid data. Try refreshing the widget later.",

        expectedDataLength: 4,  // <- num <td>s expected per <tr> in data from server

        createRowDisplayData: function (td) {
            var defUrl = [mydamcoBasepath, "WidgetLink/tracktrace/scheduledreports/", "?action=extract_center&defid=", td[0]].join("");
            return [[{ text: td[1], href: defUrl }, { text: td[2], href: null }, { text: td[3], href: null }]];
        }
    };

    // Create a JQuery plugin to setup all widget instances of this widget-type, by calling the shared tabulardata function. This plugin is used from elsewhere.
    $.fn.widget_scheduledreports = function () {
        return this.each(function () {
            widget_shared_tabulardata.call(this, config);
        });
    };

    // Setup widget for all instances by running the plugin
    $(".widget-scheduledreports").widget_scheduledreports();
});
