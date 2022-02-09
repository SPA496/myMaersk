/** Todo? Maybe use the listener pattern instead of this, so that the other js-files doesn't have to know about piwik? (Still need to send the correct params along)
    (Also, must be registered with all instances of f.e. widets (+ any new added widgets), or a "static" listener shared by all instances of a widget)
    (Have to be sure the other files are loaded before i begin attaching listeners) (Is it too complex/inddirect for myDamco?) 
     (Can use custom jquery events? Probably don't need to be able to access any methods, only a string...)

    Piwik JS-tracking API: https://piwik.org/docs/javascript-tracking/
    (Clever way to use getter-functions: _paq.push([ function() { var customVariable = this.getCustomVariable(1); }]); )
    */
var PiwikUtil = function () {

    /** Generic trackEvent function - contains the basic format for the title we use for the fake pageview when tracking events */
    function trackEvent(eventInfoArray) {
        $(function () { // This is to make sure the page has fully loaded, so that _paq has been defined (and piwikConstants).
            trackEvent_dontWaitForDOMReady(eventInfoArray);
        }); 
    }
    
    function trackEvent_dontWaitForDOMReady(eventInfoArray) {
        if (typeof(_paq) === "undefined") return; // abort if _paq not defined (piwik disabled) to avoid js error - the event is lost.

        // Note: This will also send any custom variables from the 'real' pageview along with the event (defined on page-load in _Piwik.cshtml) as well as any custom variables defined later.
        _paq.push(['trackPageView', 'Events/' + eventInfoArray.join("/")]);
    }

    /** Track a widget-instance related event - this also sends the widgetInstanceId along as a custom variable (so that any eventual plugins we later make, can see which events belongs to the same instances) */
    function trackWidgetInstanceEvent(widgetInstanceId, widgetUId, eventInfoArray) {       
        $(function () { // This is to make sure the page has fully loaded, so that _paq has been defined (and piwikConstants).

            if (typeof (_paq) === "undefined" || typeof (piwikConstants) === "undefined") return; // abort if still not defined (piwik disabled) to avoid js error - the event is lost.

            if (typeof (widgetInstanceId) === "string" && widgetInstanceId.substring(0, 6) === "widget" && widgetInstanceId.length > 6) // remove "widget" prefix from the id, if present
                widgetInstanceId = widgetInstanceId.substring(6, widgetInstanceId.length);
            
            var wiCustvar = piwikConstants.customVariables.widgetInstanceId;
            var wCustvar  = piwikConstants.customVariables.widgetUId;
            _paq.push(['setCustomVariable', wiCustvar.id, wiCustvar.name, widgetInstanceId, wiCustvar.scope]);
            _paq.push(['setCustomVariable', wCustvar.id, wCustvar.name, widgetUId, wCustvar.scope]);
            trackEvent_dontWaitForDOMReady(eventInfoArray);
            _paq.push(['deleteCustomVariable', wiCustvar.id, wiCustvar.scope]); // dont send it along with the next event
            _paq.push(['deleteCustomVariable', wCustvar.id, wCustvar.scope]);   // --||--
        });
    }

    /** Track the Change Role event, when a user clicks a button in the Change Role dialog 
     *  (is currently not tracked when the menu is included on external sites - the event should be done serverside, if we want to track that) 
     */
    function trackChangeRoleEvent(roleId, roleName, roleOrgName) {
        trackEvent(["Role", "Change Role", roleName + " (id: " + roleId + ") (org: " + roleOrgName + ")"]);
    }

    /** Track the Change Password event, when a user changes his password on the Account page. (tracks if it succeeded or failed) */
    function trackChangePasswordEvent(success) {
        trackEvent(["Account", "Change Password", success]);
    }

    /** Track the navigation link clicks in the navigation menu */
    function trackNavigationLinkClickEvent(applicationName) {
        trackEvent(["Navigation", "Click Link", applicationName]);
    }

    /** Generic function for all pie-chart events */
    function trackPieChartEvent(widgetInstanceId, widgetUId, additionalInfoArray) {
        trackWidgetInstanceEvent(widgetInstanceId, widgetUId, ["Widgets", "Performance Chart"].concat(additionalInfoArray));
    }

    /** Track when a new report is selected in a pie chart */
    function trackPieChartSelectReportEvent(widgetInstanceId, widgetUId, selectedReportId, selectedReportName) { // also oldReportName?
        trackPieChartEvent(widgetInstanceId, widgetUId, ["Select Report", selectedReportName + " (id: " + selectedReportId + ")"]); // other way around? id first, then report name?
    }

    /** Track when the "see report" link is clicked */
    function trackPieChartSeeReportClickEvent(widgetInstanceId, widgetUId, reportId, reportName) { // TODO: Click problem (left, middle, keyboard, contextmenu): http://stackoverflow.com/questions/8927208/catching-event-when-following-a-link
        trackPieChartEvent(widgetInstanceId, widgetUId, ["Click See Report Link", reportName + " (id: " + reportId + ")"]); // other way around? id first, then report name?
    }

    /** Track when the a filter is "set" for a column in the pie chart - not that set filter removes all previous filters */
    function trackPieChartSetFilterEvent(widgetInstanceId, widgetUId, reportId, reportName, /*order,*/ columnName, filterValues) { // TODO: Is this event really useful? (Or maybe he wants full filter-values for all columns each time, not just the one we click? Could use "getChartFilterString(this.configuration.filters)" for that (or make my own))
        trackPieChartEvent(widgetInstanceId, widgetUId, ["Set Filter", reportName + " (id: " + reportId + ")", /*orderId,*/ columnName, filterValues != "" ? filterValues : "<none>"]);
    }

    /** Track when a video is shown in e-learning */
    function trackElearningShowEvent(widgetInstanceId, widgetUId, appName, videoName) {
        trackWidgetInstanceEvent(widgetInstanceId, widgetUId, ["Widgets", "E-Learning", "Show", appName, videoName]);
    }

    /** Generic function for all rss-news events */
    function trackRssNewsEvent(widgetInstanceId, widgetUId, widgetName, additionalInfoArray) {
        trackWidgetInstanceEvent(widgetInstanceId, widgetUId, ["Widgets", "News", widgetName].concat(additionalInfoArray));
    }

    /** Track when a newsitem is clicked in the rss-news widgets */
    function trackRssNewsItemClickEvent(widgetInstanceId, widgetUId, widgetName, newsFeedName, newsItemName) { // TODO: Click problem (left, middle, keyboard, contextmenu): http://stackoverflow.com/questions/8927208/catching-event-when-following-a-link
        trackRssNewsEvent(widgetInstanceId, widgetUId, widgetName, ["Click News Link", newsFeedName, newsItemName]);
    }

    /** Track when a newscategory/newsfeed is selected (manually by the user) in the rss-news widgets */
    function trackRssNewsSelectFeedEvent(widgetInstanceId, widgetUId, widgetName, newsFeedName) {
        trackRssNewsEvent(widgetInstanceId, widgetUId, widgetName, ["Select Feed", newsFeedName]);
    }

    /** Track link click in the tabular widgets (Note: This could also be done server-side in the WidgetLink action-method instead - might be more roboust) */
    function trackTabularLinkClickEvent(widgetInstanceId, widgetUId, widgetName, reportName) { // TODO: Click problem (left, middle, keyboard, contextmenu): http://stackoverflow.com/questions/8927208/catching-event-when-following-a-link
        trackWidgetInstanceEvent(widgetInstanceId, widgetUId, ["Widgets", widgetName, "Click Link", reportName]);          //       Could be solved by doing tracking serverside. (Can only be almost solved on javascript side - but maybe it is sufficient?)
    }

    /** Generic function for non widget-instance specific widget events (general widget events) */
    function trackGeneralWidgetEvent(widgetInstanceId, widgetUId, additionalInfoArray) {
        trackWidgetInstanceEvent(widgetInstanceId, widgetUId, ["Widgets", "General"].concat(additionalInfoArray));
    }

    /** Track when the user adds a widget to his dashboard */
    function trackAddWidget(widgetInstanceId, widgetUId) { // More info about the widget/user? Probably not needed for the user, since his name/role/organization is already in a custom variable.
        trackGeneralWidgetEvent(widgetInstanceId, widgetUId, ["Add Widget", widgetUId]);
    }

    /** Track when the user removes a widget from his dashboard */
    function trackRemoveWidget(widgetInstanceId, widgetUId) { // More info about the widget/user? Probably not needed for the user, since his name/role/organization is already in a custom variable.
        trackGeneralWidgetEvent(widgetInstanceId, widgetUId, ["Remove Widget", widgetUId]);
    }

    return {
        trackChangeRoleEvent: trackChangeRoleEvent,
        trackChangePasswordEvent: trackChangePasswordEvent,
        trackNavigationLinkClickEvent: trackNavigationLinkClickEvent, 
        widget: {
            trackAddWidget: trackAddWidget,
            trackRemoveWidget: trackRemoveWidget,
            piechart: {
                trackSelectReportEvent: trackPieChartSelectReportEvent,
                trackSeeReportClickEvent: trackPieChartSeeReportClickEvent,
                trackSetFilterEvent: trackPieChartSetFilterEvent
            },
            elearning: {
                trackShowEvent: trackElearningShowEvent
            },
            rssnews: {
                trackItemClickEvent: trackRssNewsItemClickEvent,
                trackSelectFeedEvent: trackRssNewsSelectFeedEvent
            },
            tabular: {
                trackLinkClickEvent: trackTabularLinkClickEvent
            }
        }
    };

}();