$(function () {

    // Create a jquery plugin for doing setup of the widget news
    $.fn.widget_rssnews = function () {
        return this.each(function () {
            var $this = $(this);

            // Widget
            var id = $this.data("uid");
            var baseconf = $this.data("baseconf");
            var instanceconf = $this.data("instanceconf");
            var config = $("div.configuration", $this);
            var itemsPerPage = null;
            var serviceUrl = $this.data("serviceurl");
            var widgetInstanceId = $this.attr("id");
            
            // Current feed
            var selectedFeed;
            var pages = 0;
            var page = 0;
            var feedItems;

            // Bail if don't have any feeds configuration
            if (!baseconf.hasOwnProperty("feeds")) return;
            if (baseconf.hasOwnProperty("headlinecount")) itemsPerPage = baseconf.headlinecount;

            // If the widget has only one feed, select it and remove editability
            if (baseconf.feeds.length == 1) {
                $this.attr("class", "widget widget-rssnews"); // Remove editability
                selectedFeed = baseconf.feeds[0];
            }
            // Otherwise see if the user has selected a feed
            else if (instanceconf.hasOwnProperty("selectedfeed")) {
                $.each(baseconf.feeds, function (index, value) {
                    if (instanceconf.selectedfeed == value.id) {
                        selectedFeed = value;
                        return;
                    }
                });
            }

            // Load selected RSS feed if we found a selected feed
            var loadFeed = function (refresh) {
                $(".serviceError", $this).hide();

                // Parameter if service cache should be refreshed.
                var data = {};
                if (refresh)
                    data = { "refresh": true };

                // Get feed, if one is selected
                if (selectedFeed) {
                    $(".loadingspinner", $this).show();

                    $.getFeed({
                        url: [serviceUrl, "/", id, "/", selectedFeed.id].join(""),
                        data: data,
                        cache: false, /* prevent IE from caching the AJAX response */
                        success: function (feed) {
                            recieveData(feed);
                        }
                    }).fail(function (data) {
                        var response;
                        try {
                            response = jQuery.parseJSON(data.responseText);
                            if (json == null) throw {}; // parseJSON can return null - for example if data=null (can f.e. happen due to canceled ajax calls on page unload)
                        } catch (e) { // response was not json (Perhaps the service controller failed in its constructor)
                            response = {title:"Internal Error", description:"Sorry, an unxpected error occurred.", detailedmessage:""};
                        }
                        $(".serviceError", $this)
                            .show()
                            .children(".errorDescription")
                            .empty()
                            .append(response.description)
                            .append("<div>" + response.detailedmessage + "</div>");

                        $(".loadingspinner", $this).hide();
                    });
                } else {
                    $(".loadingspinner", $this).hide();
                    config.show();
                }
            }

            // Render current feed
            var renderFeed = function () {
                var template = $(".feedTitle.template", $this);
                var dateTemplate = $(".feedDate.template", $this);
                var container = template.parent();
                container.children().not(".template").remove();


                var offset = itemsPerPage * page;
                var limit = feedItems.length;
                if (itemsPerPage != null && itemsPerPage + offset < feedItems.length)
                    limit = offset + itemsPerPage;

                if (limit == 0) $(".feedEmpty", $this).show();
                else $(".feedEmpty", $this).hide();

                var date = null;
                for (var j = offset; j < limit; j++) {
                    var item = feedItems[j];
                    var itemDate = new Date();
                    itemDate.setTime(Date.parse(item.updated));

                    var itemLocaleDateStringUTC = getLocaleDateStringUTC(itemDate); // items are grouped by UTC-date and dates are shown in UTC

                    if (date == null || itemLocaleDateStringUTC != date) {
                        var dateRow = dateTemplate.clone();
                        dateRow.attr("class", "feedDate").html(itemLocaleDateStringUTC);
                        container.append(dateRow.show());
                        date = itemLocaleDateStringUTC;
                    }

                    var row = template.clone();
                    row.attr("class", "feedTitle")
                        .children("a")
                        .attr("href", item.link)
                        .html(escapeHTML(item.title));
                    container.append(row.show());
                }
                
                // Set "show archive" link
                var archiveLink = selectedFeed && selectedFeed.id ? [mydamcoBasepath, "Dashboard/NewsFeed/", selectedFeed.id, "/", "?showArchived=true"].join("") : "";
                $(".archive a", $this).attr("href", archiveLink);

                // Enable/Disable bottom buttons
                var hasOlder = page < pages - 1;
                var hasNewer = page > 0;
                var showArchive = baseconf.hasOwnProperty("showarchive") ? baseconf.showarchive == "true" : false;
                if (hasOlder) $(".older", $this).show();
                else $(".older", $this).hide();
                if (hasNewer) $(".newer", $this).show();
                else $(".newer", $this).hide();
                if (showArchive) $(".archive", $this).show();
                else $(".archive", $this).hide();
                if (hasOlder || hasNewer || showArchive) $(".bottom-options", $this).show();
                else $(".bottom-options", $this).hide();

                $(".loadingspinner", $this).hide();
            };

            // Returns a string containing a date (not time) in UTC (not local time), in the users locale format. (param localDate is a Date object)
            var getLocaleDateStringUTC = function (localDate) {
                var utcDate = new Date(localDate.getTime() + localDate.getTimezoneOffset() * 1000 * 60); // create a date with the timezone-offset removed (ex: for "UTC+02:00" we add -120 minutes)
                return utcDate.toLocaleDateString(); // toLocaleDateString() now returns the date relative to UTC instead of local time
            };

            // Navigate pages of feed items (older or newer)
            var navigateFeed = function (older) {
                if (older && page < pages || !older && page > 0) {
                    if (older) page++;
                    else page--;
                }
                renderFeed();
            };

            // Load configuration view 
            var configList = config.children(".configList");
            $.each(baseconf.feeds, function (index, value) {
                var item = configList.children(".template").clone();
                item.attr("class", "configItem")
                    .html(value.title)
                    .click(function () {
                        config.slideUp(600);
                        $this.setWidgetConfiguration({ "selectedfeed": value.id });
                        $this.setWidgetTitle(value.title);
                        selectedFeed = value;
                        loadFeed();
                        if (typeof(PiwikUtil) === "object") PiwikUtil.widget.rssnews.trackSelectFeedEvent(widgetInstanceId, $this.data("uid"), id, selectedFeed.title); // track the event
                    });
                configList.append(item.css("display", "block"));
            });

            // Add navigate buttons listeners
            $(".older", $this).click(function () {
                navigateFeed(true);
            });
            $(".newer", $this).click(function () {
                navigateFeed(false);
            });

            // Add refresh button listener
            $(".refresh", $this).click(function () {
                $(".loadingspinner", $this).show();
                $(".feedTitle.template", $this).parent().children().not(".template").remove();
                loadFeed(true);
            });

            // Add news item click listener (for tracking)
            $this.on("click", ".feedTitle a", function () { // Using "delegate event" instead of "direct event" to conserve memory (only 1 listener) and since the links have not been added to the DOM yet.
                var $newsLink = $(this);
                var widgetUId = id;
                var widgetName = id;
                var newsFeedName = selectedFeed.title;
                var newsItemName = $newsLink.text();
                if (typeof(PiwikUtil) === "object") PiwikUtil.widget.rssnews.trackItemClickEvent(widgetInstanceId, widgetUId, widgetName, newsFeedName, newsItemName);
            });

            var recieveData = function (feed) {

                pages = Math.ceil(feed.items.length / itemsPerPage);
                page = 0;
                feedItems = feed.items;
                renderFeed();

            };


            var cache = $this.data("fromcache");
            if (cache != "") {
                var feed = new JRss($.parseXML(cache));
                recieveData(feed);
            } else {
                loadFeed(false);
            }
        });
    };

    // Setup News widgets for all damconews instances
    $(".widget-rssnews").widget_rssnews();
});