$(function () {

    var config = {
        textNoEntriesFound: "No searches found.",
        textInvalidData: "The reporting application returned invalid data. Try refreshing the widget later.",

        expectedDataLength: 3,  // <- num <td>s expected per <tr> in data from server

        createRowDisplayData: function (td, prev_td) {
            var sUrl = mydamcoBasepath + "WidgetLink/tracktrace/recentsearches/?" + td[1]; // TODO: Verify the target url

            var category = td[0];
            var prevCategory = prev_td ? prev_td[0] : null;
		
            var rows = [];
            if (category != prevCategory) {		
                rows.push([{ text: td[0], th: true, odd: true }, { text: "", th: true }]);
            }
		
            rows.push([{text:""}, {text: td[2], href: sUrl}]);
            return rows;
        }
    };

    // Create a JQuery plugin to setup all widget instances of this widget-type, by calling the shared tabulardata function. This plugin is used from elsewhere.
    $.fn.widget_recentsearches = function () {
        return this.each(function () {
            widget_shared_tabulardata.call(this, config);
        });
    };
    
    // Setup widget for all instances by running the plugin
    $(".widget-recentsearches").widget_recentsearches();
});

