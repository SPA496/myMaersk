<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:template match="defheader">
        <xsl:if test="message">
            <div class="message show">
                <xsl:value-of select="message"/>
            </div>
        </xsl:if>
        <table width="90%">
            <tr>
                <th width="100" class="r">Description</th>
                <td id="def_description" class="data">
                    <xsl:value-of select="//definition/description"/>
                </td>
                <xsl:if test="//definition/user">
                    <xsl:if test="//definition/shared">
                        <th width="100" class="r">Edit by</th>
                    </xsl:if>
                    <xsl:if test="not(//definition/shared)">
                        <th width="100" class="r">Owned</th>
                    </xsl:if>
                    <td width="60" class="data">
                        <xsl:value-of select="//definition/user"/>
                    </td>
                </xsl:if>
            </tr>
        </table>
    </xsl:template>
    <xsl:template match="extract">
        <tbody exid="{id}">
            <xsl:if test="refresh">
                <xsl:attribute name="refresh">1</xsl:attribute>
            </xsl:if>
            <tr>

                <td nowrap="nowrap" class="data">
                    <xsl:if test="stop">
                        <xsl:attribute name="align">left</xsl:attribute>
                        <a href="#" class="noicon" stop_exid="{id}">
                            <img src="images/c.gif" alt="" height="15" width="1" class="but delete"/>
                            Stop
                        </a>
                    </xsl:if>
                    <xsl:if test="status='ok'">
                        <xsl:attribute name="align">right</xsl:attribute>
                        <xsl:if test="agrows>0">
                            <xsl:attribute name="download">
                                <xsl:value-of select="id"/>
                            </xsl:attribute>
                            <a href="page?action=extract_view&amp;exid={id}" target="_top">
                                <xsl:value-of select="agrows"/>
                            </a>
                        </xsl:if>
                        <xsl:if test="agrows=0">0</xsl:if>

                    </xsl:if>
                </td>
                <td align="center" class="data">
                    <xsl:if test="snapid">
                        <a href="page?action=snapshot&amp;defid={//definition/@defid}&amp;snapid={snapid}"
                           target="_top">
                            <xsl:value-of select="starttime"/>
                        </a>
                    </xsl:if>
                </td>

                <td align="center" class="data">
                    <xsl:value-of select="exetime"/>
                </td>
                <td align="left" class=" data">
                    <table width="100%" cellpadding="0" cellspacing="0">
                        <tr>
                            <td width="20">
                                <img src="images/c.gif" width="15" height="15" hspace="1" class="x{status}"/>
                            </td>
                            <xsl:if test="parainfo">
                                <td width="20">
                                    <img src="images/parameter.gif" title="{parainfo}" width="15" height="15"
                                         hspace="1"/>
                                </td>
                            </xsl:if>
                            <xsl:if test="user">
                                <td width="20">
                                    <img src="images/usericon.gif" title="Run by {user}" width="15" height="15"
                                         hspace="1"/>
                                </td>
                            </xsl:if>
                            <xsl:if test="rungroup">
                                <td width="20">
                                    <img src="images/groupicon.gif" width="15" height="15" hspace="1"
                                         title="Run in role: {rungroup}"/>
                                </td>
                            </xsl:if>
                            <td style="padding-left:4px">
                                <xsl:value-of select="runinfo"/>
                            </td>
                        </tr>
                    </table>
                </td>
                <td align="center">
                    <input type="checkbox" class="checkbox">
                        <xsl:if test="user">
                            <xsl:attribute name="disabled">disabled</xsl:attribute>
                        </xsl:if>
                    </input>
                </td>
            </tr>
        </tbody>
    </xsl:template>
    <xsl:template match="extract_gui">
        <div class="message" id="extract_message"/>
        <div class="bigrun">
            <iframe src="frame?action=extract_list&amp;defid={//pagedata/definition/id}" class="fullheight"
                    name="extract_frame"
                    width="100%"
                    frameborder="0"
                    id="extract_frame" load_id="reload" scrolling="no"/>
        </div>
    </xsl:template>
    <xsl:template match="schedule_frame">
        <div class="message" id="schedule_message"/>
        <xsl:call-template name="schedule_add"/>
        <div class="bigrun">
            <iframe src="frame?action=schedule_list&amp;defid={//pagedata/definition/id}" name="schedule_frame"
                    width="100%"
                    frameborder="0"
                    id="schedule_frame" load_id="reload" class="fullheight" scrolling="no"/>
        </div>
    </xsl:template>
    <xsl:template match="extractlist_gui">
        <xsl:if test="//definition/extract">


            <table width="100%" class="tablesorter" id="ex_table" style="table-layout:fixed;margin-right:16px">
                <thead>
                    <tr align="left">
                        <th width="110" class="r">View rows</th>
                        <th width="160" class="c">Last Run (snapshot)</th>
                        <th width="50" class="c">Exe.time</th>
                        <th id="cap_info">Info</th>
                        <th width="20" align="center" valign="middle">
                            <input name="checkbox" type="checkbox" class="checkbox" id="sel_all"/>
                        </th>
                    </tr>
                </thead>
                <xsl:apply-templates select="//definition/extract"/>
            </table>

        </xsl:if>


        <script type="text/javascript">
            $(function(){

            <xsl:if test="//definition/extract">
                NORECORD='';
            </xsl:if>
            parent.gui_showmessage('extract_message',NORECORD);
            });
        </script>


    </xsl:template>

    <xsl:template match="parameter_gui">
        <xsl:if test="not(//definition/parameters/para)">
            <div class="message show">There is no parameter in this report definition.
                <br/>
                Parameter is defined in the report builder.

            </div>
        </xsl:if>


        <xsl:if test="//definition/parameters/para">
            <div class="message" id="parameter_message"/>
            <br/>
            <div class="cap">Fill parameter values</div>
            <br/>

            <table border="0" cellspacing="0" id="parameters">

                <xsl:for-each select="//definition/parameters/para">
                    <tr>
                        <th>
                            <xsl:value-of select="name"/>
                        </th>
                        <td>
                            <input name="p_{@id}" unit="{pfieldunit}" type="text" class="text" value="{value}"
                                   maxlength="1000"/>
                        </td>

                        <td>
                            <input name="help" class="help but" type="image" src="images/c.gif" param_id="{@id}"
                                   fieldid="{pfieldid}" fieldtype="{pacttype}" format="{pformat}"
                                   operator="{poperator}"/>
                        </td>
                    </tr>
                </xsl:for-each>
            </table>

            <div class="submitbar">
                <input id="run_report" type="submit" class="submit" value="Run"/>
                <span class="capsmall">with current parameter values</span>
            </div>
        </xsl:if>


    </xsl:template>
    <xsl:template match="tb">
        <table>
            <xsl:apply-templates select="*"/>
        </table>
    </xsl:template>
    <xsl:template match="schedule_mail_gui">
        <div id="mailsetup"  width="400" style="display:none; z-index: 12000;">
                    <div id="mail_box">

                        <div id="mail_tab1">
                            <table width="400" border="0" cellpadding="0" cellspacing="4" id="sch_mail2">
                                <tr>
                                    <td valign="top">
                                        <input name="xxx_radio" type="radio" class="selbut" value="0" id="ra_0" checked="checked"/>
                                    </td>
                                    <td valign="top">No e-mail notification</td>

                                </tr>
                                <tr>
                                    <td valign="top"><br/></td>

                                </tr>
                                <tr>
                                    <td width="24" valign="top">
                                        <input name="xxx_radio" type="radio" class="selbut" id="ra_1" value="1"/>
                                    </td>
                                    <td valign="top">Send e-mail notification (Number of rows and link to report)</td>

                                </tr>
                                <xsl:if test="@all">
                                <tr>
                                    <td valign="top">
                                        <input name="xxx_radio" type="radio" class="selbut" id="ra_2" value="2"/>
                                    </td>
                                    <td valign="top">E-mail with result as
                                        <strong>Excel</strong>
                                        file (zipped)
                                    </td>

                                </tr>
                                <tr>
                                    <td valign="top">
                                        <input name="xxx_radio" type="radio" class="selbut" id="ra_3" value="3"/>
                                    </td>
                                    <td nowrap="nowrap" valign="top">E-mail with result as
                                        <strong>CSV</strong>
                                        file (zipped)
                                    </td>

                                </tr>
                                </xsl:if>
                            </table>
                            <br/>
                            <br/>

                            <div class="submitbar"> <table> <tr><td>
                                <input type="submit" class="submit" value="Save" id="sch_mail_save"/></td> <td><span class="message" id="m_user">No user assigned</span></td></tr></table>
                            </div>

                        </div>

                    </div>
                </div>
    </xsl:template>
    <xsl:template name="schedule_add">

        <div class="section" id="schedule_box" name="schedule_box" width="400">
            <div>
                <div id="headmessage" class="capsmall"></div>
                <table border="0" cellpadding="0" cellspacing="4" id="sch_new">
                    <tr>
                        <td colspan="2"></td>

                        <td align="right" class="label">Current time in UTC is
                            <xsl:value-of select="."/>
                        </td>
                    </tr>
                    <tr class="data">
                        <td width="24" valign="top">
                            <input name="sch_radio" type="radio" class="selbut" value="1" checked="checked"/>
                        </td>
                        <td nowrap="nowrap" valign="top">Every day</td>
                        <td valign="top">
                            <span id="sch_selecttime">
                                at
                                <select name="hour" size="1" class="input-select" id="sch_hour">
                                    <option value="00">00</option>
                                    <option value="01">01</option>
                                    <option value="02">02</option>
                                    <option value="03">03</option>
                                    <option value="04">04</option>
                                    <option value="05">05</option>
                                    <option value="06">06</option>
                                    <option value="07">07</option>
                                    <option value="08">08</option>
                                    <option value="09">09</option>
                                    <option value="10">10</option>
                                    <option value="11">11</option>
                                    <option value="12">12</option>
                                    <option value="13">13</option>
                                    <option value="14">14</option>
                                    <option value="15">15</option>
                                    <option value="16">16</option>
                                    <option value="17">17</option>
                                    <option value="18">18</option>
                                    <option value="19">19</option>
                                    <option value="20">20</option>
                                    <option value="21">21</option>
                                    <option value="22">22</option>
                                    <option value="23">23</option>
                                </select>
                                :
                                <select name="minut" size="1" class="input-select" id="sch_minut">
                                    <option value="00">00</option>
                                    <option value="10">10</option>
                                    <option value="20">20</option>
                                    <option value="30">30</option>
                                    <option value="40">40</option>
                                    <option value="50">50</option>
                                </select>
                                [UTC time]
                            </span>
                        </td>
                    </tr>
                    <tr>
                        <td valign="top">
                            <input name="sch_radio" type="radio" class="selbut" value="2"/>
                        </td>
                        <td valign="top">
                            Every
                            <select class="input-select" name="weekday" size="1" id="sch_day">
                                <option value="01" selected="selected">Monday</option>
                                <option value="02">Tuesday</option>
                                <option value="03">Wednesday</option>
                                <option value="04">Thursday</option>
                                <option value="05">Friday</option>
                                <option value="06">Saturday</option>
                                <option value="07">Sunday</option>
                            </select>
                        </td>
                        <td valign="top" nowrap="nowrap"/>
                    </tr>
                    <tr>
                        <td valign="top">
                            <input name="sch_radio" type="radio" class="selbut" value="3"/>
                        </td>
                        <td valign="top">Last day in the month</td>
                        <td valign="top" nowrap="nowrap"/>
                    </tr>
                    <tr>
                        <td valign="top">
                            <input name="sch_radio" type="radio" class="selbut" value="4"/>
                        </td>
                        <td nowrap="nowrap" valign="top">
                            The
                            <input type="text" class="text" id="sch_month" value="1" size="2" maxlength="2"/>
                            . day in the month
                        </td>
                        <td valign="top" nowrap="nowrap"/>
                    </tr>
                </table>
                <table>
                    <tr>
                        <td width="75">
                            <div class="submitbar">
                                <input type="submit" class="submit" value="Save" id="sch_save"
                                       defid="{//definition/id}"/>
                                <br/>
                            </div>
                        </td>
                        <td>
                            <span id="redmsg" class="message">Longer queue time expected while selected time slice has
                                many reports scheduled
                            </span>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </xsl:template>


    <xsl:template match="schedule_gui">
        <div id="sch_list_hold">
            <xsl:apply-templates select="//schedulelist"/>
        </div>
        <script type="text/javascript">
            $(function(){
            var ss='';
            <xsl:if test="not(//schedulelist/schedule)">
                ss='This report has no schedule assigned';
            </xsl:if>
            parent.gui_showmessage('schedule_message',ss);
            });
        </script>
    </xsl:template>
    <xsl:template match="schedulelist">
        <xsl:variable name="showmail"><xsl:value-of select="@mail"/> </xsl:variable>
        <xsl:if test="(schedule)">
            <table border="0" cellspacing="1" cellpadding="2" id="sch_list">
                <xsl:if test="@expire=1">
                    <xsl:attribute name="expire">1</xsl:attribute>
                </xsl:if>
                <thead>
                    <tr>
                        <th width="60">Status</th>
                        <th>Schedule</th>
                        <th width="100">Next run</th>
                        <th width="100">Expire</th>
                        <xsl:if test="$showmail='1'">
                        <th width="50">Notification</th>
                        </xsl:if>
                        <th width="40"/>
                    </tr>
                </thead>
                <xsl:for-each select="schedule">
                    <tbody>
                        <xsl:attribute name="code">
                            <xsl:value-of select="."/>
                        </xsl:attribute>
                        <tr>
                            <td class="data">
                                <xsl:value-of select="@status"/>
                            </td>
                            <td class="data"/>
                            <td class="data">
                                <xsl:value-of select="@nextrun"/>
                            </td>
                            <td class="data">
                                <xsl:value-of select="@expire"/>
                            </td>
                            <xsl:if test="$showmail='1'">
                            <td class="data">
                                <a href="#" class="mail" m_id="{@m_id}" m_type="{@m_type}" sch_id="{@id}">
                                    <xsl:choose>
                                        <xsl:when test="@m_type=1">Mail</xsl:when>
                                        <xsl:when test="@m_type=2">Excel</xsl:when>
                                        <xsl:when test="@m_type=3">CSV</xsl:when>
                                        <xsl:otherwise>None</xsl:otherwise>
                                    </xsl:choose>

                                </a>
                                <select style="display:none">
                                <xsl:for-each select="assigned/user">
                                  <option value="{@login}"><xsl:value-of select="."/> [<xsl:value-of select="@login"/>] </option>
                                </xsl:for-each>

                                </select>

                            </td>
                            </xsl:if>
                            <td align="center" class="data">
                                <input type="checkbox" class="checkbox" sch_id="{@id}"/>
                            </td>
                        </tr>
                    </tbody>
                </xsl:for-each>
            </table>
            <xsl:if test="//schedule/@exdyn">
                <div class="message show">Schedule are expired while definition do not contain dynamic filters</div>
            </xsl:if>
        </xsl:if>
    </xsl:template>
    <xsl:template match="userinfo"/>
    <xsl:template match="pageinfo"/>
</xsl:stylesheet>
