var $PARAMHELP = null;
var CurrentParameter = null;
var folderfunction = null;
var isloading = false;
var parameterhelp_loaded = false;
var SEARCHURL;
var $FOLDEREDIT;
var $REPORTMOVE;
var $currentfolder;

$(function() {
    folder_edit_ini();
    report_move_ini();
    send_to_user_ini();
 $('#param_help_but').click(function(){
	paramhelp();
 });

   // $(window).resize(function(){
	//	resizeframe('tree_frame');
//		resizeframe('folder_frame');
//	});
});
function child_dialog_function(){
    //ini from childframe
}
var searchpage;
function make_filterhelp(){

	$('body').append('<div class="content" id="helpframe"  style="height:470px;display:none" width="840"  height="510" ><iframe width="100%" height="100%" frameborder="0" src="filter_help.html" id="filter_help" scrolling="no"></iframe></div>');
	$('#helpframe').dialog_wrap({title:'Filter helper',width:865,height:520,resizable:false});

}
function loaded(name) {
    $('#folder_frame').show();
    var fid = $('#' + name).attr('fid');
}

function removerow(defid) {
    $('#folder_frame').content().find('#extract_list tr[defid=' + defid + ']').remove();
    loaded('folder_frame');
}
var open_folder_id;
var openfolderfunct = null;
function openfolder(id) {

    if (openfolderfunct == null)
        open_folder_id = id;
    else
        openfolderfunct(id);

}
function setOpenFolderFunc(func) {
    if (open_folder_id != null || open_folder_id == '') {
        func(open_folder_id);
    }
    openfolderfunct = func;
}
function loadframe(url) {
  // $('#folder_frame').contents().find('body').html('').addClass('bigrun');
    gui_makeerror('');
    gui_jdebug(url);
    return true;
}
function isSearch() {
    return   $('#search_frame').length > 0;
}
///*************frame functions
function tree_reread() {
    //child function
}
function tree_rename(id, name) {   //child function
}
function tree_level_hide(id, selfhide) {  //child function
}
function report_get_selected() {   //child function
}
function helploaded() {

	paramhelp();
}
function setParameterType(){}
function setfolderheader() {
}
///*************frame functions


function paramhelp() {
if($('#helpframe').length ==0)
	   make_filterhelp();
	else{
    	parameterhelp_ini();
    	$('#helpframe').dialog('open');
	}
}

function ini_parameters($buts) {
    $('body').append('<div class="content" id="helpframe" width="810" height="430"><img src="images/logorun.gif" id="bigrun" /></div>');

}
function parameterhelp_ini() {

    var input = CurrentParameter.parents('tr:eq(0)').find('input');
    setParameterType(CurrentParameter, input.val(),function(value){input.val(value); $('#helpframe').dialog('close');},true);

}

function folder_move($but) {

    var $folderobj = $but.data('folder');
    if ($folderobj != null) {
        $but = $(this);

        tree_level_hide();

        var ss = 'ajax?action=folder_move&id=' + folder_read_id($currentfolder) + '&folder=' + folder_read_id($folderobj);

        $FOLDEREDIT.close();
        ajaxXML(ss, $but, tree_reread, gui_makeerror, xmlOK);


    }
}

function folder_read_id($obj) {
   if ($obj) {
        return $obj.parents('li:eq(0)').attr('id').split('_')[1];

    }
    return -1;
}

//***********************folderactions******************************

function folder_edit_ini() {
   // $FOLDEREDIT = $('#folder_edit_but').menupop($('#folder_edit_dialog'), {caption:'Edit folders','width':360,'ypos':120,'xpos':200,
    //    onclose:function() {

           //folderfunction = null;
      //      tree_level_hide();
       // }});

	$('#folder_edit_but').click(function(){

		$('#folder_edit_dialog').dialog_wrap({width:500,title:'Edit folders',resizable:false,position:[120,200],close:function() {

            folderfunction = null;
            tree_level_hide();
        }});
		 });
    $('#tab_folder3').click(function() {
        tree_level_hide(folder_read_id($currentfolder), true);
        folder_move_reset();
    });
    $('#tab_folder1,#tab_folder2,#tab_folder4').click(function() {
        tree_level_hide();

    });
    $('#folder_move_but').click(function() {
        folder_move($(this));
        return false;
    });
    gui_tab();
    $('#folder_delete').click(function() {

        folder_delete($(this));

    });
    $('#folder_new_but').click(function() {
        folder_new();
    });
    $('#folder_rename_but').click(function() {
        folder_rename();
    });
}

