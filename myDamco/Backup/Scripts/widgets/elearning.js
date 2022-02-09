$(function () {
    // Create a jquery plugin for doing setup of the widget news
    $.fn.widget_elearning = function () {
        return this.each(function () {
            var $this = $(this);

            var uid = $this.data("uid");
            var baseconf = $this.data("baseconf");
            var instanceconf = $this.data("instanceconf");
            var content = $(".widget-content", $this);
            var serviceUrl = $this.data("serviceurl") + "/" + uid;
            var targetUrl = baseconf.targeturl;
            var widgetInstanceId = $this.attr("id");

            if ($.cookie("elearning_id") == null)
                $.cookie("elearning_id", 0);

            // Add refresh button listener
            $(".refresh", $this).click(function () {
                var id = parseInt($.cookie("elearning_id"));
                $.cookie("elearning_id", id + 1);
                getData(serviceUrl + "?refresh=1&" + $.cookie("elearning_id"));
            });

            function Render(data) {
                content.empty();
                var cnt = 0;
                $.each(data, function(app) {
                    if (data[app].length > 0) {
                        content.append(app);
                        var ul = $("<ul style='list-style-type: none;'></ul>");
                        content.append(ul);

                        $.each(data[app], function (i, swffile) {
                            var li = $("<li>");

                            var link = $("<a>");
                            //link.attr("href", targetUrl + "/?widgetUid={0}&application={1}&name={2}".format(// <- is a link, instead of just onclick, so that it can be opened in a new tab and be saved?
                            //    encodeURIComponent(uid),
                            //    encodeURIComponent(app),
                            //    encodeURIComponent(swffile.file)
                            //));
                            link.attr("href", targetUrl + "/{0}/{1}/{2}/{3}.swf".format(// <- is a link, instead of just onclick, so that it can be opened in a new tab and be saved?
                                encodeURIComponent(uid),
                                encodeURIComponent(app),
                                encodeURIComponent(swffile.id),
                                encodeURIComponent(swffile.file)
                            ));
                            link.click(function (e) {
                                e.preventDefault();
                                showElearning(link.attr("href"), swffile.id, swffile.width, swffile.height, swffile.resize, swffile.file, app, widgetInstanceId, uid);
                                e.preventDefault();
                            });
                            link.text(swffile.file);

                            li.append(link);
                            ul.append(li);

                            cnt++;
                        });
                    }
                });

                if (cnt == 0) {
                    content.text("No E-Learning available.");
                }
            }

            function getData(serviceUrl) {
                content.html('<img class="loadingspinner" src="'+mydamcoBasepath+'Content/images/wait_15.gif")" alt="Loading" style="padding-top:8px;" />');

                $.get(serviceUrl, Render).error(function (msg) {
                    var response;
                    try {
                        response = JSON.parse(msg.responseText);
                        if (response == null) throw {}; // JSON.parse can return null - for example if data=null (can f.e. happen due to canceled ajax calls on page unload)
                    } catch (e) { // response was not json (Perhaps the service controller failed in its constructor)
                        response = { title: "Internal Error", description: "Sorry, an unxpected error occurred.", detailedmessage: "" };
                    }
                    content.html("<div class='serviceError'>The E-Learning service failed with the following description:" +
                                    "<div class='errorDescription'>" +
                                        response.description +
                                        "<div>" + response.detailedmessage + "</div>" +
                                    "</div>" +
                                "</div>");

                });
            }

            var cache = $this.data("fromcache");
            if (cache == "") {
                getData(serviceUrl + "?" + $.cookie("elearning_id"));
            }
            else {
                Render(cache);
            }
        });
    };
    $(".elearningdimdiv").click(function () {
        hideElearning();
    });

    // Setup News widgets for all damconews instances
    $(".widget-elearning").widget_elearning();

    function shrinkToWindow(width, height) {
        var viewportWidth = $(window).width();
        var viewportHeight = $(window).height();

        var inputRatio = width / height;
        var viewportRatio = viewportWidth / viewportHeight;
        if (width < viewportWidth && height < viewportHeight) {
            var newheight = height;
            var newwidth = width;
        }
        else if (viewportRatio > inputRatio) {
            var newheight = viewportHeight;
            var newwidth = width * viewportHeight / height;
        }
        else {
            var newwidth = viewportWidth;
            var newheight = height * viewportWidth / width;
        }

        return [newwidth, newheight];
    }

    function clipToWindow(width, height) {
        var viewportWidth = $(window).width();
        var viewportHeight = $(window).height();

        if (width < viewportWidth)
            var newwidth = width;
        else
            var newwidth = viewportWidth;
        if (height < viewportHeight)
            var newheight = height;
        else
            var newheight = viewportHeight;
        
        return [newwidth, newheight];
    }

    function showElearning(src, id, width, height, resize, name, app, widgetInstanceId, widgetUId) {
        if (resize == "none")
            var newSize = [width, height];
        else if (resize == "clip")
            var newSize = clipToWindow(width, height);
        else
            var newSize = shrinkToWindow(width, height);
        
        $(".elearningdimdiv").show();
        $("#elearningflashdiv").flash({ src: src, width: newSize[0], height: newSize[1], base: "Services/ELearningLoadFile/" + widgetUId + "/" + app + "/" + id + "/", overflow: scroll });

        if (typeof (PiwikUtil) === "object") PiwikUtil.widget.elearning.trackShowEvent(widgetInstanceId, widgetUId, app, name);
    }

    function hideElearning() {
        $(".elearningdimdiv").hide();
        $("#elearningflashdiv").empty();
    }

    $(document).keyup(function (e) {
        if (e.keyCode == 27) { hideElearning(); }   // esc
    });
});

