<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="html" doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"/>
    <xsl:include href="damco.xsl"/>
    <xsl:include href="extractdef.xsl"/>
    <xsl:include href="extractdeflist.xsl"/>

    <xsl:template match="center_gui">
        <table class="columns" cellspacing="0" cellpadding="0">
            <tbody>
                <tr>
                    <td valign="top" width="280" class="bigrun">
                        <iframe id="tree_frame" width="98%" frameborder="0"
                                src="framefolderlist.xml"
                                scrolling="no" class="fullheight"/>
                    </td>
                    <td valign="top" class="bigrun">
                        <iframe src="{//pagedata/frame1/url}" width="100%" frameborder="0"
                                id="folder_frame" name="folder_frame" load_id="{//pagedata/frame1/@id}" scrolling="no"
                                style="display:none" class="fullheight"/>
                    </td>
                </tr>
            </tbody>
        </table>
        <img src="images/c.gif" id="folder_edit_but"/>
        <img src="images/c.gif" id="param_help_but"/>
        <img src="images/c.gif" id="report_dialog_but"/>
        <img src="images/c.gif" id="send_dialog_but"/>
         <div id="send_dialog" style="display:none">
            <br/>
            <table class="fields">
                <tr>
                    <td class="caption">User login name</td>
                    <td align=""  nowrap="nowrap">
                        <input type="text" value="" cap="User login name" class="text" id="username"/>
                    </td>
                </tr>

            </table>
            <br/>
            <div class="submitbar">
                <input type="submit" class="submit" id="submit_send" value="Send" defid="{//definition/id}"
                       folderid="{//definition/folder}"/>
            </div>

        </div>
        <div id="report_move_dialog" style="width:350px;display:none">
            <div>
                <div style="height:70px">
                    <div class="message show" style="margin:0px;width:300px">Click on destination folder</div>
                    <br/>
                    <table>
                        <tr>
                            <th>Destination folder</th>
                            <td class="data" id="destfolder1">!!no folder selected!!</td>
                        </tr>
                    </table>
                </div>
                <div class="submitbar">
                    <input type="submit" class="submit" value="OK" id="report_move_but"/>
                </div>
            </div>
        </div>
        <div id="folder_edit_dialog" style="display:none">
            <div>
                <div class="tabbar">
                    <ul>
                        <li id="tab_folder1" tab="#folder1">
                            <span>New</span>
                        </li>
                        <li id="tab_folder2" tab="#folder2">
                            <span>Rename</span>
                        </li>
                        <li id="tab_folder3" tab="#folder3">
                            <span>Move</span>
                        </li>
                        <li id="tab_folder4" tab="#folder4">
                            <span>Delete</span>
                        </li>
                    </ul>
                </div>
                <div style="clear:both;height:1px">
                </div>
            </div>
            <div class="secbody">
                <div id="folder1" style="display:none">
                    <div style="height:65px">
                        <div class="text">
                            <br/>
                            <table>
                                <tr>
                                    <th>Create sub-folder with name</th>
                                </tr>
                                <tr>
                                    <td>
                                        <input type="text" class="text mandatory" ln="2" value="" id="folder_new"/>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </div>
                    <div class="submitbar">
                        <input type="submit" class="submit" value="OK" id="folder_new_but"/>
                    </div>
                </div>
                <div id="folder2" style="display:none">
                    <div style="height:65px">
                        <div class="text">
                            <br/>
                            <table>
                                <tr>
                                    <th>Rename folder to</th>
                                </tr>
                                <tr>
                                    <td>
                                        <input type="text" class="text mandatory" ln="2" value="" id="folder_rename"/>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </div>
                    <div class="submitbar">
                        <input type="submit" class="submit" value="OK" id="folder_rename_but"/>
                    </div>
                </div>
                <div id="folder3" style="display:none">
                    <div style="height:65px">

                        <div class="message show" >Click on destination folder</div>
                        <br/>
                        <table>
                            <tr>
                                <th>Destination folder</th>
                                <td class="data" id="destfolder">!!no folder selected!!</td>
                            </tr>
                        </table>
                    </div>

                    <div class="submitbar">
                        <input type="submit" class="submit" value="OK" id="folder_move_but"/>
                    </div>
                </div>
                <div id="folder4" style="display:none">
                    <div style="height:65px">
                        <div class="message show" >Are you sure you want to delete folder? <br/> Sub-folders and reports in the folder will also be deleted.

                            
                        </div>
                    </div>
                    <div class="submitbar">
                        <input type="submit" class="submit" value="OK" id="folder_delete"/>
                    </div>
                </div>
            </div>
        </div>
    </xsl:template>
    <xsl:template match="treelist_gui">
        <ul id="tree" class="tree">
            <li id="recentfolder">
                <input type="image" src="images/c.gif" class="but plus int"/>
                <span class="capsmall">Recent</span>
                <ul>
                    <li class="link" id="recentrun">
                        <span>
                            <a href="frame?action=recent" target="folder_frame" folderid="recent">Report results</a>
                        </span>
                    </li>
                    <xsl:if test="search">
                        <li class="link" id="recentsearch" style="display:none">
                            <span>
                                <a href="frame?action=definition_search&amp;name={search}&amp;x={x}" folderid="search"
                                   target="folder_frame">Search
                                </a>
                            </span>
                        </li>
                    </xsl:if>

                </ul>
            </li>
            <li id="myfolders">
                <input type="image" src="images/c.gif" class="but plus int"/>
                <span class="capsmall">My report folders</span>
                <ul>

                    <xsl:apply-templates select="//my/folder"/>
                </ul>
            </li>
            <li id="sharedfolders">
                <input type="image" src="images/c.gif" class="but plus int"/>
                <span class="capsmall">Shared report folders</span>
                <ul><xsl:apply-templates select="//share/folder"/> </ul>
            </li>
            <li>
                <input type="image" src="images/c.gif" class="but plus int"/>
                <span class="capsmall">Deleted reports</span>
                <ul>
                    <li class="link" id="mydeleted">
                        <span>
                            <a href="frame?action=definition_bin&amp;my=1" target="folder_frame" class="noicon"
                               folderid="del1">My reports
                            </a>
                        </span>
                    </li>
                    <li class="link" id="roledeleted">
                        <span>
                            <a href="frame?action=definition_bin&amp;my=0" target="folder_frame" class="noicon"
                               folderid="del1">Shared reports
                            </a>
                        </span>
                    </li>
                </ul>
            </li>
        </ul>

    </xsl:template>


    <xsl:template match="folder">
        <li id="fo_{@id}" class="link"  root="{@root}">
            <xsl:if test="folder">
                <input type="image" src="images/c.gif" class="but plus int"/>
            </xsl:if>
            <xsl:if test="not(folder)">
                <img alt="" src="images/c.gif" class="treeline"/>
            </xsl:if>
            <span>
                <a href="frame?action=folder_content&amp;id={@id}" target="folder_frame" folderid="{@id}" root="{@root}">
                    <xsl:value-of select="name"/>
                </a>
            </span>
            <xsl:if test="folder">
                <ul>
                    <xsl:apply-templates select="folder"/>
                </ul>
            </xsl:if>
        </li>
    </xsl:template>
</xsl:stylesheet>
