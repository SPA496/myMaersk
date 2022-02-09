IF DB_NAME() = 'Master' 
BEGIN
	RaisError ('The current database is master', 20, -1) with log
	set noexec on
END

SET ANSI_NULLS ON
GO

-- DEBUG - drop all data
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Navigation_IECompatibilityMode]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Navigation] DROP CONSTRAINT [DF_Navigation_IECompatibilityMode]
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Navigation]') AND type in (N'U'))
DROP TABLE [dbo].[Navigation]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_WidgetInstancesHistory_Widgets]') AND parent_object_id = OBJECT_ID(N'[dbo].[WidgetInstanceHistory]'))
ALTER TABLE [dbo].[WidgetInstanceHistory] DROP CONSTRAINT [FK_WidgetInstancesHistory_Widgets]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_WidgetInstances_Widgets]') AND parent_object_id = OBJECT_ID(N'[dbo].[WidgetInstance]'))
ALTER TABLE [dbo].[WidgetInstance] DROP CONSTRAINT [FK_WidgetInstances_Widgets]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_WidgetInstances_Configuration]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[WidgetInstance] DROP CONSTRAINT [DF_WidgetInstances_Configuration]
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WidgetInstanceHistory]') AND type in (N'U'))
DROP TABLE [dbo].[WidgetInstanceHistory]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WidgetInstance]') AND type in (N'U'))
DROP TABLE [dbo].[WidgetInstance]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Downtime_NewsItems]') AND parent_object_id = OBJECT_ID(N'[dbo].[Downtime]'))
ALTER TABLE [dbo].[Downtime] DROP CONSTRAINT [FK_Downtime_NewsItems]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Downtime]') AND type in (N'U'))
DROP TABLE [dbo].[Downtime]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NewsItem]') AND type in (N'U'))
DROP TABLE [dbo].[NewsItem]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NewsCategory]') AND type in (N'U'))
DROP TABLE [dbo].[NewsCategory]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Widgets_Configuration]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Widget] DROP CONSTRAINT [DF_Widgets_Configuration]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Widgets_Disabled]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Widget] DROP CONSTRAINT [DF_Widgets_Disabled]
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Widget]') AND type in (N'U'))
DROP TABLE [dbo].[Widget]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ServiceTokens_Service]') AND parent_object_id = OBJECT_ID(N'[dbo].[ServiceTokens]'))
ALTER TABLE [dbo].[ServiceTokens] DROP CONSTRAINT [FK_ServiceTokens_Service]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServiceTokens]') AND type in (N'U'))
DROP TABLE [dbo].[ServiceTokens]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Services_Configuration]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Services] DROP CONSTRAINT [DF_Services_Configuration]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ELMAH_Error_ErrorId]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ELMAH_Error] DROP CONSTRAINT [DF_ELMAH_Error_ErrorId]
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ELMAH_Error]') AND type in (N'U'))
DROP TABLE [dbo].[ELMAH_Error]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ELMAH_GetErrorsXml]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ELMAH_GetErrorsXml]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ELMAH_GetErrorXml]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ELMAH_GetErrorXml]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ELMAH_LogError]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ELMAH_LogError]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Page]') AND type in (N'U'))
DROP TABLE [dbo].[Page]
GO
