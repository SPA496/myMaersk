// JavaScript Document
var $GROUP_MENU;
var $GLOBALSEARCH;
$(function() {

 if($('#dropdownsearch').length!=0){

 $GLOBALSEARCH=$('#g_search_menu').iframemenu({knap:$('#dropdownsearch')});


 $('#backend').iframemenu({knap:$('#backendmenu'), align :'left',width:180});


    $('#global_search').parent().click(function() {

        if ($('#global_search').val().length > 0) {
            location.href = $('#global_search').attr('url') + encodeURIComponent($('#global_search').val())  +'&x='+ new Date().valueOf();
        }
    });
   
    $('#global_search').keydown(function(e) {
        if (e.keyCode == 13) {
            $(this).parent().trigger('click');
        }
    });


	$('div.menuitem').hoverCss('hover');
  }
});

function searchvalue(text,url){
	if(url!=null){
      $('#dropdownsearch span').text(text);
      $('#global_search').attr('url', url).get(0).focus();

	  $GLOBALSEARCH.data('close')();
      return false;
}
}
