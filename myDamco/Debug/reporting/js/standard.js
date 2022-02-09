var runlocal = false;
var URL_RUNLOGO='images/logorun.gif';
var FRAMENAME;
var resizeAction;
function setFRAMENAME(f) {
    FRAMENAME = f;
}
$(function() {

    $('table[remdob]').each(function() {
        var data = new Array();
        var remd = parseInt($(this).attr('remdob'), 10);
        $(this).find('tr:gt(0)').each(function() {
            var tds = $(this).find('td');
            for (var i = 0; i < remd; i++) {
                if (data.length > i && data[i] == tds.eq(i).text())
                    tds.eq(i).text('-');
                else
                    data[i] = tds.eq(i).text();
            }
        });
    });
    $('input.text').keypress(
        function(evt) {
            return isTextValid(evt);
        }).blur(function() {
            var nows = $(this).val();
            $(this).val(makeStringValid(nows));
            if ($(this).val() != nows) {
                $(this).addClass('error').attr('title', 'Some illegal charters is removed from the text');
            }
        });
    if ($('iframe.fullheight').length > 0) {
        resizeAction = window.setInterval(iFrameHeight, 1500);
    }
    if ($('iframe.topbutton').length > 0) {
        $(window).resize(function() {
            iFrameTopButton();
        });
        iFrameTopButton();
    }

    $('div.section').each(function() {
        var $icon = $(this).find('td.section-icon img').butclick();
        $(this).find('table.section-header:eq(0)').mouseover(
            function() {
                $icon.addClass('show');
            }).mouseout(function() {
                $icon.removeClass('show');
            });

        var $div = $(this).find('div.secbody:eq(0)');

        $icon.click(function() {

            $img = $(this);
            if ($img.hasClass('plus')) {
                $img.removeClass('plus').addClass('minus');
                $div.show();
            } else {
                $img.removeClass('minus').addClass('plus');
                $div.hide();
            }
        });
    });


    $('select[xvalue]').each(function() {
        $(this).val($(this).attr('xvalue'));

    });


    $('input[type=checkbox]').click(function() {
        if (this.checked)
            this.value = 1;
        else
            this.value = 0;
    });

    $('input.text').focus(
        function() {
            $(this).addClass('activ');
        }).blur(function() {
            $(this).removeClass('activ');
        });

    $('input.mandatory').keyup(
        function() {
            mandatory($(this));
        }).blur(
        function() {
            mandatory($(this));
        }).focus(
        function() {
            mandatory($(this));

        });
    /*.each(function() {
     $(this).parent().attr('title', 'This field is mandatory - accepted length > ' + minlength($(this)));
     }); */
    function minlength(obj) {
        var ln = obj.attr('ln');
        if (!ln)
            ln = 0;
        return ln
    }

    function mandatory(obj) {
        var fldVal = obj.val().split(' ').join('');
        if ((fldVal != '') && (obj.val().length > minlength(obj)))
            obj.removeClass('error');
        else
            obj.addClass('error');
    }

    $('#message.error').each(function() {
        gui_makeerror($(this).html());

    });
    $('#message.message').each(function() {
        gui_makestatment($(this).html());

    })


});
function iFrameTopButton() {


    $('iframe.topbutton').each(function() {
        var h = $(this).position().top;
        $(this).height($(window).height() - (h + 10));

    })
}

function iFrameHeight() {


    $('iframe.fullheight').each(function() {
        try {
            var dx=$(this).contents().find('body').get(0).scrollHeight;
            if($(this).data('dx')!=dx){
            var h = $(this).contents().find('body').get(0).scrollHeight + 20;
            $(this).height(h > 200 ? h : 200);
            }

        } catch(a) {
        }
        $(this).data('dx',$(this).height());
    })
}
function resizeframe(id) {

    $('#' + id).each(function() {
        try {
            var h = $(this).contents().find('body').get(0).scrollHeight + 20;
            var hx = $(window).height() - ($(this).position().top + 10);
            if (hx > h)
                h = hx;
            $(this).height(h > 200 ? h : 200);

            parent.resizeframe(FRAMENAME);
        } catch(a) {
        }
    })

}
function resizemenu(id, h) {
    try {
        if (h == null) {
            $('#' + id).each(function() {

                var h = $(this).contents().find('body').get(0).scrollHeight;

                $(this).height(h);

            })
        } else {
            $('#' + id).height(h);
        }
    } catch(a) {
    }
}
var framename;

