var G_Listdata;
var G_resizetime;
var showdebug = false;

var local = false;
var ServerLink = new Array('');
var FRAMENAME = 'folder_frame';


$(function() {
    // gui_makestatment('Initializing ', 'r', function() {
    $('body').html($('body').html());

    iniall();
    gui_download();
    parent.loaded(FRAMENAME);
    parent.openfolder(data_id);
    frameheight(FRAMENAME);


});

function iniall() {
    parent.report_get_selected = report_get_selected;
    parent.setfolderheader = setfolderheader;
    parent.reports_move=reports_move;
    $('#def_move').click(function() {

        parent.folder_dialog_show($(this),'Move reports',reports_move)

    });
	$('#copy_role').click(function() {

        parent.folder_dialog_show($(this),'Copy to shared folder',reports_share)

    });

    $('#definition_delete').click(function() {
        delete_selected()
    });

    $('#chk_all').click(function() {
        var box = this;
        var $inpbox = $(this).parents('table:eq(0)').find('input[type=checkbox]:gt(0)');
        $inpbox.each(function() {
            this.checked = box.checked;
        });
    });
    var $message = $('div.message');
    $message.each(
            function() {
                $(this).insertBefore($('div.content'));
                if (this.id != '')
                    gui_showmessage(this.id);
            });


    $('#extract_list').tablesorter().find('a[defid]').click(function() {
        parent.loadframe(this.href);

    });


}
 function delete_selected(){
     var ids=report_get_selected();

     if (ids.length == 0) {
             parent.gui_makeerror('No reports selected !');
             return;
         }

    if (ids.length>0) {
        var link = 'ajax?action=definition_delete&ids=' +ids.join(',') ;

        $('div.section').hide();
        $('body').addClass('bigrun');
        ajaxXML(link, null, location.reload, function(e){
            parent.gui_makeerror(e);
            if(e!=null)
            location.reload();
        },xmlOK);
    }

 }
function reports_share($but){
    var folder = $but.data('folder');
    if (folder) {
    var ids = report_get_selected();
    var defids = ids.join(',');
    if (ids.length == 0) {
        parent.gui_makeerror('No reports selected !');
        return;
    }
    var ss = 'ajax?action=copy_role&id=' + parent.folder_read_id(folder) + '&defids=' + defids;
        parent.closeAllDialogs();
        ajaxXML(ss, null, function(){
            parent.gui_makestatment('Report/s copied successfully to shared folder.');
        }, function(e){
             parent.gui_makeerror(e);
             if(e!=null)
               location.reload();
        }, xmlOK);
    }
}

function reports_move($but) {

    var folder = $but.data('folder');
    if (folder) {
    var ids = report_get_selected();
     var defids = ids.join(',');
    if (ids.length == 0) {
        parent.gui_makeerror('No reports selected !');
        return;
    }
   var ss = 'ajax?action=definition_move&id=' + parent.folder_read_id(folder) + '&defids=' + defids;
        parent.closeAllDialogs();
        ajaxXML(ss, null, function(){
            location.reload();
        }, function(e){
             parent.gui_makeerror(e);
             if(e!=null)
               location.reload();
        }, xmlOK);
    }

}
//*************************folder actions*********************************
function setfolderheader(path) {
    $('#folder_part td.sectionline:eq(0) span').html(path);
}


function report_get_selected() {
    var ids = new Array();
    $('#extract_list input:checked').each(function() {
        if ($(this).attr('defid'))
            ids[ids.length] = $(this).attr('defid');
    });
    return ids;
}




