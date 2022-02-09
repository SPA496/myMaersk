// Apparantly MVC wants this specific format for receiveing datetime objets.
// KV: Found here: http://stackoverflow.com/questions/2573521/how-do-i-output-an-iso-8601-formatted-string-in-javascript  
function ISODateString(d) {
    var i = new Date(d);
    function pad(n) { return n < 10 ? '0' + n : n; }
    return i.getUTCFullYear() + '-'
    + pad(i.getUTCMonth() + 1) + '-'
    + pad(i.getUTCDate()) + 'T'
    + pad(i.getUTCHours()) + ':'
    + pad(i.getUTCMinutes()) + ':'
    + pad(i.getUTCSeconds());
}

function UnixTimeToUTCTime(t) {
    var dt = new Date(t),
        offset = dt.getTimezoneOffset();// returns minutes

    dt.setMinutes(dt.getMinutes() + offset);
    return dt;
}

function UTCTimeToUnixTime(dt) {
    var offset = dt.getTimezoneOffset();// returns minutes

    dt.setMinutes(dt.getMinutes() - offset);
    return dt.getTime();
}

function dateToDisplay(d) {
    if (d == "Invalid Date" || d == NaN || d == "NaN" || d == undefined)
        return "";
    return moment(d).utc().format('MM/DD-YYYY HH:mm') + " UTC";
}

function forceDatePickerNowButtonToUseUTC() {
    // This sets the time in UTC instead of localtime, when pressing the "now" button in any datepicker. This is run after the built-in inline "onclick" function on the now-button, which sets the time to the current time in localtime
    // instead of in UTC (as we want). This listener thus overrides the result of the built-in onclick with the current time in UTC instead.
    $('button.ui-datepicker-current').live('click', function () {
        $.datepicker._curInst.input.datepicker('setDate', UnixTimeToUTCTime(new Date().getTime()));
    });
    
    // IE6-8 DateTimePicker Now-button "fix" : I have been unable to get the "now" button to pick datetime in UTC on IE6-8, so for now i hide the "now" button in IE6-8 instead (This is a hack - would be nice to solve it properly).
    $('head').append("<!--[if lte IE 8]> <style type='text/css' id='IE6to8DateTimePickerNowButtonRemove'>button.ui-datepicker-current {display:none;}</style> -->");
}
