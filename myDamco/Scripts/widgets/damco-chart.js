// TODO: Do filter-editing with this: http://loopj.com/jquery-tokeninput/

function ADFSReportingLoginRefreshAllWidgets() {
    $(".widget-performancechart").each(function () {
        var $this = $(this);
        var container = $(".widget-content", $this);
        var chartWidget = container.data("widgetObj");
        if (chartWidget) { // abort if widget not yet satup
            chartWidget.invokeConfigService(); // TODO: Maybe click on refresh button instead.
        }
    });
}

function ADFSReportingLoginComplete() {

    // Create a jquery plugin for doing setup of the widget
    $.fn.widget_performancechart = function () {
        return this.each(function () {
            var $this = $(this);
            var baseconf = $this.data("baseconf");
            var instanceconf = $this.data("instanceconf");
            var container = $(".widget-content", $this);

            var chartWidget = new ChartWidget($this.attr("id"), container, baseconf, instanceconf);
            container.data("widgetObj", chartWidget);

            $("div.configuration", container).hide();

            $(".clear-filters", container).click(function (e) {
                if (!chartWidget.configuration.filters)
                    return false;

                chartWidget.startSpinner();

                chartWidget.removeFilters(0, chartWidget.configuration.filters.length);
                chartWidget.configuration.group = chartWidget.getOrder()[0];

                chartWidget.updateColumns();
                chartWidget.invokeDataService();
                chartWidget.sendConfigurationToServer();

                return false;
            });

            $("a.charttype", $this).click(function (e) {
                chartWidget.configuration.charttype = this.id;
                chartWidget.sendConfigurationToServer();
                chartWidget.updateData();
            });


            $("#filter", $this).click(function (e) {
                $(".filter-element", container).toggle();

                return false;
            });

            // Add refresh button listener
            $(".refresh", $this).click(function () {
                chartWidget.invokeConfigService();
            });

            // Add "See Report"-link listener (for tracking)
            $(container).on("click", ".chartdetails", function () {
                var reportId = chartWidget.configuration.definitionId;
                var widgetUId = $this.data("uid");
                if (typeof(PiwikUtil) === "object") PiwikUtil.widget.piechart.trackSeeReportClickEvent(chartWidget.id, widgetUId, reportId, chartWidget.getDefinitionName(reportId));
            });

            // Add "rename title of this widget instance" functionality (Note: This could easily be made to work for all widget types, not just pie-charts)
            (function () {
                var renameContainer = $("div.rename-title-container", $this);
                var toolbarIcon = $(".icon.rename-title", $this);
                var submitButton = $(".rename-title-button", renameContainer);
                var textField = $(".renamed-title-text", renameContainer);
                renameContainer.hide();
                toolbarIcon.click(function () {
                    textField.val($this.getWidgetTitle());
                    renameContainer.toggle();
                });
                var submitHandler = function () {
                    var newTitle = textField.val();
                    if (newTitle.indexOf("<") != -1 || newTitle.indexOf(">") != -1) {
                        $("#dashboard-error-dialog").showErrorDialog('{ "Title": "Widget title could not be set.", "Description": "The special characters \'<\' and \'>\' can not be used in titles." }');
                        //textField.val(newTitle.replace(/[\<\>]/g, ""));
                        return;
                    }
                    var onError = function (xhr, oldTitle) {
                        textField.val(newTitle);
                        renameContainer.show();
                        $("#dashboard-error-dialog").showErrorDialog('{ "Title": "Widget title could not be set.", "Description": "An error occured while setting the widget title." }'); // (xhr.responseText)
                    };
                    $this.setWidgetTitle(newTitle, onError);
                    renameContainer.hide();
                };
                submitButton.click(submitHandler);
                textField.keypress(function (event) {
                    if (event.which == 13) submitHandler();
                    if (event.which == "<".charCodeAt(0) || event.which == ">".charCodeAt(0)) // Disallow entering "<" and ">". Currently, the MVC acton-methods will automatically reject html-injections in strings (and razor will also HTML escape strings when outputting), so this is to improve the user-experience only (not security!), by making it visible that "<" and ">" are not allowed.
                        event.preventDefault();
                });
            }());

            // KV: For unknown reasons, IE8 and below will fire a couple of window-resize events, when loading the dashboard.
            // Workaround here: http://stackoverflow.com/questions/1852751/window-resize-event-firing-in-internet-explorer
            var lastWindowHeight = $(window).height();
            var lastWindowWidth = $(window).width();

            $(window).resize(function () {

                //confirm window was actually resized
                if ($(window).height() != lastWindowHeight || $(window).width() != lastWindowWidth) {

                    //set this windows size
                    lastWindowHeight = $(window).height();
                    lastWindowWidth = $(window).width();

                    container.data("widgetObj").updateData();
                }
            });

            chartWidget.invokeConfigService();
        });
    };

    // Setup Chart widgets for all instances
    $(".widget-performancechart").widget_performancechart();


};

