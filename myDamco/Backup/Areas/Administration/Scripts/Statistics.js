$(function () {

    function initWidgetUsageStatisticsSection() {
        
        // Init DateTimePickers
        var fromPicker = $("#widgetstats_from_picker");
        var toPicker = $("#widgetstats_to_picker");
        fromPicker.datetimepicker({
            dateFormat: "dd-mm-yy",
            timeFormat: "HH:mm",
            stepMinute: 1,
            useLocalTimezone: false,
            defaultTimezone: '+0000'
        });
        
        toPicker.datetimepicker({
            dateFormat: "dd-mm-yy",
            timeFormat: "HH:mm",
            stepMinute: 1,
            useLocalTimezone: false,
            defaultTimezone: '+0000'
        });

		// Init DateTimePickers to current time (UTC)
        var nowDate = new Date(); 
        fromPicker.datetimepicker('setDate', UnixTimeToUTCTime(nowDate.getTime()));
        toPicker.datetimepicker('setDate', UnixTimeToUTCTime(nowDate.getTime()));

        // Force the datepicker "Now" button to set the date/time in UTC instead of in local time
        forceDatePickerNowButtonToUseUTC();
        

        // Init Update Button
        var updateButton = $("#widgetstats_update_button");
        updateButton.click(function() {

            // get (from,to) time and convert it to the format MVC likes for its action method parameters.
            var from = $("#widgetstats_from_picker").datetimepicker("getDate");
            var to = $("#widgetstats_to_picker").datetimepicker("getDate");
            var fromTimestamp = UTCTimeToUnixTime(from);
            var toTimestamp = UTCTimeToUnixTime(to);
            var isofrom = ISODateString(fromTimestamp);
            var isoto = ISODateString(toTimestamp);

            //alert(from+"\n"+isofrom+"\n"+fromTimestamp);

            if (!from) {
                alert("From time not filled out");
                return;
            }

            if (!to) {
                alert("To time not filled out");
                return;
            }
            
            if (fromTimestamp > toTimestamp) {
                alert("'From' time must be before or the same as 'To' time");
                return;
            }

            var loadingSpinner = $("#widgetStats .loadingspinner");
            var container = $("#widgetStatsContainer");
            loadingSpinner.show();
            container.css("visibility", "hidden");
            
            $.ajax({
                type: "GET",
                url: "Statistics/WidgetStatistics",
                traditional: true,
                data: {
                    from: isofrom,
                    to: isoto
                },
                success: function (data) {
                    loadingSpinner.hide();
                    container.css("visibility", "visible");
                    container.html(data);
                },
                error: function () {
                    loadingSpinner.hide();
                    container.css("visibility", "visible");
                    alert("There was an error fetching the widget statistics.");
                }
            });
        });
    }

    // Init everything
    initWidgetUsageStatisticsSection();
});