<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="html" doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"/>
    <xsl:include href="portalheader.xsl"/>
    <xsl:template match="data">
        <xsl:if test="//ajax">
            <xsl:apply-templates select="//ajax/*"/>
        </xsl:if>
        <xsl:if test="not(//ajax)">
            <xsl:apply-templates select="//wp"/>
        </xsl:if>
        <xsl:if test="//sessionout">
            <html>
                <script type="text/javascript">
                    if(parent!=this)
                    parent.location.reload();
                    else
                    alert('Session timed out');
                </script>
            </html>
        </xsl:if>
        <xsl:if test="//jerror">
            <html>
                <script type="text/javascript">
                    alert('<xsl:value-of select="//jerror"/> ');
                history.back();

                            </script>
              </html>

        </xsl:if>
    </xsl:template>
    <xsl:template match="wp">
        <html>
            <head>
                <xsl:if test="xua">
                     <meta http-equiv="X-UA-Compatible" content="IE=EmulateIE7; IE=EmulateIE9"/>
                </xsl:if>
                <title>myDamco</title>
                <link href="css/damco.css" rel="stylesheet" type="text/css" media="screen"/>
                <link href="css/picdef.css" rel="stylesheet" type="text/css"  media="screen"/>
                <link href="css/jquery-ui.css" rel="stylesheet"  media="screen" type="text/css"/>
                <link href="css/print.css" rel="stylesheet" type="text/css"  media="print" />

                <link rel="SHORTCUT ICON" href="images/favicon.ico"/>
                <xsl:for-each select="//style">
                    <link href="{.}" rel="stylesheet" type="text/css"/>
                </xsl:for-each>
                <xsl:call-template name="includejava"/>
                <xsl:if test="not(popup)">
                <xsl:call-template name="portalheader" />
                </xsl:if>
            </head>
            <body>
                <xsl:apply-templates select="@*"/>
                <xsl:if test="not(frame)">
                      <xsl:call-template name="newpagetop"/>
                      <xsl:apply-templates select="page|popup"/>
                </xsl:if>
                <xsl:if test="frame">
                    <xsl:if test="//error">
                        <script type="text/javascript">
                            $(function(){parent.gui_makeerror('<xsl:value-of select="//error"/>')});
                        </script>
                    </xsl:if>
                    <xsl:apply-templates select="frame/*"/>
                </xsl:if>
                <xsl:if test="error">
                    <xsl:call-template name="top-error"/>
                </xsl:if>
                <xsl:if test="//code_version">
                    <div style="position:absolute; top:3px; left:3px; background-color:#FFFF00">version : <xsl:value-of select="//code_version"/> </div>
                </xsl:if>
            </body>
        </html>
    </xsl:template>
    <xsl:template name="includejava">
        <script src="js/jquery/jquery-1.7.2.min.js" type="text/javascript"/>
        <script src="js/jquery/jquery-ui-1.8.21.custom.min.js" type="text/javascript"/>
        <script src="js/jquery/jquery.scrollTo-min.js" type="text/javascript"/>
        <script src="js/jquery/jquery.tablesorter.min.js" type="text/javascript"/>
        <script src="js/jquery/jquery.corner.js" type="text/javascript"/>
        <script src="js/jquery/jquery.bgiframe.min.js" type="text/javascript"/>
        <script src="js/jquery/myjquery.js" type="text/javascript"/>
        <script src="js/standard.js" type="text/javascript"/>
        <script src="js/pageheader.js" type="text/javascript"/>
        <xsl:if test="//jscript">
            <script src="js/track.js" type="text/javascript"/>
        </xsl:if>
        <xsl:for-each select="//script">
            <script src="{.}" type="text/javascript"/>
        </xsl:for-each>
        <xsl:if test="//javatext">
            <script type="text/javascript">
                <xsl:for-each select="//javatext">
                    <xsl:apply-templates select="@*"/>
                    <xsl:value-of select="."/>
                </xsl:for-each>
            </script>
        </xsl:if>
    </xsl:template>
    <xsl:template match="dropmenu">
        <div id="{@id}" class="{@class}" style="width:100%">
            <xsl:for-each select="item">
                <div class="menuitem" url="{@url}">
                    <xsl:value-of select="."/>
                </div>
            </xsl:for-each>
        </div>
    </xsl:template>
   <xsl:template match="menusearch">

        <td>
        <xsl:if test="@showmenu">
            <a id="dropdownsearch" class="dropmenu"> <span><xsl:value-of select="."/></span></a>
            <div id="g_search_menu" class="menublock" style="overflow:visible">
                    <iframe style="z-index: -1; display: block;  position: absolute; width: 170px;" tabIndex="-1"
                            src="{@showmenu}" frameBorder="0" scrolling="no" id="frame_searchmenu"/>
                </div>
            </xsl:if>
             <xsl:if test="not(@showmenu)">
              <a id="dropdownsearch" class="noicon"><span ><xsl:value-of select="."/></span></a>
             </xsl:if>
        </td>
        <td nowrap="nowrap" class="searchmenu">
            <input type="text" class="text" size="12" id="global_search" url="{@url}"/>
        </td>

    </xsl:template>
    <xsl:template match="icons">

        <xsl:apply-templates select="."/>

    </xsl:template>
    <xsl:template match="topmenu">
        <span class="topmenu">
            <a href="{@href}" class="{@class}" id="{@id}">
                <xsl:value-of select="."/>
            </a>
            <xsl:if test="@dropmenu">
                <div id="{@dropmenu}" class="menublock" style="overflow:visible">
                    <iframe style="z-index: -1; display: block;  position: absolute; width: 170px;" tabIndex="-1"
                            src="{@menu}" frameBorder="0" scrolling="no" id="frame_{@dropmenu}"/>
                </div>
            </xsl:if>
        </span>
    </xsl:template>
    <xsl:template match="icon">
        <img alt="{@title}" src="images/c.gif" type="image" class="{@class} but" id="{@id}">
            <xsl:if test="@href">
                <xsl:attribute name="onclick">location.href='<xsl:value-of select="@href"/>'
                </xsl:attribute>
            </xsl:if>
        </img>
        <xsl:if test="@dropmenu">
            <div id="{@dropmenu}" class="menublock" style="overflow:visible">
                <iframe style="z-index: -1; display: block;  position: absolute; width: 170px;" tabIndex="-1"
                        src="{@menu}" frameBorder="0" scrolling="no" id="frame_{@dropmenu}"/>
            </div>
        </xsl:if>
        <xsl:apply-templates select="*"/>
    </xsl:template>
    <xsl:template match="time">
        <div class="loadtime" title="Server UTC time">
            <xsl:value-of select="."/>
        </div>
    </xsl:template>
    <xsl:template match="dragmenu">
        <a class="empty">
            <xsl:apply-templates select="@*"/>
            <img alt="drag" src="images/c.gif" class="dragdrop drag but"/>
            <xsl:value-of select="."/>
        </a>
    </xsl:template>
    <xsl:template match="menutext">
        <span>
            <xsl:apply-templates select="@*"/>
            <xsl:value-of select="."/>
        </span>
    </xsl:template>
    <xsl:template match="menu">
        <a>
            <xsl:apply-templates select="@*"/>
            <xsl:if test="not(@href)">
                <xsl:attribute name="href">#</xsl:attribute>
            </xsl:if>
            <xsl:if test="not(@href or @id)">
                <xsl:attribute name="style">display:none</xsl:attribute>
            </xsl:if>
            <xsl:value-of select="."/>
        </a>
    </xsl:template>
    <xsl:template name="pagemenu">
        <xsl:if test="menu">
            <div class="pagebar" style="width:50%">
                <div class="line">
                    <table border="0" cellspacing="0" cellpadding="0">
                        <tr>
                            <td class="mainmenu">
                                <xsl:apply-templates select="menu"/>
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
        </xsl:if>
    </xsl:template>
    <xsl:template name="pageheader">
        <table width="100%" border="0" cellspacing="0" cellpadding="0">
            <tbody>
                <tr>
                    <td class="page-header"/>
                    <td id="myDamcoHeadline" class="{//application/portalclass} pageline ">
                        <img alt="" src="images/c.gif"/>
                        <xsl:value-of select="title"/>
                    </td>
                    <xsl:if test="//page and not(//nomenu)">
                        <td width="800" align="right" class="fade"> <img src="images/damco_small.gif" width="84" height="24" id="printlogo"/>

                            <table border="0" cellspacing="0" cellpadding="0" class="mainmenu">
                                <tr>
                                    <td>
                                        <xsl:apply-templates select="//application/menu"/>
                                    </td>
                                    <xsl:apply-templates select="//application/menusearch"/>
                                </tr>
                            </table>

                        </td>
                        <td align="right" class="iconbar">

                            <xsl:apply-templates select="//application/icons/icon"/>

                        </td>
                    </xsl:if>
                </tr>
            </tbody>
        </table>
    </xsl:template>
    <xsl:template match="content">
        <div class="content">
            <xsl:apply-templates select="*"/>
        </div>
    </xsl:template>
    <xsl:template name="pageactionbar">
        <xsl:if test="bar">
            <div class="bar">
                <div class="barline" style="text-align:left">
                    <xsl:apply-templates select="bar/*"/>
                </div>
            </div>
        </xsl:if>
    </xsl:template>
    <xsl:template match="page|popup">
        <div class="{name(.)}">
            <xsl:call-template name="pageheader"/>
            <xsl:if test="name(.)='page'">
                <xsl:call-template name="pagemenu"/>
                <xsl:call-template name="pageactionbar"/>
            </xsl:if>
            <xsl:call-template name="top-error"/>
            <xsl:apply-templates select="form|sec|space|columns|grid|ns|iframe"/>
        </div>
        <xsl:if test="//jdebug">
            <textarea cols="120" rows="20" id="javadebug"/>
        </xsl:if>
    </xsl:template>
     <xsl:template name="top-error">
        <div id="message">
            <xsl:if test="//error">
                <xsl:attribute name="class">error</xsl:attribute>
                <xsl:value-of select="//error[1]"/>
            </xsl:if>
            <xsl:if test="not(//error) and //msg">

                <xsl:attribute name="class">message</xsl:attribute>

                <xsl:for-each select="//msg">
                    <xsl:value-of select="."/>
                    <br/>
                </xsl:for-each>
               </xsl:if>

        </div>
    </xsl:template>
   

    <xsl:template name="section">
        <div class="section" id="{@id}">
            <xsl:if test="@hide">
                <xsl:attribute name="style">display:none</xsl:attribute>
            </xsl:if>
            <xsl:if test="not(tab)">
                <table border="0" cellspacing="0" cellpadding="0" class="section-header">
                    <xsl:if test="@width">
                        <xsl:attribute name="width">
                            <xsl:value-of select="@width"/>
                        </xsl:attribute>
                    </xsl:if>
                    <xsl:if test="not(@width)">
                        <xsl:attribute name="width">100%</xsl:attribute>
                    </xsl:if>
                    <tbody>
                        <tr>
                            <td class="sectionstart">
                                <img src="images/c.gif"/>
                            </td>
                            <td class="sectionline">
                                <span class="cap">
                                    <xsl:value-of select="title"/>
                                </span>
                            </td>
                            <td class="section-icon">
                                <img src="images/c.gif" class="minus"/>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </xsl:if>
            <xsl:if test="tab">
                <xsl:call-template name="tabs"/>
            </xsl:if>
            <div class="secbody">
                <xsl:call-template name="actionbar"/>
                <xsl:if test="@b_error">
                    <div class="error">Information could not be displayed due to temporary error</div>
                </xsl:if>
                <xsl:if test="tab">
                    <xsl:call-template name="tabbody"/>
                </xsl:if>
                <xsl:if test="not(tab)">
                    <div class="content">
                        <xsl:if test="@option=4">
                            <table cellpadding="0" cellspacing="0" class="section-table">
                                <tr>
                                    <xsl:for-each select="*">
                                        <td>
                                            <xsl:apply-templates select="self::node()"/>
                                        </td>
                                    </xsl:for-each>
                                </tr>
                            </table>
                        </xsl:if>
                        <xsl:if test="not(@option=4)">
                            <xsl:apply-templates select="*"/>
                        </xsl:if>
                    </div>
                </xsl:if>
            </div>
        </div>
    </xsl:template>

    <xsl:template name="actionbar">
       <xsl:if test="maction">
           <div class="bar">
                <div class="barline">
                    <xsl:for-each select="maction">
                        <a>
                            <xsl:if test="not(@href)">
                                <xsl:attribute name="href">#</xsl:attribute>
                            </xsl:if>
                            <xsl:apply-templates select="@*"/>
                            <xsl:value-of select="."/>
                        </a>
                    </xsl:for-each>
                </div>
            </div>
        </xsl:if>
        

        <xsl:if test="actionbar">
            <div class="bar">
                <xsl:apply-templates select="actionbar/@*"/>
                <div class="barline">
                    <xsl:apply-templates select="actionbar/*"/>
                </div>
            </div>
     </xsl:if>

    </xsl:template>
    <xsl:template match="sec">
        <xsl:call-template name="section"/>
    </xsl:template>
    <xsl:template name="tabbody">
        <xsl:variable name="tabkey">
            <xsl:value-of select="@key"/>
        </xsl:variable>
        <xsl:for-each select="tab">
            <div id="{$tabkey}{position()}" style="display:none">
                <xsl:call-template name="actionbar"/>
                <div class="content">
                    <xsl:apply-templates select="*"/>
                </div>
            </div>
        </xsl:for-each>
    </xsl:template>
    <xsl:template name="tabs">
        <xsl:variable name="tabkey">
            <xsl:value-of select="@key"/>
        </xsl:variable>
        <div class="tabbar">
            <ul>
                <xsl:for-each select="tab">
                    <li tab="#{$tabkey}{position()}" id="tab_{$tabkey}{position()}">
                        <xsl:apply-templates select="@*"/>
                        <span class="{title/@class}">
                            <xsl:value-of select="title"/>
                        </span>
                    </li>
                </xsl:for-each>
            </ul>
            <div style="clear:both;height:1px"/>
        </div>
    </xsl:template>
    <xsl:template match="sub">
        <div class="sub-section">
            <xsl:if test="tab">
                <xsl:call-template name="tabs"/>
            </xsl:if>
            <xsl:if test="not(tab)">
                <table width="100%" cellpadding="0" cellspacing="0" class="sub-header">
                    <tr>
                        <td class="sub-start">
                            <img src="images/c.gif"/>
                        </td>
                        <td class="sub-headerline">
                            <span class="cap">
                                <xsl:value-of select="title"/>
                            </span>
                        </td>
                    </tr>
                </table>
            </xsl:if>
            <div class="secbody">
                <xsl:call-template name="actionbar"/>
                <xsl:if test="tab">
                    <xsl:call-template name="tabbody"/>
                </xsl:if>
                <xsl:if test="not(tab)">
                    <xsl:if test="@option=4">
                        <table cellpadding="0" cellspacing="0">
                            <tbody>
                                <tr>
                                    <xsl:for-each select="*">
                                        <td align="left">
                                            <xsl:apply-templates select="."/>
                                        </td>
                                    </xsl:for-each>
                                </tr>
                            </tbody>
                        </table>
                    </xsl:if>
                    <xsl:if test="not(@option=4)">
                        <xsl:for-each select="*">
                            <div style="float:left">
                                <xsl:apply-templates select="."/>
                            </div>
                        </xsl:for-each>
                    </xsl:if>
                </xsl:if>
            </div>
        </div>
    </xsl:template>
    <xsl:template match="columns">
        <table cellpadding="0" cellspacing="0" class="columns">
            <xsl:variable name="width">
                <xsl:if test="@width">
                    <xsl:value-of select="@width"/>
                </xsl:if>
                <xsl:if test="not(@width)">
                    <xsl:value-of select="round(100 div count(sec))"/>
                </xsl:if>
            </xsl:variable>
            <tr>
                <xsl:if test="@b_error">
                    <td class="error">Column cannot be displayed, due to data error</td>
                </xsl:if>
                <xsl:if test="not(@b_error)">
                    <xsl:for-each select="sec">
                        <td valign="top">
                            <xsl:if test="@width">
                                <xsl:attribute name="width">
                                    <xsl:value-of select="@width"/>
                                </xsl:attribute>
                            </xsl:if>
                            <xsl:if test="not(@width) and not(@full)">
                                <xsl:attribute name="style">width:<xsl:value-of select="$width"/>%
                                </xsl:attribute>
                            </xsl:if>
                            <div>
                                <xsl:if test="not(position()=1)">
                                    <xsl:attribute name="class">column</xsl:attribute>
                                </xsl:if>
                                <xsl:call-template name="section"/>
                            </div>
                        </td>
                    </xsl:for-each>
                </xsl:if>
            </tr>
        </table>
    </xsl:template>
    <xsl:template match="grid">
        <table class="grid" cellspacing="2" cellpadding="1">
            <xsl:apply-templates select="@*"/>
            <xsl:if test="@b_error">
                <tr>
                    <td class="error">Information could not be displayed due to temporary error</td>
                </tr>
            </xsl:if>
            <xsl:if test="not(@b_error)">
                <xsl:apply-templates select="row"/>
            </xsl:if>
        </table>
    </xsl:template>
    <xsl:template match="row">
            <xsl:if test="td">
			<xsl:call-template name="row"/>
			</xsl:if>
            <xsl:if test="not(td)">
            <thead><xsl:call-template name="row"/></thead>
            </xsl:if>

    </xsl:template>
    <xsl:template name="row">
        <xsl:if test="not(xxx)">
             <tr>
                <xsl:apply-templates select="@*"/>
                <xsl:apply-templates select="td|head"/>
            </tr>
        </xsl:if>
    </xsl:template>
    <xsl:template match="head">
        <th>
            <xsl:apply-templates select="@*"/>
            <xsl:call-template name="captiontext"/>
        </th>
    </xsl:template>
    <xsl:template match="fields">
        <xsl:if test="not(@option)">
            <xsl:for-each select="td">
                <table class="float_field">
                    <tr>
                        <th class="fixheight">
                            <xsl:call-template name="cap"/>
                        </th>
                    </tr>
                    <tr>
                        <xsl:call-template name="data"/>
                    </tr>
                </table>
            </xsl:for-each>
        </xsl:if>
        <xsl:if test="@option=3">
            <xsl:for-each select="td">
                <table class="float_field">
                    <tr>
                        <th>
                            <xsl:value-of select="cap"/>
                        </th>
                        <xsl:call-template name="data"/>
                    </tr>
                </table>
            </xsl:for-each>
        </xsl:if>
        <xsl:if test="@option=4">
            <table class="fields" cellpadding="1" cellspacing="2">
                <tr>
                    <xsl:for-each select="td">
                        <th class="fixheight">
                            <xsl:call-template name="cap"/>
                        </th>
                    </xsl:for-each>
                </tr>
                <tr>
                    <xsl:for-each select="td">
                        <xsl:call-template name="data"/>
                    </xsl:for-each>
                </tr>
            </table>
        </xsl:if>
        <xsl:if test="@option=5">
            <table class="fields" cellpadding="1" cellspacing="2">
                <xsl:for-each select="td">
                    <tr>
                        <th>
                            <xsl:call-template name="cap"/>
                        </th>
                        <xsl:call-template name="data"/>
                    </tr>
                </xsl:for-each>
            </table>
        </xsl:if>
        <xsl:if test="@option=35">
          
            <xsl:for-each select="maction">
                <div class="float_field data" style="width: 300px">
                    <a>
                            <xsl:if test="not(@href)">
                                <xsl:attribute name="href">#</xsl:attribute>
                            </xsl:if>
                            <xsl:apply-templates select="@*"/>
                            <xsl:value-of select="."/>
                        </a>

                </div>
             </xsl:for-each>
      
        </xsl:if>
        <div style="clear:both;height:1px"/>
    </xsl:template>
    <xsl:template name="cap">
        <xsl:for-each select="cap">
            <xsl:call-template name="captiontext"/>
        </xsl:for-each>
        <xsl:if test="not(cap)">
            <img src="images/c.gif" alt="" width="1" height="14"/>
        </xsl:if>
    </xsl:template>
    <xsl:template match="caption">
        <span class="caption">
            <xsl:value-of select="."/>
        </span>
    </xsl:template>
    <xsl:template name="captiontext">
        <xsl:variable name="string">
            <xsl:value-of select="."/>
        </xsl:variable>
        <xsl:choose>
            <xsl:when test="contains($string, '\')">
                <xsl:value-of select="substring-before($string,'\')"/>
                <br/>
                <xsl:value-of select="substring-after($string,'\')"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$string"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    <xsl:template match="table">
        <xsl:if test="@b_error">
            <span class="error">Information could not be displayed due to temporary error</span>
        </xsl:if>
        <xsl:if test="not(@b_error)">
            <div class="scrollbox">
                <table class="datatable" cellpadding="1" cellspacing="2">
                    <xsl:apply-templates select="@*"/>
                    <xsl:for-each select="row">
                        <xsl:if test="head">
                            <thead>
                               <xsl:call-template name="tablerow"/>
                            </thead>
                        </xsl:if>
                        <xsl:if test="not(head)">
                            <xsl:call-template name="tablerow"/>
                        </xsl:if>
                    </xsl:for-each>
                </table>
            </div>
        </xsl:if>
    </xsl:template>
    <xsl:template name="tablerow">
        <tr><xsl:apply-templates select="@*"/><xsl:apply-templates select="*"/> </tr>
    </xsl:template>
    <xsl:template match="th">
        <th colspan="{@colspan}">
            <xsl:value-of select="."/>
        </th>
    </xsl:template>
    <xsl:template match="td">
        <xsl:call-template name="data"/>
    </xsl:template>
    <xsl:template name="data">
        <td align="{@align}">
            <xsl:if test="not(input or select or multiselect or password or submit or combo or checkbox or inputdate )">
                <xsl:attribute name="class">data</xsl:attribute>
            </xsl:if>
            <xsl:if test="@b_error">
                <span class="error">error</span>
            </xsl:if>
            <xsl:if test="not(@b_error)">
            <xsl:for-each select="a">
                <a>
                <xsl:apply-templates select="@*"/>
                <xsl:value-of select="."/>
                </a><br/>
             </xsl:for-each>
              <xsl:if test="not(a)">
                <xsl:attribute name="nowrap">nowrap</xsl:attribute>
                <xsl:choose>
                    <xsl:when test="@space">
                        <div style="height:5px;width:5px"/>
                    </xsl:when>
                    <xsl:when test="@java">
                        <xsl:apply-templates select="@*"/>
                    </xsl:when>
                    <xsl:when test="@href">
                        <xsl:if test="not(@target)">
                            <xsl:attribute name="xclass">tdlink</xsl:attribute>
                        </xsl:if>
                        <xsl:call-template name="link"/>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:apply-templates select="@*"/>
                        <xsl:apply-templates select="*"/>
                        <xsl:choose>
                            <xsl:when test="@wrap">
                                <xsl:call-template name="wrap"/>
                            </xsl:when>
                            <xsl:otherwise>
                                <xsl:value-of select="text()"/>
                                <xsl:if test="count(child::node())=0 or (count(child::node())=1 and cap)">
                                    <xsl:if test="not(text())"> &#160;</xsl:if>
                                </xsl:if>
                            </xsl:otherwise>
                        </xsl:choose>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:if></xsl:if>
        </td>
    </xsl:template>
    <xsl:template match="image">
        <img src="images/{.}">
            <xsl:apply-templates select="@*"/>
        </img>
    </xsl:template>
    <xsl:template match="inputimage">
        <input type="image" class="but {.}" src="images/c.gif">
            <xsl:apply-templates select="@*"/>
        </input>
    </xsl:template>
    <xsl:template match="inputdate">
        <input type="text" class="text date-pick" cap="{../cap}" value="{.}" style="z-index:100">
            <xsl:if test="not(@name)">
                <xsl:attribute name="name">
                    <xsl:value-of select="@id"/>
                </xsl:attribute>
            </xsl:if>
            <xsl:apply-templates select="@*"/>
        </input>
    </xsl:template>
    <xsl:template match="combo">
        <select name="{@id}" id="{@id}" xvalue="{@value}" size="1" cap="{../cap}" action="{@action}" style="{@style}">
            <xsl:apply-templates select="item"/>
        </select>
    </xsl:template>
    <xsl:template name="link">
        <a href="{@href}" target="{@target}" class="{@linkclass}" title="{@title}">
            <xsl:value-of select="node()"/>
        </a>
    </xsl:template>
    <xsl:template match="input-search">
     <span nowrap="nowrap" class="searchmenu">
            <input type="text" class="text" size="12">
               <xsl:apply-templates select="@*"/>
            </input>
     </span>

    </xsl:template>
    <xsl:template match="input">
        <input type="text" value="{.}" cap="{../cap}" class="text">
            <xsl:apply-templates select="@*"/>
            <xsl:if test="@max">
                <xsl:attribute name="maxlength">
                    <xsl:value-of select="@max"/>
                </xsl:attribute>
            </xsl:if>
        </input>
    </xsl:template>
    <xsl:template match="listdata">
        <div class="y_scroll" style="height:100px">
            <xsl:for-each select="item">
                <xsl:value-of select="."/>
                <br/>
            </xsl:for-each>
        </div>
    </xsl:template>
    <xsl:template match="label">
        <xsl:if test="not(msg) and not(error)">
        <span>
            <xsl:if test="not(@class)">
                <xsl:attribute name="class">text</xsl:attribute>
            </xsl:if>

            <xsl:value-of select="." disable-output-escaping="yes"/>
        </span>
        </xsl:if>
    </xsl:template>
    <xsl:template match="text">
        <xsl:if test="not(msg) and not(error)">
        <div>
            <xsl:if test="not(@class)">
                <xsl:attribute name="class">text</xsl:attribute>
            </xsl:if>
           <xsl:apply-templates select="@*"/>
            <xsl:value-of select="." disable-output-escaping="yes"/>
        </div>
        </xsl:if>
    </xsl:template>
    <xsl:template match="form">
        <form >
            <xsl:if test="@method">
                <xsl:attribute name="fmethod">1</xsl:attribute>

            </xsl:if>
            <xsl:if test="@post">
                <xsl:attribute name="post">1</xsl:attribute>
            </xsl:if>
            <xsl:apply-templates select="@*"/>
            <xsl:apply-templates select="*"/>
        </form>
    </xsl:template>
    <xsl:template match="password">
        <input value="{.}" type="password" maxlength="{@max}" size="{@max}" name="{@name}" class="text">
            <xsl:apply-templates select="@*"/>
        </input>
    </xsl:template>
    <xsl:template match="multiselect">
        <table border="0" cellspacing="0" cellpadding="2" id="{@id}" class="multisel">
            <xsl:if test="search|combo">
                <tr>
                    <td colspan="3" align="right">
                        <xsl:if test="combo">
                            <xsl:call-template name="combolook"/>
                        </xsl:if>
                        <xsl:apply-templates select="search"/>
                    </td>
                </tr>
            </xsl:if>
            <tr>
                <td class="label">Selected</td>
                <td/>
                <td nowrap="nowrap" class="label">Choose from</td>
            </tr>
            <tr>
                <td>
                    <xsl:apply-templates select="select[1]"/>
                </td>
                <td align="center" width="40" valign="middle">
                    <p/>
                    <img src="images/c.gif" class="selall but int"/>
                    <p/>
                    <img src="images/c.gif" class="sel but int"/>
                    <p/>
                    <img src="images/c.gif" class="desel but int"/>
                    <p/>
                    <img src="images/c.gif" class="deselall but int"/>
                </td>
                <td>
                    <xsl:apply-templates select="select[2]">


                    </xsl:apply-templates>
                </td>
            </tr>
        </table>
    </xsl:template>
    <xsl:template name="combolook">
        <div class="search">
            <table>
                <tr>
                    <td>
                        <span/>
                    </td>
                    <td align="left" class="label">
                        <xsl:value-of select="cap"/>
                    </td>
                    <td>
                        <xsl:apply-templates select="combo"/>
                    </td>
                </tr>
            </table>
        </div>
    </xsl:template>
    <xsl:template match="checkbox">
        <input type="checkbox" id="{@id}" class="checkbox" name="{.}">
            <xsl:apply-templates select="@*"/>
            <xsl:if test="@checked">
                <xsl:attribute name="value">1</xsl:attribute>
            </xsl:if>
            <xsl:if test="not(@checked)">
                <xsl:attribute name="value">0</xsl:attribute>
            </xsl:if>
        </input>
    </xsl:template>
    <xsl:template match="search">
        <div class="search">
            <table>
                <tr>
                    <td class="label">
                        <xsl:value-of select="cap"/>
                    </td>
                    <td>
                        <input type="text" name="{@name}" xname="{@xname}" class="searchinput text"/>
                        <br/>
                    </td>
                    <td>
                        <input type="image" src="images/c.gif" class="but search int" xname="{@action}" url="{@url}"/>
                    </td>
                </tr>
                <xsl:if test="@single">
                    <tr>
                        <td class="label">
                            Selected Code
                        </td>
                        <td colspan="2">
                            <select name="{@xname}"  size="1" class="searchinputresult" style="width:150px" fieldname="{@fname}">
                          
                            </select>
                        </td>
                        <td/>
                    </tr>
                </xsl:if>
            </table>
            <span class="error"/>
        </div>
    </xsl:template>
    <xsl:template match="hidden">
        <input type="hidden" value="{.}" cap="{../cap}">
            <xsl:apply-templates select="@*"/>
        </input>
    </xsl:template>
    <xsl:template match="select">
        <select size="10" multiple="multiple" style="width:200px" name="{@name}" class="list">
            <xsl:apply-templates select="@id|@disabled"/>
            <xsl:apply-templates select="item"/>
        </select>
    </xsl:template>
    <xsl:template match="textarea">
    <textarea >
          <xsl:apply-templates select="@*"/>
          <xsl:value-of select="."/>
      </textarea>
    </xsl:template>
    <xsl:template match="item">
        <option title="{@title}">
            <xsl:if test="@key">
                <xsl:attribute name="value">
                    <xsl:value-of select="@key"/>
                </xsl:attribute>
            </xsl:if>
            <xsl:if test="not(@key)">
                <xsl:attribute name="value">
                    <xsl:value-of select="."/>
                </xsl:attribute>
            </xsl:if>
            <xsl:value-of select="."/>
        </option>
    </xsl:template>
    <xsl:template match="lable">
        <lable>
            <xsl:value-of select="."/>
        </lable>
    </xsl:template>
    <xsl:template match="br">
        <br/>
    </xsl:template>
    <xsl:template match="space">
        <div style="height:5px;width:5px"/>
    </xsl:template>
    <xsl:template match="action">
        <a>
            <xsl:if test="not(@href)">
                <xsl:attribute name="href">#</xsl:attribute>
            </xsl:if>
            <xsl:apply-templates select="@*"/>
            <xsl:value-of select="."/>
        </a>
    </xsl:template>
    <xsl:template match="moveaction">
        <a class="empty">
            <xsl:apply-templates select="@*"/>
            <img alt="drag" src="images/c.gif" class="dragdrop move but" style="margin-right:4px"/>
            <xsl:value-of select="."/>
        </a>
    </xsl:template>
    <xsl:template name="submit">
        <input type="submit" class="submit" value="{.}">
            <xsl:apply-templates select="@*"/>
        </input>
    </xsl:template>
    <xsl:template match="submit">
        <xsl:call-template name="submit"/>
    </xsl:template>
    <xsl:template match="submitbar">
        <div class="submitbar">
            <xsl:call-template name="submit"/>
            <xsl:apply-templates select="*"/>
        </div>
    </xsl:template>


    <xsl:template name="wrap">
        <table cellpadding="0" cellspacing="0" style="table-layout:fixed;width:100%">
            <tr>
                <td nowrap="nowrap" style="overflow:hidden" title="{text()}">
                    <xsl:value-of select="text()"/>
                </td>
            </tr>
        </table>
    </xsl:template>
    <xsl:template match="jscript">
        <div style="display:none"  >
            <xsl:attribute name="class">
               <xsl:value-of select="."/>
            </xsl:attribute>

        </div>
    </xsl:template>
    <xsl:template match="div">
          <div>
           <xsl:apply-templates select="@*"/>
           <xsl:apply-templates select="*"/></div>
    </xsl:template>
    <xsl:template match="@*">
        <xsl:choose>
            <xsl:when test="@space">
                <div style="height:5px;width:5px"/>
            </xsl:when>
            <xsl:when test="@java">
                <xsl:apply-templates select="@java"/>
            </xsl:when>
            <xsl:when test="@href">
                <xsl:call-template name="link"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:attribute name="{name()}">
                    <xsl:value-of select="."/>
                </xsl:attribute>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    <xsl:template match="@javax">
        <a href="javascript:" java="{.}">
            <xsl:value-of select="../node()"/>
        </a>
    </xsl:template>
    <xsl:template match="iframe">
        <iframe frameBorder="0" width="100%"  id="{@id}" class="{@class}">
            <xsl:attribute name="src">
                <xsl:value-of select="."/>
            </xsl:attribute>
        </iframe>

    </xsl:template>
    <xsl:template match="@disabled">
        <xsl:attribute name="disabled">disabled</xsl:attribute>
    </xsl:template>
    <xsl:template match="@id">
        <xsl:attribute name="id">
            <xsl:value-of select="."/>
        </xsl:attribute>
    </xsl:template>
    <xsl:template match="xmlhtml">
    <xsl:call-template name="copy"/>
   
    
    
    </xsl:template>
    <xsl:template name="copy">
    <xsl:for-each select="*"><xsl:copy>
     <xsl:apply-templates select="@*"/><xsl:value-of select="."/>
    <xsl:call-template name="copy"/>
    </xsl:copy></xsl:for-each>    
    </xsl:template>    
    <xsl:template match="@mandatory">
        <xsl:attribute name="mandatory">1</xsl:attribute>
    </xsl:template>
    <xsl:template match="ns">
        <xsl:apply-templates select="*"/>
    </xsl:template>
    <xsl:template match="*"/>
</xsl:stylesheet>
