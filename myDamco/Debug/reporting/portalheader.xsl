<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="html" doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"/>
    <xsl:template name="portalheader">
         <xsl:if test="//globalmenu">
                    <script src="{//globalmenu}" type="text/javascript"/>
                    <xsl:if test="//application/portalclass">
                    <style>
                    .<xsl:value-of select="//application/portalclass"/> { background-position : right top !important; }
                    .<xsl:value-of select="//application/portalclass"/> a { color: #FFFFFF !important;}
                    .<xsl:value-of select="//application/portalclass"/> span { background-position : 0px 0px !important;}


                      </style>
                     </xsl:if>
                </xsl:if>
    </xsl:template>
    <xsl:template name="newpagetop">
         <xsl:if test="not(popup)">
               <div id="myDamcoTopbar" style="padding-bottom:16px"></div>
        </xsl:if>
    </xsl:template>
</xsl:stylesheet>