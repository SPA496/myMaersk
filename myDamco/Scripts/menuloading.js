// KV: This file is included by Dashboard/getMenu. It is the main javascript code for loading in the menu on external sites.
// It also asumes the presence of the "myDamcoTopbarHTML", "basepathMyDamcoTopBar", "menuscriptMyDamcoTopBar" variables/functions, which contains all the HTML/Scripts of the topbar.

// Always load JQuery (etc) so we are sure we have a full version with button and dialog
getscriptMyDamcoTopBar(basepathMyDamcoTopBar + "Navigation/ExternalMenuJS/?" + myDamcoLastJSChange, function () {
    var scripts = mydamcoScriptsInit();
    var myDamcoJQuery = scripts.jquery;
    var myDamcoMoment = scripts.moment;

    myDamcoJQuery(function (myDamcoJQuery) {
        loadMyDamcoTopBar(myDamcoJQuery, myDamcoMoment);
    });
});

// Insert DOM elements
function loadMyDamcoTopBar($, moment) {
    $("#myDamcoADFSLoginHack").remove();

    if ($("#myDamcoTopbar").size() == 0) {
        $("body").prepend('<div id="myDamcoTopbar"></div>');
    }

    $("head").append('<link rel="stylesheet" type="text/css" href="' + basepathMyDamcoTopBar + 'Content/menu.css?' + lastCSSChange + '" />');
    $("head").append('<link rel="stylesheet" type="text/css" href="' + basepathMyDamcoTopBar + 'Content/maersk-theme.css?' + lastMtCSSChange + '" />');

    // TODO: Be smarter about how we include the jquery-ui css as it might already be included
    // IDEA? Could maybe generate a custom version of the jquery-ui.min.css file in which all selectors have been prefixed by some class/id (e.g. ".myDamcoNav" or "#myDamcoNav"), so that they only match our elements. (Maybe create an action which does the preprocessing and caches the result forever) (Problem: Their css-rules might still get through to our html. Might be possible to CSS Reset it?)
    //$("head").append('<link rel="stylesheet" type="text/css" href="' + basepathMyDamcoTopBar + 'Content/themes/base/minified/jquery-ui.min.css" />');
    $("#myDamcoTopbar").html(myDamcoTopbarHTML);

    // Fallback function
    if ($("#MyDamcoMenu_navigation .active").length == 0) {
        if (document.URL.indexOf("damco.com/AMI") != -1) { // We are in dynamic flow Control
            $("#MyDamcoMenu_navigation .DYNAMICFLOWCONTROL").addClass("active");
        }
        else if (document.URL.indexOf("Reporting/page?action=extract_center") != -1) { // In reporting
            $("#MyDamcoMenu_navigation .REPORTING").addClass("active");
        }
        else if (document.URL.indexOf("reporting.damco.com/Reporting/page") != -1) { // In reporting
            $("#MyDamcoMenu_navigation .REPORTING_TRACKTRACE").addClass("active");
        }
    }

    menuscriptMyDamcoTopBar($, moment);
}

// Utility function for loading scripts
function getscriptMyDamcoTopBar(url, success) {
    var script = document.createElement('script');
    script.src = url;

    var head = document.getElementsByTagName('head')[0],
		done = false;

    // Attach handlers for all browsers
    script.onload = script.onreadystatechange = function () {
        if (!done && (!this.readyState || this.readyState == 'loaded' || this.readyState == 'complete')) {
            done = true;
            success();
            script.onload = script.onreadystatechange = null;
            head.removeChild(script);
        };
    };
    head.appendChild(script);
};