function frameheight_ini(name) {
    framename = name;

}

function menuheight() {
    parent.resizemenu(FRAMENAME);
}
function frameheight() {
    try {
        //parent.resizeframe(FRAMENAME);
    } catch(a) {
    }
}

function read_form_url(form) {
    var ss = '';
    var valid = true;
    form.find('input[name!=""]').each(function() {
        if (!check_mandatory($(this))) {
            var fname = $(this).attr('name');
            if ($(this).attr('cap') != '')
                fname = $(this).attr('cap');
            alert('Data is missing in ' + fname);
            $(this).trigger('focus');
            valid = false;
            return false;
        }
        ss = ss + '&' + $(this).attr('name') + '=' + $(this).val();
    });

    if (!valid) {
        return '';
    }
    form.find('select[name!=""]').each(function() {
        var data = new Array();
        if ($(this).attr('size') == '1') {
            if (this.selectedIndex == -1) {
                alert('Select data before submit');
                $(this).trigger('focus');
                valid = false;
                return false;
            }
            ss = ss + '&' + $(this).attr('name') + '=' + $(this).val();
        } else {

            $(this).find('option').each(function() {
                data[data.length] = $(this).attr('value');
            });
        }
        ss = ss + '&' + $(this).attr('name') + '=' + data.join(',');
    });
    if (!valid) {
        return '';
    }
    return ss;
}

function read_form_xml(form) {
    var valid = true;
    var ss = '<data>';
    form.find('input[name!=""]').each(function() {
        if (!check_mandatory($(this))) {
            alert('Data is missing in ' + $(this).attr('name'));
            $(this).trigger('focus');
            valid = false;
            return;
        }

        ss = ss + '<' + $(this).attr('name') + '>' + encodeURIComponent($(this).val()) + '</' + $(this).attr('name') + '>';
    });

    if (!valid)
        return '';
    form.find('select[name!=""]').each(function() {
        ss = ss + '<' + $(this).attr('name') + '>';
        $(this).find('option').each(function() {

            ss = ss + '<no>' + $(this).attr('value') + '</no>';

        });
        ss = ss + '</' + $(this).attr('name') + '>';
    });
    ss += '</data>';
    return ss;
}


function element_validate() {
    $('input[type=text]').focus(
        function() {
            $(this).removeClass('error');
            $(this).addClass('activ');
        }).blur(function() {
            $(this).removeClass('activ');
            if (!check_mandatory($(this)))
                $(this).addClass('error');

        });
}

