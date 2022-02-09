// KV: This file is included by Dashboard/getMenu. 

// Always load JQuery so we are sure we have a full version with button and dialog
getscriptMyDamcoLogin(basepathMyDamcoTopBar + "Scripts/jquery-1.7.2.min.js", function () {
    var myDamcoJQuery = $.noConflict(true);
    myDamcoJQuery(mydamcoLogin);
}); 

// Insert DOM elements
function mydamcoLogin($) {
    if ($("#myDamcoADFSLoginHack").size() == 0) {
        $("body").prepend('<iframe id="myDamcoADFSLoginHack" style="display: none"></iframe>');
        var cnt = 0;
        var timeout = null;
        $("#myDamcoADFSLoginHack").load(function () {
            cnt++;
            if (cnt == 1) {
                timeout = setTimeout(
                    function () {
                        $("head").append('<script src="' + basepathMyDamcoTopBar + 'Navigation/Menu" />');
                    }, 5000);
            }
            else if (cnt == 2) {
                clearTimeout(timeout);
                $("head").append('<script src="' + basepathMyDamcoTopBar + 'Navigation/Menu" />');
            }
        }).attr('src', basepathMyDamcoTopBar + "Navigation/LoginDummy");
    }

}

// Utility function for loading scripts
function getscriptMyDamcoLogin(url, success) {
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