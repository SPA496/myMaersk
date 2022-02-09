$(function () {

    var config = {
        textNoEntriesFound: "No pouches found.",
        textInvalidData: "The document management application returned invalid data. Try refreshing the widget later.",

        expectedDataLength: 3,  // <- num <td>s expected per <tr> in data from server

        createRowDisplayData: function (td) {
            var username = "'" + mydamcoUsername + "'";
            var pUrl = [mydamcoBasepath, "WidgetLink/documentmanagement/recentpouches/", "?bl=", td[0], "&loginID=", encodeURIComponent(username)].join("");
            return [[{ text: td[0], href: pUrl }, { text: td[1], href: null }, { text: td[2], href: null }]];
        }
    };

    // Create a JQuery plugin to setup all widget instances of this widget-type, by calling the shared tabulardata function. This plugin is used from elsewhere.
    $.fn.widget_recentpouches = function () {
        return this.each(function () {
            widget_shared_tabulardata.call(this, config);
        });
    };

    // Setup widget for all instances by running the plugin
    $(".widget-recentpouches").widget_recentpouches();
});
