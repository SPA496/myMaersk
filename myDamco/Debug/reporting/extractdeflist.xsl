<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	    <xsl:output method="html" doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"/>
	<xsl:template match="folder_gui">
		
					<xsl:apply-templates select="//listdef"/>
			
	</xsl:template>
	<xsl:template  match="listdef">
		<xsl:if test="def">
			
				<table width="100%" border="0" cellspacing="1" cellpadding="2" class="tablesorter" id="extract_list" folderid="{id}" style="tabel-layout:fixed">
					<thead>
						<tr>
							<xsl:if test="@type='bin' or @type='folder'">
								<td width="20"><input type="checkbox" id="chk_all"/></td>
							</xsl:if>

							<th align="left">Definition</th>
							<xsl:if test="not(@type='bin')">
							<th width="115" class="r">View result</th>
							<th width="125" align="left">Last Run</th>
							<th width="60">Info</th></xsl:if>
						</tr>
					</thead>
					<tbody>
						<xsl:variable name="datatype">
							<xsl:value-of select="@type"/>
						</xsl:variable>
						<xsl:for-each select="def">
                            <xsl:variable name="title">
                                <xsl:value-of select="description"/><xsl:if test="path">  folder path: <xsl:value-of select="path"/></xsl:if>
                            </xsl:variable>
							<tr defid="{id}">

								<xsl:if test="$datatype='folder'  or $datatype='bin' ">
									<td align="left" valign="top">
										<input type="checkbox" defid="{id}"/>
										
									</td>
								</xsl:if>
								<xsl:if test="$datatype='bin'">
									<td align="left" valign="top" title="{$title}" class="data">
										<xsl:value-of select="name"/>
									</td>
								</xsl:if>
								<xsl:if test="not($datatype='bin')">
									<td align="left" valign="top" class="data" title="View definition details"  nowrap="nowrap" style="overflow:hidden">
										<a href="frame?action=definition_extract&amp;defid={id}" defid="{id}" title="{$title}"  target="folder_frame" >
											<xsl:value-of select="name"/>
										</a>
									</td>
								
								<td valign="top" nowrap="nowrap" class="data" align="right">
									<xsl:if test="extract/status='ok'">
                                        <xsl:if test="extract/agrows>0">
                                        <xsl:attribute name="download"><xsl:value-of select="extract/id"/> </xsl:attribute>
										<a href="page?action=extract_view&amp;exid={extract/id}"  target="_top" title="View extracted data">
										    <xsl:value-of select="extract/agrows"/>
										</a>
                                        </xsl:if>
                                        <xsl:if test="extract/agrows=0">0
                                        </xsl:if>
									</xsl:if>
								</td>
								
								<td valign="top" nowrap="nowrap" class="data">
									<xsl:value-of select="extract/starttime"/>
								</td>
								<td align="left" valign="top" class="data" nowrap="nowrap">
								<xsl:if test="extract">
								<img class="status x{extract/status}" src="images/c.gif" title="{extract/runinfo}" width="15" height="15"/>
								</xsl:if>
								<xsl:if test="extract/parainfo">
                                <img src="images/parameter.gif" title="{extract/parainfo}" width="15" height="15" hspace="1" />
                                </xsl:if>
                                <xsl:if test="shared">
                                <img src="images/sharedicon.gif" title="Shared definition"  width="15" height="15" hspace="1" />
                                </xsl:if>
                                <xsl:if test="extract/user and not(shared)">
                                <img src="images/usericon.gif" title="Run by {user}"  width="15" height="15" hspace="1" />
                                </xsl:if>
					            <xsl:if test="extract/rungroup">
                                <img src="images/groupicon.gif" width="15" height="15" hspace="1" title="Run in role: {extract/rungroup}" />
                                </xsl:if>
								</td></xsl:if>
							</tr>
						</xsl:for-each>
					</tbody>
				</table>
			
		</xsl:if>
		<xsl:if test="not(def)">
			<xsl:if test="@type='bin'">
				<div class="message" height="24" id="empty_bin">No deleted reports are found</div>
			</xsl:if>
			<xsl:if test="@type='folder'">
				<div class="message" height="24" id="empty_folder">No reports are found in this folder </div>
			</xsl:if>
			<xsl:if test="@type='recent'">
				<div class="message" height="24" id="empty_recent">No reports have been executed recently</div>
			</xsl:if>
			<xsl:if test="@type='search'" >
				<div class="message" height="24" id="empty_search">Search did not find any reports</div>
			</xsl:if>
		</xsl:if>
		 <div id="foldermove" style="display:none" >
                <br/>
                <table class="fields">
                  <tr>
                    <td class="caption">Define destination folder by clicking the folder in the folder tree.</td>
                    
                  </tr>
                </table>
               
            </div>
		  <div id="foldername" style="display:none" >
                <br/>
                <table class="fields">
                  <tr>
                    <td class="caption">Folder name</td>
                    <td align="" class="data" nowrap="nowrap"><input type="text" value="" cap="Folder name" class="text"  /></td>
                  </tr>
                </table>
                <div style="clear:both:height:1px"> </div ><br/>
                             <div class="submitbar">
                  <input type="submit" class="submit" value="Submit"  defid="{//definition/id}" />
                </div>
            
            </div>
	</xsl:template>
	<xsl:template match="userinfo"/>
</xsl:stylesheet>