function element_form_ini() {
    element_validate();
    $('form input[type=checkbox]').each(function() {
        $(this).click(function() {
            if (this.checked) {
                this.value = 1;
            }
            else {
                this.value = 0;
            }
        });
    });
    $('form').submit(function() {

        var knap = $(this).find('input[type=submit]');

        if ($(this).attr('fmethod') != null) {
            var ss = read_form_xml($(this));

            return ss != '';

        }
        if ($(this).attr('post')) {
            ss = read_form_xml($(this));
            gui_jdebug(ss);
            if (ss != '')
                postaction(ss, knap, $(this).attr('action'));
        } else {

            ss = read_form_url($(this));
            gui_jdebug(ss);
            if (ss != '') {
                var url = 'page?action=' + $(this).attr('action') + ss;
                submitaction(url, knap);
            }
        }
        return false;
    });


}
function runBut(button) {
    if (button.data('run')) {
        button.removeData('run').removeClass('submitrun').removeClass('wait');

    }
    else {
        button.data('run', '1');
        if (button.hasClass('submit'))
            button.addClass('submitrun');
        else
            button.addClass('wait');
    }
}
function submitaction(URL, button) {
    if (!button.data('run')) {
        runBut(button);
        gui_makestatment();
        getXML(URL, button, function(xml, obj) {
            runBut(obj);
            if (xmlError(xml)) {
                respondSave($(xml));
            }

        }, function () {
            runBut(obj);
            gui_makeerror('Cannot get in contact with server');

        });
    }
}
function respondSave(xml) {
    gui_jdebug(xml.xml);
    if (xml.find('optlock').length > 0) {
        $('input[name=optlock]').val(xml.find('optlock').text());
    }
    if (xml.find('return_script').length)
        eval(xml.find('return_script').text() + '($(xml))');
    if (xml.find('msgbox').length > 0) {
        gui_makestatment(xml.find('msgbox').text());
    }
    if (xml.find('error').length > 0) {
        gui_makeerror(xml.find('error').text());
    }
    if (xml.find('return').length > 0)
        location.href = xml.find('return').text();


}
function postaction(data, button, action) {
    if (!button.hasClass('wait')) {
        gui_makestatment();
        runBut(button);
        $('#javadebug').val($('#javadebug').val() + data);
        $.post('ajax', {  'data': data,'action':action},
            function(xml) {
                if (xmlError(xml)) {
                    respondSave($(xml));

                }
                runBut(button);
            },

            'xml');
    }
}
var loadobj;
var global_floatdiv;
//*******************tabfunction


function gui_tab(obj, frameresize) {

    if (obj == null)
        obj = '';
    $(obj + ' div.tabbar').each(function() {
        var Q_tb = $(this);
        var Q_tabs = Q_tb.find('li[tab]');
        Q_tabs.data('tabs', Q_tabs);

        Q_tabs.click(function() {
            gui_tabclick($(this), frameresize);


        })
            .mouseover(function() {
                if ($(this).hasClass('tab'))
                    $(this).addClass('hover');
            })
            .mouseout(function() {
                $(this).removeClass('hover');
            });

        Q_tabs.each(function() {
            $($(this).attr('tab')).hide();

        });


        gui_tabclick(Q_tabs.eq(0), frameresize);
        $(Q_tabs.eq(0).attr('tab')).show();
        //if($(Q_tabs.get(0).tab).length>0){
        // $(Q_tabs.get(0).tab).show();
        //}
    });
}

function gui_tabclick(tab, frameresize) {

    tab.data('tabs').each(function() {
        if ($(this).attr('tab') != tab.attr('tab'))
            $($(this).removeClass('activ').attr('tab')).hide();
        else
            $(tab.addClass('activ').attr('tab')).show();

    });
    if (tab.data('focus') != null)
        tab.data('focus').trigger('focus');
    if (tab.data('func') != null) {
        tab.data('func')();
    }
    if (frameresize != null)
        frameheight();
}

function gui_jdebug(ss) {
    try {
        if (parent != this)
            parent.gui_jdebug(ss);

        $('#javadebug').val($('#javadebug').val() + '\n     ' + ss);
    } catch(a) {
    }
}


//*********************************************************************************************


//**********************************************************************************************

function gui_makeerror(message, func) {
    if ($('#message').length==0&&window.parent!=this){
		 try {
			window.parent.gui_makeerror(message, func);
            return;
        } catch(a) {
        }
	}
    if (message == null || message == '') {
        $('#message').hide();
        return;
    }

    $('#message').attr('class', 'errorbox').html(message);
    if ($('#message:hidden').length == 1)
        $('#message').slideDown(function() {

            if (func != null) func($(this));
        });
    else
    if (func != null) func($(this));
}

function gui_makestatment(message, func) {
    if (message == null || message == '') {
        $('#message').hide();
        return;
    }
    $('#message').attr('class', 'message').html(message).slideDown(function() {

        if (func != null) func($(this));
    });
}
function gui_hidemessage($message) {
    $message.hide();
}


function gui_showmessage(message_id, ss, func) {
    var $message = $('#' + message_id);
    if (ss == null)
        ss = $message.text();
    $message.html(ss);
    if (ss.length == 0)
        $message.hide();
    else {
        $message.slideDown(function() {
            $message.addClass('show');
            if (func != null)
                func();
        });
    }
}