function getChartObject(id) {
    var container = $("#" + id + " .widget-content");
    return container.data("widgetObj");
}
// The ChartWidget Class

function ChartWidget(id, container, baseconf, instanceconf) {
    this.id = id;
    this.container = container;
    this.baseconf = baseconf;

    // Init shortcuts to recent used places in the HTML.
    this.settingsContainer = $(".configuration", container);
    this.columnsContainer = $(".columns", container);
    this.chartContainer = $(".chart", container);

    this.errorMessageContainer = $(".charterrormessage", container);

    // The configuration is what we save in the database
    this.configuration = {
        charttype: instanceconf.charttype !== undefined ? instanceconf.charttype : "pie",
        definitionId: instanceconf.definitionId !== undefined ? instanceconf.definitionId : null,
        filters: instanceconf.filters !== undefined ? instanceconf.filters : null,
        group: instanceconf.group !== undefined ? instanceconf.group : null
    };

    this.paper = null;
    this.exclude = [];

    $("#chartcontent", this.container).hide();

    this.getDefinitionName = function (id) {
        var index = $.inArray(id, this.definitions.columns.values);
        if (index == -1)
            return "";
        return this.definitions.columns.names[index];
    };

    this.setDefinitionId = function (id) {
        this.configuration.definitionId = id;

        $("input.charttype[value=" + this.configuration.charttype + "]", this.container).attr("checked", "checked");

        if (id == "" || id == null || id == undefined) {
            this.columns = null;
            this.data = null;
            $("#chartcontent", this.container).hide();
            this.stopSpinner();
        }
        else {
            $("#definitions", this.container).val(this.getDefinitionName(id));
            $("#chartcontent", this.container).show();
            this.startSpinner();
            this.invokeColumnService();
        }
    };

    // The "invoke"-functions are the functions that invoke the services.
    this.invokeDataService = function () {
        this.startSpinner();
        this.errorMessageContainer.hide();

        var filter = getChartFilterString(this.configuration.filters);

        var dataUrl = this.baseconf.ReportingChartDataServiceUrl.format(this.configuration.definitionId, filter, this.configuration.group, this.returnData ? this.returnData : "");

        var obj = this;
        var errorFun = function () {
            obj.setError("Could not retrieve data from Reporting. The Web Service did not reply.");
        };
        var succesFun = function (data) {
            obj.setData(data);
        };

        attachScript(dataUrl, succesFun, errorFun);
    };

    this.invokeColumnService = function () {
        this.startSpinner();
        this.errorMessageContainer.hide();

        var confUrl = this.baseconf.ReportingColumnsServiceUrl.format(this.configuration.definitionId);

        var obj = this;
        var errorFun = function () {
            obj.setError("Could not retrieve columns from Reporting. The Web Service did not reply.");
        };
        var succesFun = function (data) {
            obj.setColumns(data);
        };

        attachScript(confUrl, succesFun, errorFun);
    };

    this.invokeConfigService = function () {
        this.startSpinner();
        this.errorMessageContainer.hide();

        var obj = this;
        var errorFun = function () {
            obj.setError("Could not retrieve definitions from Reporting. The Web Service did not reply.");
        };
        var succesFun = function (data) {
            obj.setConfiguration(data);
        };

        var confUrl = this.baseconf.ReportingConfigurationServiceUrl;
        attachScript(confUrl, succesFun, errorFun);
    };


    // The "set"-functions are the functions callback functions for JSONP, and sets it correctly.
    this.setConfiguration = function (defs) {
        this.definitions = defs;

        this.updateDefinitions();
        if (this.configuration.definitionId != null) {/* widget config exists */
            this.setDefinitionId(this.configuration.definitionId);
        }
        else {
            $(".configuration", this.container).show();
            this.stopSpinner();
        }

    };

    this.setColumns = function (cols) {
        this.columns = cols.columnDefinitions;

        // Check if column-data is invalid (such as empty)
        if (!this.columns || !$.isArray(this.columns) || this.columns.length == 0) {
            this.setError("Received empty column data from Reporting.");
            return;
        }

        // If no filter has been set already, set a default. (Also do this, if a filter with invalid length has been set, which doesn't match the number of columns)
        if (this.configuration.filters == null || this.columns.length != this.configuration.filters.length) {
            this.configuration.filters = this.defaultFilter();
            this.configuration.group = 0;
        }

        // Init columns
        var cont = $(".sortable", this.columnsContainer);
        cont.empty();

        for (var order = 0; order < this.configuration.filters.length; order++) {
            var index = this.columnOrderToIndex(order);

            var header = this.columns[index].columnHeader;
            cont.append('<li id="' + index + '" class="filter"><span class="filtereditorholder"></span><span class="sortablehead">' + header + " <span class='count'></span></span></li>");
        }

        var obj = this;
        var IE6orIE7 = navigator.appVersion.indexOf("MSIE 7.") != -1 || navigator.appVersion.indexOf("MSIE 6.") != -1;

        $(".filter", cont).hover(function () { /* KV: IE 6 hack */
            $(this).addClass('hover');
            if (IE6orIE7 && obj.configuration.charttype == "pie")
                obj.chartContainer.css("visibility", "hidden");
        }, function () {
            $(this).removeClass('hover');
            if (IE6orIE7 && obj.configuration.charttype == "pie")
                obj.chartContainer.css("visibility", "visible");
        });

        // Init the sortability
        var obj = this;
        cont.sortable({
            handle: ".sortablehead",
            forcePlaceholderSize: true,
            start: function () {
                $(".filtereditorholder", this).empty(); // When we start dragging a column, remove the filterlist. Looks nicer.
            },
            stop: function () {
                var container = $(this).parent().parent();
                var order = obj.getColumnOrderFromContainer();

                var oldOrder = obj.getOrder();
                obj.reorderFilters(order);
                var newOrder = obj.getOrder();

                //Find the changed part
                for (var i = 0; i < oldOrder.length; i++) {
                    if (oldOrder[i] != newOrder[i])
                        break;
                }

                obj.removeFilters(i, newOrder.length);

                obj.setGroupFromActiveFilters();

                obj.updateColumns();
                obj.invokeDataService();
                obj.sendConfigurationToServer();
            }
        });

        $(".sortablehead", cont).click(function () {
            obj.configuration.group = parseInt(this.parentElement.id);
            obj.updateColumns(-1);

            obj.invokeDataService();
            obj.sendConfigurationToServer();
        });



        this.updateColumns();

        this.invokeDataService();
        this.sendConfigurationToServer();
    };

    this.setData = function (data) {
        this.data = data;

        // KV: Whenever we recieve data, this is new posibilities for filtering values. Add these for more user friendliness
        this.columns[this.configuration.group].filterValues = union_arrays(this.data.legends, this.columns[this.configuration.group].filterValues);
        this.updateColumn(this.configuration.group);

        // Set the total count
        var s = Math.floor(sum(this.data.values));
        $(".totalCount", container).text(s);

        // Set the "See report" link
        var detailsLink = $(".chartdetails", this.container);
        var filterString = getChartFilterString(this.configuration.filters);
        var url = mydamcoBasepath + "WidgetLink/tracktrace/performancechart/" + this.baseconf.ReportingDetailsUrlTemplate.format(this.configuration.definitionId, filterString, this.configuration.group);
        detailsLink.attr('href', url); // Avoid roundtrip to server by setting href directly. The controller does nothing anyway!      

        this.updateData();
    };


    // The below functions are the functions that updates HTML based on the current state.
    this.updateDefinitions = function () {

        var obj = this;
        var defs = this.definitions.columns;

        var reports = [];
        for (var i = 0; i < defs.names.length; i++) {
            reports.push({ label: defs.names[i], value: defs.values[i] });
        }

        $("#definitions", this.container).autocomplete({
            minLength: 0,
            source: reports,
            focus: function (event, ui) {
                $(this).val(ui.item.label);
                return false;
            },
            select: function (event, ui) {
                $(this).val(ui.item.label);

                obj.configuration.filters = null;
                obj.configuration.group = 0;
                obj.setDefinitionId(ui.item.value);

                obj.sendConfigurationToServer();

                obj.settingsContainer.hide();
                $("#" + obj.id).setWidgetTitle(ui.item.label);

                // track the report selection
                var widgetUId = $("#" + obj.id).data("uid");
                if (typeof(PiwikUtil) === "object") PiwikUtil.widget.piechart.trackSelectReportEvent(obj.id, widgetUId, ui.item.value, ui.item.label);

                return false;
            }
        }).focus(function () {
            if (this.value == "")
                $(this).trigger('keydown.autocomplete');
        });
    };

    this.updateColumns = function (index) {
        if (index == undefined) {
            for (var order = 0; order < this.configuration.filters.length; order++) {
                var index = this.columnOrderToIndex(order);
                this.updateColumn(index);
            }
        }
        else if (index != -1) {
            // Only do this if index is defined and not -1
            // -1 means "We only changed the group, no need to redraw columnlists"
            this.updateColumn(index);
        }

        // Reset classes on all filters
        $(".filter", this.columnsContainer).removeClass("activegroup").removeClass("passedgroup");
        var maxOrder = this.columnIndexToOrder(this.configuration.group);

        for (var order = 0; order < maxOrder; order++) {
            var filter = $("#" + this.columnOrderToIndex(order) + ".filter", this.columnsContainer);
            filter.addClass("passedgroup");
        }

        // Add it for the current active filter
        var activeFilterIndex = this.configuration.group;
        var activeFilter = $("#" + activeFilterIndex + ".filter", this.columnsContainer);
        activeFilter.addClass("activegroup");



    };

    this.updateColumn = function (index) {
        var order = this.columnIndexToOrder(index);
        var filter = $("#" + index + ".filter", this.container);
        var filterEditor = $(".filtereditorholder", filter);
        var values = this.getFilterValueList(index);

        //var filterString = '<iframe src="javascript:false;" ></iframe>';
        var filterString = "";
        filterString += "<div class='filtereditor'>";

        filterString += "<div class='filterlist'>";
        for (var i = 0; i < values.length; i++) {
            var selected = ""
            if (values[i].selected)
                selected = "checked";

            var box = "<input type='checkbox' " + selected + " onClick='getChartObject(\"" + this.id + "\").toggleFilterValue(" + index + ", \"" + values[i].item + "\")'>";
            filterString += "<div class='filteritem'>" + box + " " + values[i].item + "</div>";
        }
        filterString += "</div>";

        filterString += "<div class='addmanualy'>Other: <input type='text' class='filtername'/> <input type='button' value='Add' class='addbutton'/></div>";
        filterString += "</div>";
        filterEditor.append(filterString);

        var obj = this;
        $(".addmanualy .filtername", filterEditor).keypress(function (ind) {
            return function (e) {
                if (e.which == 13) {
                    obj.toggleFilterValue(ind, $(this).val());
                }
            }
        }(index));

        $(".addmanualy .addbutton", filterEditor).click(function (ind) {
            return function (e) {
                var txtElement = $(".filtername", this.parentElement);
                obj.toggleFilterValue(ind, txtElement.val());
            }
        }(index));

        count = this.configuration.filters[order].values.length;
        if (count > 0)
            $(".count", filter).text('(' + count + ')');
        else
            $(".count", filter).text('');
    };

    this.updateData = function () {
        var chartData = this.data;

        var values = [];
        var legends = [];
        var colors = [];
        var links = [];

        if (chartData && chartData.values && chartData.values.length > 0) {
            var currentLevel = this.currentLevel();

            var actualLinkNo = 0;

            for (var i = 0; chartData.legends && i < chartData.legends.length; i++) {
                var shouldBeIncluded = $.inArray(chartData.legends[i], this.exclude) == -1; // this.exclude is set when user presses "others".

                // The last condition is because gRaphael fucks up if you have 0 size elements in the values.
                if (shouldBeIncluded && chartData.values[i] != 0) {

                    legends.push(chartData.legends[i]);
                    values.push(chartData.values[i]);
                    colors.push(chartData.colors[i])
                    if (currentLevel + 1 < this.configuration.filters.length)
                        links.push({ actualLinkNo: actualLinkNo, currentLevel: currentLevel });

                    actualLinkNo++;
                }

            }

            if (!chartData.colors)
                colors = null;

            if (this.configuration.charttype == "pie" || this.configuration.charttype == "bar")
                this.updatePie(values, legends, links, colors);
            else
                this.updateTable(values, legends, links);

        } else {
            this.chartContainer.empty().append("<span>No data to show</span>");
        }

        this.stopSpinner();
    };

    this.updateTable = function (values, legends, links) {
        // Sort :-/
        var valuesWithIndexes = $(values).map(function (i, e) { return { i: i, e: e }; });
        valuesWithIndexes.sort(function (a, b) { return ((a.e < b.e) ? 1 : ((a.e > b.e) ? -1 : 0)); });
        var valuesAndLegends = $(valuesWithIndexes).map(function (_, e) { return { legend: legends[e.i], value: values[e.i], link: links[e.i] }; });

        this.chartContainer.empty();
        var table = ("<table class='tabledata'><thead><tr><td>" + this.columns[this.configuration.group].columnHeader + "</b></td><td>Count</td><td>Percentage</td><tr></thead>");
        var total = sum(values);
        for (var i = 0; i < valuesAndLegends.length; i++) {
            var link = "#";
            if (links.length > 0) {
                link = "javascript:getChartObject(\"" + this.id + "\").sliceLink(\"" + valuesAndLegends[i].legend + "\"," + valuesAndLegends[i].link.currentLevel + ")";
            }

            var pct = Math.round((valuesAndLegends[i].value / total * 100) + "") + "%";

            var linkText = valuesAndLegends[i].legend;
            if (linkText == "") linkText = "&nbsp;"; // <- browsers won't render empty links, so replace it with a nbsp. (empty legend names can happen)
            table += ("<tr><td class='legend'><a href='" + link + "'>" + linkText + "</td><td class='cnt'>" + valuesAndLegends[i].value + "</td><td class='pct'>" + pct + "</td></tr>");
        }

        table += "</table>";
        this.chartContainer.append(table);
    };

    this.updatePie = function (values, legends, links, colors) {
        this.chartContainer.empty();
        var elementId = this.id + "_chartDiv";

        this.chartContainer.append('<div id="' + elementId + '" style="z-index: -1"></div>');
        var element = $("#" + elementId, this.container);

        var legs = $.map(legends, function (v, i) { return "%%.% – " + v; });

        FixRaphael(values, legs);

        if (this.configuration.charttype == "pie") {
            var sliceCount = 6;
            element.height(250 + 16 * sliceCount);
            var r = Raphael(elementId);
            
            var obj = this;

            if (links.length > 0)
                links = $(links).map(function (i, e) { return "javascript:getChartObject('" + obj.id + "').sliceLink(" + e.actualLinkNo + "," + e.currentLevel + ")"; });
            this.pie = r.piechart(element.width() / 2, 110, 100, values, { legend: legs, legendpos: "south", href: links, colors: colors, maxSlices: sliceCount-1, matchColors: true });

            var hoverIn = function (e) {
                this.sector.stop();
                this.sector.transform("");
                this.sector.scale(1.1, 1.1, this.cx, this.cy);

                if (this.label) {
                    this.label[0].stop();
                    this.label[0].attr({ r: 7.5 });
                    this.label[1].attr({ "font-weight": 800 });
                }

                var name = legends[this.value.order];
                var labelname = "labelhover" + this.value.order;
                if (this.value.others == true || name == undefined)
                    name = "Others";
                $("<div class='" + labelname + " pielabel'>" + name + "</div>").hide().appendTo("body").delay(100).fadeIn("fast");

                $(obj.chartContainer).bind('mousemove', function (e) {
                    $('.' + labelname).css({
                        position: "absolute",
                        left: e.pageX + 10,
                        top: e.pageY - 20,
                        background: 'white',
                        padding: '4px',
                        border: '1px solid',
                        cursor: 'default'
                    });
                });
            };

            var hoverOut = function () {
                this.sector.animate({ transform: 's1 1 ' + this.cx + ' ' + this.cy }, 500, "bounce");

                if (this.label) {
                    this.label[0].animate({ r: 5 }, 500, "bounce");
                    this.label[1].attr({ "font-weight": 400 });
                }
                $('.pielabel').remove(); // KV: We remove all labels. Allways. Some weird bug in IE8 makes it so the hoverOut sometimes does not get triggered. This ensures that we greedily remove everything.
            };
            $(this.chartContainer).mouseout(function () {
                $('.pielabel').remove(); // KV: We remove all labels. Also when we leave the chartcontainer.
            });

            var isIE6 = (/\bMSIE 6/.test(navigator.userAgent) && !window.opera);
            if (!(isIE6 || (navigator.appVersion.indexOf("MSIE 7.") != -1))) {
                this.pie.hover(hoverIn, hoverOut);
                this.pie.hoverLabel(hoverIn, hoverOut);
            }

            // IE6 FIX, so that you can click the pie-pieces. The problem is our links are on the form "javascript:xxx", which doesn't seem to work in IE6's VML. This fix converts these href's to onclicks in the DOM. 
            // (i hope it does not get overwritten if raphael decides to refresh the DOM... but it seems to work consistently and is better than nothing. Maybe it can be set on the javascript objects instead before raphael generates the DOM elements?)
            // (Note, you still can't click on the names below the piechart)
            if (Raphael.vml && isIE6) { // Raphael uses VML instead of SVG for IE6-8 (since IE6-8 doesn't support SVG)
                $('shape').each(function (i, shapeElement) {
                    if (typeof (shapeElement.href) !== "string") return true; // just in case
                    if (shapeElement.href.indexOf("javascript:") == 0) {
                        shapeElement.onclick = function () {
                            eval(shapeElement.href.substring(11)); // <- an abomination, but we got release soon :/
                        };
                    }
                });
            }

        }
        else { // Bar chart - not finished
            var marginLeft = 60;
            var marginRight = 30;
            var marginTop = 20
            var chartWidth = element.width() - marginLeft - marginRight;

            var valuesWithIndexes = $(values).map(function (i, e) { return { i: i, e: e }; });
            valuesWithIndexes.sort(function (a, b) { return ((a.e < b.e) ? 1 : ((a.e > b.e) ? -1 : 0)); });

            var valuesToAdd = [];
            var otherCount = 0;
            var otherSum = 0;

            var highestValue = valuesWithIndexes[0].e;
            var pixelPerUnit = chartWidth / highestValue;
            for (var i = 0; i < valuesWithIndexes.length; i++) {
                if (valuesWithIndexes[i].e * pixelPerUnit < 2 || i > 8) {// Less than 10 pixels is put into others
                    otherCount++;
                    otherSum += valuesWithIndexes[i].e;
                }
                else {
                    valuesToAdd.push(valuesWithIndexes[i]);
                }
            }

            var added = $(valuesToAdd).map(function (i, e) { return e.e; });
            var addedTitles = $(valuesToAdd).map(function (i, e) { return legends[e.i]; });;

            if (otherCount > 0) {
                added.push(otherSum);
                addedTitles.push("Others (" + otherCount + ")");
            }

            var barHeight = 25; // KV: This does not translate directly to pixels
            var elementHeight = added.length * barHeight;
            element.height(elementHeight);
            var r = Raphael(elementId);

            this.pie = r.hbarchart(marginLeft, marginTop, chartWidth, elementHeight - marginTop, added, { type: "soft" })
            .hover(function () {
                this.flag = r.popup(this.bar.x, this.bar.y, this.bar.value || "0").insertBefore(this);
            }, function () {
                this.flag.remove();
            })
            .label(addedTitles, false);
        }

    };

    this.stopSpinner = function () {
        $(".loadingspinner", this.container).hide();
    };

    this.startSpinner = function () {
        $(".loadingspinner", this.container).show();
    };

    this.setError = function (msg) {
        //this.container.empty();
        //this.container.text(msg);
        $("#chartcontent", this.container).hide();
        this.errorMessageContainer.show();
        this.errorMessageContainer.text(msg); // could perhaps store the msg in chartContainer instead (like "no data to show") (prob: can see empty filters, etc)
    };

    // Misc functions
    this.sendConfigurationToServer = function () {
        sendWidgetConfiguration(this.id, this.configuration);
    };

    this.toggleFilterValue = function (index, value) {
        var order = this.columnIndexToOrder(index);

        var exists = false;
        for (var i = 0; i < this.configuration.filters[order].values.length; i++) {
            if (this.configuration.filters[order].values[i] == value)
                exists = true;
        }

        if (exists) {
            this.configuration.filters[order].values = $.grep(this.configuration.filters[order].values, function (n, i) { return n != value; });
        }
        else {
            this.configuration.filters[order].values.push(value);
        }

        // this.setGroupFromActiveFilters(); //KV: Should we do this?
        this.updateColumns(index);
        this.invokeDataService();
        this.sendConfigurationToServer();

        // track the event
        var reportId = this.configuration.definitionId;
        var widgetUId = $("#" + this.id).data("uid");
        if (typeof(PiwikUtil) === "object") PiwikUtil.widget.piechart.trackSetFilterEvent(this.id, widgetUId, reportId, this.getDefinitionName(reportId), this.columns[order].columnHeader, this.configuration.filters[order].values);
    };


    this.columnIndexToOrder = function (index) {
        var order;
        for (order = 0; order < this.configuration.filters.length; order++) {
            if (this.configuration.filters[order].index == index) {
                break;
            }
        }
        return order;
    };

    this.columnOrderToIndex = function (order) {
        return this.configuration.filters[order].index;
    };


    this.defaultFilter = function () {
        if (this.columns && this.columns.length > 0) {
            var filter = [];
            for (var i = 0; i < this.columns.length; i++) {
                filter[i] = { index: i, values: [] };
            }
            return filter;
        }
        return [];
    };

    this.currentLevel = function () {
        for (var i = 0; i < this.configuration.filters.length; i++) {
            if (this.configuration.filters[i].index == this.configuration.group) {
                return i;
            }
        }
        return 0;
    };

    this.getLabel = function (i) {
        return this.pie.labels.items[i][1].attrs.text;
    };


    this.setFilter = function(order, filter) {
        this.configuration.filters[order].values = filter;

        // track the event
        var reportId = this.configuration.definitionId;
        var widgetUId = $("#" + this.id).data("uid");
        if (typeof(PiwikUtil) === "object") PiwikUtil.widget.piechart.trackSetFilterEvent(this.id, widgetUId, reportId, this.getDefinitionName(reportId), this.columns[order].columnHeader, this.configuration.filters[order].values);
    };

    this.setGroupFromActiveFilters = function () {
        var lastActiveFilter = -1;

        for (var i = 0; i < this.configuration.filters.length; i++) {
            if (this.configuration.filters[i].values.length != 0)
                lastActiveFilter = i;
        }

        var nextFilter = Math.min(lastActiveFilter + 1, this.configuration.filters.length - 1); // Do not pass last filter
        this.configuration.group = this.columnOrderToIndex(nextFilter);
    };

    this.sliceLink = function (label, order) {
        function stripPct(str) {
            if (str.toLowerCase().startsWith("other"))
                return "others";

            // The format used by labels that are displayed.
            var format = /^(\d{0,3}(?:\.\d)?%)\s+.\s+(.*)$/i;

            var matches = str.match(format);
            return matches[2];
        }

        var selectedValue = "";
        if (this.configuration.charttype == "pie")
            selectedValue = stripPct(this.getLabel(label));
        else if (this.configuration.charttype == "table")
            selectedValue = label;
        else
            throw "This should not happen!";

        if (selectedValue != "others") {
            this.setFilter(order, [selectedValue]);
            this.setGroupFromActiveFilters();
            this.updateColumns(this.columnOrderToIndex(order));
            this.invokeDataService();
            this.sendConfigurationToServer();
        }
        else { // Others has been clicked show the "dive in"
            // Find currently shown labels.
            var currentLabels = [];

            var labels = this.pie.labels;
            for (var i = 0; i < labels.length; i++) {
                var label = this.getLabel(i);

                if (label.toLowerCase() == "others") continue;
                currentLabels.push(stripPct(label));
            }

            for (i in currentLabels) {
                this.exclude.push(currentLabels[i]);
            }
            this.updateData();
        }

    };

    this.reorderFilters = function (order) {
        var newFilters = [];

        for (var i = 0; i < this.configuration.filters.length; i++) {
            newPos = $.inArray(this.configuration.filters[i].index.toString(), order);
            newFilters[newPos] = this.configuration.filters[i];
        }

        this.configuration.filters = newFilters;
    };

    this.getColumnOrderFromContainer = function () {
        var order = [];

        var hash = $(".filter", this.columnsContainer);
        for (var i = 0; i < hash.length; i++) {
            order[i] = $(hash[i]).attr('id');
        }
        return order;
    };

    this.getOrder = function () {
        return $.map(this.configuration.filters, function (n) { return n.index; });
    };

    this.removeFilters = function (fromOrder, toOrder) {
        for (var i = fromOrder; i < toOrder; i++) {
            this.setFilter(i, []);
        }
        this.exclude = [];
    };

    // Collects all possible filters, and returns them, including whether they are currently active
    this.getFilterValueList = function (index) {
        function existsInValuesList(values, valueName) {
            // KV: Binary search based on http://www.dweebd.com/javascript/binary-search-an-array-in-javascript/
            var low = 0, high = values.length - 1,
                i, comparison;
            while (low <= high) {
                i = Math.floor((low + high) / 2);
                if (values[i].item < valueName) { low = i + 1; continue; };
                if (values[i].item > valueName) { high = i - 1; continue; };
                return true;
            }
            return false;

        }

        var values = [];
        var activeFilterValues = this.configuration.filters[this.columnIndexToOrder(index)].values;
        activeFilterValues.sort();
        for (var i = 0; i < activeFilterValues.length; i++) {
            values.push({ selected: true, item: activeFilterValues[i] });
        }

        var filtersFromColumnsService = this.columns[index].filterValues;
        filtersFromColumnsService.sort();
        for (var i = 0; i < filtersFromColumnsService.length; i++) {
            var item = filtersFromColumnsService[i];

            if (!existsInValuesList(values, item))
                values.push({ selected: false, item: item });
        }
        return values;
    };
    
}

