$(function () {

    var config = {
        textNoEntriesFound: "No reports found.",
        textInvalidData: "The reporting application returned invalid data. Try refreshing the widget later.",

        expectedDataLength: 6,  // <- num <td>s expected per <tr> in data from server

        createRowDisplayData: function(td) {
            var defUrl = [mydamcoBasepath, "WidgetLink/tracktrace/recentreports/", "?action=extract_center&defid=", td[0], "&folder=", td[1]].join("");
            var resUrl = [mydamcoBasepath, "WidgetLink/tracktrace/recentreports/", "?action=extract_view&exid=", td[2]].join("");
            return [[{text: td[3], href: defUrl}, {text: td[4], href: null}, {text: td[5], href: resUrl}]];
        }
    };

    // Create a JQuery plugin to setup all widget instances of this widget-type, by calling the shared tabulardata function. This plugin is used from elsewhere.
    $.fn.widget_recentreports = function () {
        return this.each(function () {
            widget_shared_tabulardata.call(this, config);
        });
    };

    // Setup widget for all instances by running the plugin
    $(".widget-recentreports").widget_recentreports();
});