function folder_edit_show($folder,isRoot) {
    if (!folderfunction) {
        $currentfolder = $folder;
        if ($folder != null) {
            var foldername = $folder.find('a:eq(0)').text();
            if (isRoot) {
                $('#tab_folder3,#tab_folder4').hide();
            } else {
                $('#tab_folder3,#tab_folder4').show();
            }

           // $FOLDEREDIT.find('td.cap:eq(0)').text('Edit folder: ' + foldername);
            $('#folder_rename').val(foldername);

            folderfunction = function($obj) {
                $('#destfolder').text($obj.text());
                $('#folder_move_but').data('folder', $obj);

            }
            folder_move_reset();
            $('#tab_folder1').trigger('click');
            $('#folder_edit_but').trigger('click');
			$('#folder_edit_dialog').dialog('option','title','Edit folder: ' + foldername);

        }
    }
}
function folder_move_reset() {
    $('#destfolder').text('!!no folder selected!!');
    $('#folder_move_but').removeData('folder');

}
function folder_new() {

    var foldername =$.trim(makeStringValid($('#folder_new').val()));

    if (foldername.length > 2) {
        gui_makeerror();
        var ss = 'ajax?action=folder_new&name=' + escape(foldername) + '&id=' + folder_read_id($currentfolder) +'&s='+ $currentfolder.parents('#sharedfolders').length;
        $('#folder_edit_dialog').dialog('close');
        ajaxXML(ss, null, tree_reread, gui_makeerror, xmlOK);
    }
    else {
        $('#folder_new').get(0).focus();

   }

}

function folder_rename() {
    var newfoldername = $.trim(makeStringValid($('#folder_rename').val()));
    if (newfoldername.length > 2) {
        var link = 'ajax?action=folder_rename&id=' + folder_read_id($currentfolder) + '&name=' + escape(newfoldername);
        $('#folder_edit_dialog').dialog('close');
        ajaxXML(link, null, folder_rename_done, gui_makeerror, xmlOK);
    } else {
        $('#folder_rename').get(0).focus()
    }

}

function folder_rename_done(xml) {
    $currentfolder.text($(xml).find('folder name').text());
    if (setfolderheader)
        setfolderheader($(xml).find('folder path').text());


}

function folder_delete($obj) {

    var ss = 'ajax?action=folder_del&id=' + folder_read_id($currentfolder);
    $('#folder_edit_dialog').dialog('close');
    ajaxXML(ss, $obj, function(){window.location.href='page?action=extract_center'; }, gui_makeerror, xmlOK);
}
//************************************************************************
function report_move_ini() {

    $('#report_move_but').click(function() {
        child_dialog_function($(this));
    });

		$('#report_move_dialog').show().dialog_wrap({title:'Move reports',autoOpen: false ,resizable:false,width:360,position:['right',120],close:function() {
    		tree_level_hide();
            tree_part_show();
        folderfunction = null;

    }});
}
function folder_dialog_show($but, caption, func) {
    if (!folderfunction) {

        child_dialog_function=func;
        $('#destfolder1').text('!!no folder selected!!');
        folderfunction = function($obj) {
            $('#destfolder1').text($obj.text());
            $('#report_move_but').data('folder', $obj);
        };
        if($but.attr('shared')==null){
            tree_part_show('myfolders');
        }else{
            tree_part_show('sharedfolders');
        }
        $('#report_move_dialog').dialog('option', 'position',[$but.position().left,120]);
        $('#report_move_dialog').dialog('option','title',caption);
        $('#report_move_dialog').dialog('open');

    }

}

//*********************
var $SENDDIALOG;

function show_send_dialog($but,e){
     var left=$but.position().left-40;

    $('#submit_send').data('but',$but);
  $('#send_dialog').dialog('option','position',[left,120]);
    $('#send_dialog').dialog('open');

}
function send_to_user_ini() {

    function user_input_valid() {
        var user_login = "";
        var inputs = $('#send_dialog input');
        user_login = inputs.eq(0).val();
        if (user_login.length < 2) {
            alert('User is not valid, please rewrite');
            inputs.get(0).focus();
            return  false;
        }
        return true;
    }



    function copy_success() {
        gui_makestatment( 'Copy to user - done.');
    }
    $('#submit_send').click(function() {
                if (user_input_valid()) {
                    $but=$(this).data('but');
                    $('#send_dialog').dialog('close');

                    var returnfunction= copy_success;

                    var ss = 'ajax?action=definition_send&user=' + $('#send_dialog input:eq(0)').val() + '&defid=' + $but.attr('defid');
                    ajaxXML(ss, $but, returnfunction, parent.gui_makeerror, xmlOK);
                }
            });
    $('#send_dialog').dialog_wrap({title:'Send to user',width:260,height:135,autoOpen: false ,resizable:false});
   // $SENDDIALOG = $('#send_dialog_but').menupop($('#send_dialog'), {caption:'Send to user', align:'floatright','ypos':120,'xpos':200,'width':250,height:160});
}

function closeAllDialogs(){
    if($('#send_dialog').length==1)
        $('#send_dialog').dialog('close');
    if($('#helpframe').length==1)
        $('#helpframe').dialog('close');
    if($('#folder_edit_dialog').length==1)
        $('#folder_edit_dialog').dialog('close');
    if($('#report_move_dialog').length==1)
        $('#report_move_dialog').dialog('close');
}