// TODO: Move into class 
// Make into bogus comma, so we can replace on real commas when these values are used in the UI
function makeBogusComma(str) {
    return str.replace(/,/g, "\u00b8");
}

function getChartFilterString(filters) {
    var filter = "";
    if (filters.length) {
        for (f in filters) {
            filter += filters[f].index + "=";
            for (v in filters[f].values) {
                var value = filters[f].values[v];
                value = value.replace(/\u00b8/g, "\\,").replace(/;/g, "\\;").replace(/\\=/g, "=").replace(/=/g, "\\=");
                filter += value + ",";
            }
            filter += ";";
        }
    }
    return escape(filter);
}


function FixRaphael(values, legends) {
    //Fix Raphael error when only one element in pie chart
    if (values && values.length == 1) {
        values[1] = values[0] / 10000;
        legends[1] = "Other";
    } else {
        var allHasValue = true;
        var sum = 0;
        for (var i = 0; i < values.length; i++) {
            var value = values[i];
            if (value) {
                sum += value;
            } else {
                allHasValue = false;
            }
            if ((legends.length > i) && (!legends[i] || legends[i] == "Undefined" || legends[i] == "undefined")) {
                legends[i] = "Unspecified";
            }
        }
        if (!allHasValue) {
            var fraction = sum ? sum / 10000 : 0;
            for (var i = 0; i < values.length; i++) {
                var value = values[i];
                if (!value) {
                    values[i] = fraction;
                }
            }
        }
    }
    //Fix complete
}