$.fn.xshow = function() {
    for (var i = 0, l = this.length; i < l; i++) {
        this[i].style.display = '';
    }

    return this;

};
function gui_makehovertext() {
    $('div.x_hover').mouseover(
        function() {
            var l = findp_length($(this));

            if (l < $(this).width() - 20) {
                $(this).addClass('expandtext');
                $(this).width(l);
            }
        }).mouseout(function() {
            $(this).removeClass('expandtext');
            $(this).width('100%');
        });
}

function findp_length($cell) {
    var $p = $cell.parents('td.exinfo:eq(0)');
    return $p.width() - ($p.find('img').length + 1) * 20
}

function element_lookup_ini() {
    $('div.search').each(function() {
        var divsearch = $(this);
        var knap = $(this).find('input[type=image].search');
        var input = $(this).find('input.searchinput');
        var select = $(this).find('select:not(.searchinputresult)');
        var run = $(this).find('span:eq(0)');
        var resultcombo = $(this).find('select.searchinputresult');
        if (resultcombo.length > 0) {
            divsearch.data('result', resultcombo);
        }
        var URL = "";
        $(this).parents('form:eq(0)').submit(function() {
            return false;
        });
        input.keydown(
            function(e) {
                if (e.keyCode == 13) {
                    knap.trigger('click');
                }
            }).blur(function() {
                knap.trigger('click');
            });
        resultcombo.change(function() {
            var title = $(this).find('option:selected').attr('title');
            if (title != '')
                $(this).attr('title', title);
        });
        select.change(function() {

            URL = '?action=' + $(this).attr('name') + '&search=' + $(this).val();

            makelookup(divsearch);

        });

        knap.butclick().click(function() {
            if ($(this).hasClass('wait'))
                return false;
            if ($(this).attr('url') == '') {
                URL = '?action=' + $(this).attr('xname');
            } else {
                URL = '?' + $(this).attr('url');
            }
            input.each(function() {
                var name = '';
                if ($(this).attr('xname'))
                    name = $(this).attr('xname');
                else
                    name = $(this).attr('name');
                if (name != '')
                    URL = URL + '&' + name + '=' + escape($(this).val());

            });


            makelookup(divsearch);


            return false;
        });


        function makelookup(obj) {
            var resultobj = obj.data('result');

            knap.addClass('wait');
            run.addClass('wait');

            resultobj.html('');

            getXML('ajax' + URL, resultobj, function(xml) {

                if (xmlError(xml)) {
                    var items = $(xml).find('item');
                    var starttext = '**Select**';
                    if (items.length == 0)
                        starttext = '**No found**';
                    var options='';
                    if (resultobj.attr('size') == 1 && items.length != 1)
                         options=options+'<option>' + starttext + '</option>';

                    items.each(function() {
                        var title = '';
                        if ($(this).attr('title') != null)
                            title = $(this).attr('title');
                        options=options+'<option value="' + $(this).attr('key') + '" title="' + title + '">' + $(this).text() + '</option>';
                    });

                    resultobj.html(options).focus();
                }
                select.parent().removeClass('wait');
                knap.removeClass('wait');

            }, function () {
                run.removeClass('wait');
                knap.removeClass('wait');
                gui_makeerror('Cannot get in contact with server');
            });
        }

    });

}
function objvalArray(pObjects) {
    var arr = new Array();
    pObjects.each(function() {
        if (this.value.length > 0)
            arr[arr.length] = this.value
    });
    return arr;
}

