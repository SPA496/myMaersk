// KV: Added from http://stackoverflow.com/questions/610406/javascript-equivalent-to-printf-string-format
// Used like this "test{0}{1}".format("v0", "v1"). Is used to format the URL templates.
String.prototype.format = function () {
    var args = arguments;
    // Support using an array as input
    if (arguments.length == 1 && Object.prototype.toString.call(arguments[0]) === '[object Array]') {
        args = arguments[0];
    }
    
    return this.replace(/{(\d+)}/g, function (match, number) {
        return typeof args[number] != 'undefined'
      ? args[number]
      : match
    ;
    });
};

// KV: Send widget instance-configuration server.
function sendWidgetConfiguration(widgetId, config) {
    var id = widgetNameToId(widgetId);

    $.ajax({
        type: "POST",
        url: "Dashboard/SetWidgetConfiguration",
        traditional: true,
        data: { 
            id: id,
            configuration: JSON.stringify(config)
        }
    });
}


// KV: added to be able easily to JSONP-inject scripts.
function attachScript(serviceUrl, successcallback, failcallback) {
    $.jsonp({
        url: serviceUrl,
        callbackParameter: "functionName",
        success: function (json) {
            successcallback(json);
        },
        error: function (xOptions, textStatus) {
            failcallback();
        }
    });
}


function sum(values) {
    if (!values) return 0;
    var s = 0;
    for (var i = 0; i < values.length; i++) {
        s += values[i];
    }
    return s;
}

function getWidgetId(widget) {
    return widget.attr("id").match("\\d+$")[0];
}

// SetWidgetTitle (As JQuery plugin)
$.fn.setWidgetTitle = function(title, optErrorCallback) {
    $this = $(this);
    var oldTitle = $(".widget-title", $this).text();
    $.ajax({
        type: "POST",
        url: "Dashboard/SetWidgetTitle",
        data: {
            id: getWidgetId($this),
            title: title
        },
        error: function (xhr) {
            $(".widget-title", $this).text(oldTitle);
            if (optErrorCallback) optErrorCallback(xhr, oldTitle, title);
        }
    });
    $(".widget-title", $this).text(title);
};

// GetWidgetTitle (As JQuery plugin)
$.fn.getWidgetTitle = function() {
    $this = $(this);
    return $(".widget-title", $this).text();
};

// SetWidgetConfiguration (As JQuery plugin)
$.fn.setWidgetConfiguration = function(config) {
    $this = $(this);
    $.ajax({
        type: "POST",
        url: "Dashboard/SetWidgetConfiguration",
        data: {
            id: getWidgetId($this),
            configuration: JSON.stringify(config)
        }
    });
};

// HTML-escapes a string (see http://stackoverflow.com/a/5251551)
function escapeHTML(string) {
    var pre = document.createElement('pre');
    var text = document.createTextNode(string);
    pre.appendChild(text);
    return pre.innerHTML;
}

// KV: http://stackoverflow.com/questions/3629817/getting-a-union-of-two-arrays-in-jquery
function union_arrays(x, y) {
    var obj = {};
    for (var i = x.length - 1; i >= 0; --i)
        obj[x[i]] = x[i];
    for (var i = y.length - 1; i >= 0; --i)
        obj[y[i]] = y[i];
    var res = []
    for (var k in obj) {
        if (obj.hasOwnProperty(k))  // <-- optional
            res.push(obj[k]);
    }
    return res;
}

// KV: http://stackoverflow.com/questions/646628/javascript-startswith
if (typeof String.prototype.startsWith != 'function') {
    // see below for better implementation!
    String.prototype.startsWith = function (str) {
        return this.indexOf(str) == 0;
    };
}