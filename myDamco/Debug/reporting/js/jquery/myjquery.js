var moz = $.browser.mozilla && /gecko/i.test(navigator.userAgent);
var webkit = $.browser.safari && $.browser.version >= 3;




$.fn.roundbottom = function(options) {


      var $this=$(this);

           if(moz){
			  $this.addClass('roundcorner');
		   } else{
			   var pref='b';
			   if($this.hasClass('greenmenu'))
			       pref='g'
			   $(this).append('<table width="100%" cellpadding="0" cellspacing="0"><tr><td width="10" ><img src="images/'+pref+'_left_corner.gif" /></td><td class="'+$this.attr('class')+'"><img src="images/c.gif"/></td><td width="10" ><img src="images/'+pref+'_right_corner.gif" /></td ></tr></table>');

		   }

        return this;
}


$.fn.iframe = function() {
    return this.get(0).contentWindow
};


$.fn.butclick = function() {
    $(this).mousedown(function() {
        $(this).addClass('click');
    }).mouseup(function() {
        $(this).removeClass('click');
    }).mouseout(function() {
        $(this).removeClass('click');
    });
    return $(this);

};


$.fn.hoverCss = function(classname) {
    $(this).each(function() {
        $(this).mouseover(function() {
            $(this).addClass(classname);
        }).mouseout(function() {
            $(this).removeClass(classname);
        });
    });

    return $(this);
};

$.fn.iframemenu = function(opt) {
    opt = $.extend({
        timeout: 1000,
        knap: $(this).parent(),
        align:'right',
        delay:1,
        top:0,
        left:0,
        width:170,
        defaultShow:false

    }, opt || {});
    var timeout = 1000;
    var closetimer = null;
    var opendelay;
    var menu = $(this);
    var iframe = menu.find('iframe:eq(0)');
    iframe.width(opt.width);
    $(window).click(menu_close()).resize(function() {

        place_menu();

    });
    opt.knap.data('open', function() {
        place_menu();
        menu_open();
        menu_canceltimer();
    });
    $(this).bind('mouseover', function() {
        menu_canceltimer();
    }).bind('mouseout', function() {
        menu_timer();
    });
    iframe.click(function() {
        menu_close()
    });
    opt.knap.bind('mouseover', function() {
      opendelay=  window.setTimeout(menu_ini, opt.delay);

    }).bind('mouseout', function() {
		clearTimeout(opendelay);
        reset_timer();
    });
    function place_menu() {
        if(opt.knap.length==1){
        var lft = opt.knap.offset().left;
        if (opt.align == 'left')
            lft = lft - opt.width;

        var ltop = opt.knap.offset().top + opt.knap.height() + 3;

        menu.css('left', lft + 'px').css('top', ltop + 'px');
    } }

    function menu_open() {
        place_menu();
        if (menu.data('down') == null) {
            menu.slideDown('fast', function() {
                resizemenu(iframe.attr('id'));
            });
            menu.data('down', 1);

        }
        //ddmenuitem = $(this).find('ul').eq(0).css('visibility', 'visible');
    }
    function menu_ini(){
        menu_open();
        menu_canceltimer();
    }
    function menu_close() {
        menu.slideUp();
        menu.removeData('down');

    }

    function menu_timer() {
        closetimer = window.setTimeout(menu_close, timeout);
    }

    function reset_timer() {
        menu_canceltimer();
        closetimer = window.setTimeout(menu_close, timeout);
    }

    function menu_canceltimer() {
        if (closetimer) {
            window.clearTimeout(closetimer);
            closetimer = null;
        }
    }

    if (!opt.defaultShow)
        menu.hide();
    else {
        menu_open();
        menu_canceltimer();
    }
    menu.data('close',function(){

	    menu.hide();
        menu.removeData('down');
	});
    return $(this);

};


$.fn.dropdownmenu = function(opt) {
    opt = $.extend({
        timeout: 1000,
        knap: $(this).parent(),
        activElement:$(this).find('div.item'),
        alignShow:'left',
        top:0,
        left:0,

        defaultShow:false

    }, opt || {});
    var timeout = 1000;
    var closetimer = null;

    var menu = $(this);

    $(window).click(menu_close()).resize(function() {
        place_menu();
    });
    opt.knap.data('open', function() {
        place_menu();
        menu_open();
        menu_canceltimer();
    });
    if(isIEOld())
    $(this).bgIframe();
    $(this).bind('mouseover', function() {
        menu_canceltimer();
    }).bind('mouseout', function() {
        menu_timer();
    }).click(function() {
        menu_close()
    });
    opt.knap.bind('mouseover', function() {
        menu_open();
        menu_canceltimer();
    }).bind('mouseout', function() {
        reset_timer();
    });
    opt.activElement.mouseover(function() {
        menu_canceltimer();
        $(this).addClass('hover');
    })
            .mouseout(function() {
        $(this).removeClass('hover');
    });


    // menu.bind('mouseout',  menu_timer);
    function place_menu() {
        if (opt.knap.length == 1) {
            var lft = opt.knap.offset().left;
            if (opt.alignShow == 'right')
                lft = lft - menu.width();

            var ltop = opt.knap.offset().top + opt.knap.height() + 3;

            menu.css('left', lft + 'px').css('top', ltop + 'px');
        }
    }

    function menu_open() {
        place_menu();
        if (menu.data('down') == null) {
            menu.slideDown('fast', function() {
               // $(this).corner('bottom')
            });
            menu.data('down', 1);
        }
        //ddmenuitem = $(this).find('ul').eq(0).css('visibility', 'visible');
    }

    function menu_close() {
        menu.slideUp();
        menu.removeData('down');
        ;
    }

    function menu_timer() {
        closetimer = window.setTimeout(menu_close, timeout);
    }

    function reset_timer() {
        menu_canceltimer();
        closetimer = window.setTimeout(menu_close, timeout);
    }

    function menu_canceltimer() {
        if (closetimer) {
            window.clearTimeout(closetimer);
            closetimer = null;
        }
    }

    if (!opt.defaultShow)
        menu.hide();
    else {
        menu_open();
        menu_canceltimer();
    }

    return $(this);

};
$.fn.dialog_wrap=function(opt){
    var res= $(this).dialog(opt);
    var $dialog=$(this).parent();

    if (isIEOld()) {
        var optx = $.extend({
        width: $dialog.width(),
        height: $dialog.height(),
        resizable: false
        });
        $(this).dialog({
           open: function(event, ui) {
           if(!optx.resizable)
		         $dialog.find('iframe.bgiframe').height($dialog.height()+2).width($dialog.width()+2);
           }
        });

     $dialog.find('iframe.bgiframe').height(optx.height+2).width(optx.width+2);

    }
    return  res;

} ;

function isIEOld(){
  return  $.browser.msie && parseInt($.browser.version,10)<9;
}