function check_mandatory(obj) {
    if (obj.attr('mandatory') == null) {
        if (obj.val() == '')
            return false;
    }
    return true;
}
function ajaxXML(path, obj, returnfunc, errorfunc, valfunc) {
    errorfunc();
    if (!obj)
        obj = $('');
    obj.addClass('wait');
    getXML(path, obj, function(xml, obj) {
        if (valfunc(xml, errorfunc)) {
            returnfunc(xml, obj);
        }
        obj.removeClass('wait');
    }, function(error) {
        obj.removeClass('wait');
        if (error != null)
            errorfunc('Cannot get in contact with the server - please try again');

    });


}
function getXML(path, obj, returnfunc, errorfunc) {
    gui_jdebug(path);
    $.ajax({
        url: path,
        dataType: ($.browser.msie) ? "text" : "xml",
        success: function(data) {
            var xml;
            if (typeof data == "string") {
                xml = new ActiveXObject("Microsoft.XMLDOM");
                xml.async = false;
                xml.loadXML(data);
            } else {
                xml = data;
            }
            gui_jdebug(xml.xml);
            returnfunc(xml, obj);

        },

        error:function (XMLHttpRequest, textStatus, errorThrown) {

            errorfunc('Connection problems');
        }   })
}

function xmlMake(data) {
    var xml;
    if (typeof data == "string") {
        xml = new ActiveXObject("Microsoft.XMLDOM");
        xml.async = false;
        xml.loadXML(data);
    } else {
        xml = data;
    }
    return xml;


}
function xmlOK(xmlDoc, func) {
    var ss = '';
    if ($(xmlDoc).find('ok').length == 0)
        ss = 'No confirm of transaction success';
    return xmlError(xmlDoc, func, ss);
}


function xmlError(xmlDoc, func, txt) {

    var ss = '';
    if ($(xmlDoc).find('sessionout').length > 0) {
        location.reload();
        return false;

    }
    if (xmlDoc == null) {
        ss = 'Request did not return anything';
    } else {
        ss = $(xmlDoc).find('error').text();
    }
    if (ss == '' && txt != null)
        ss = txt;
    if (func == null) {
        gui_makeerror(ss);
    } else {
        func(ss)
    }

    return ss.length == 0;
}


function createCookie(name, value, days) {
    var expires = "";
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toGMTString();
    }

    document.cookie = name + "=" + value + expires + "; path=/";
}

function readCookie(name) {
    var nameEQ = name + '=';

    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

function eraseCookie(name) {
    createCookie(name, "", -1);
}
function clearCookieGraph(pre) {

    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(pre) == 0)  eraseCookie(c.split('=')[0]);
    }
    return null;
}

function fitframe(obj) {

    window.setInterval(res, 1000);
    function res() {
        $(obj).each(function() {
            var ex = 35;
            if ($.browser.msie)
                ex = 40;
            if (this.contentWindow.document != null) {
                var xx = this.contentWindow.document.body.scrollHeight + ex;
                if (this.contentWindow.fheight != null)
                    xx = this.contentWindow.fheight;
                this.style.height = xx + 'px';
            }
        })
    }

}

function makeStringValid(ss) {
    var txt = '';
    for (i = 0; i < ss.length; i++) {
        var charCode = ss.charCodeAt(i);
        if (charCode < 255) {
            txt += ss.charAt(i);
        }

    }
    return txt;
}

function isTextValid(evt) {
    var charCode = (evt.which) ? evt.which : window.event.keyCode;
    return ( charCode < 255);

}
function gui_download(){
     $('body').append('<iframe width="2" height="2" src="empty.html" name="_download" id="_download" frameborder="0"/>');
   $('#dialog_download img').click(function(){

	   $(this).parent().trigger('click');
   });
    $('td[download]').mouseover(function(){
          if($(this).data('dn')==null){



	var a='page?action=export&data=2&titles=1&sep=0&exid='+$(this).attr('download')+'&ftype=1';
	var b='page?action=export&grid=1&titles=1&filterinfo=1&exid='+$(this).attr('download')+'&ftype=2&sort=&filter=&remdub=0&colorder=';
		$(this).children(0).before('<span class="dnload" ><a href="'+a+'" target="_download" class="noicon"><img src="images/c.gif" class="zip but" title="Download csv report zipped"  /></a><a href="'+b+'" target="_download" class="noicon"><img src="images/c.gif" class="export but" title="Download report in Excel comptible format" /></a></span>');
		$(this).data('dn',$(this).find('span:eq(0)'));
		  }else{
			  $(this).data('dn').show();
		  }
    }).mouseleave(function(){
        $(this).data('dn').hide();
    });
}