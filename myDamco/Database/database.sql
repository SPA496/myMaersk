IF DB_NAME() = 'Master' 
BEGIN
	RaisError ('The current database is master', 20, -1) with log
	set noexec on
END

-- Create myDamco tables
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Widget' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Widget](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UId] [nvarchar](255) NOT NULL,
        [Title] [nvarchar](255) NOT NULL,
        [Description] [nvarchar](255) NOT NULL, 
        [Category] [nvarchar](255) NOT NULL,
        [Icon] [nvarchar](255), -- Name of icon, will add -small.gif or -large.png depending on where it is used
        [Template] [nvarchar](255) NOT NULL, -- Name of partial view in code		
        [TemplateHTML] [nvarchar](max), -- HTML Data of actual widget implementation
        [TemplateInstanceHTML] [nvarchar](max), -- HTML Data of actual instance widget implementation
        [TemplateCSS] [nvarchar](max), -- CSS Data of actual widget implementation
        [TemplateJS] [nvarchar](max), -- JS Data of actual widget implementation
        [Configuration] [nvarchar](max) NOT NULL,
		[ServiceURL] [nvarchar](255),
        [ServiceConfiguration] [nvarchar](max) NOT NULL,
        [ConfigurationSchema] [nvarchar](max), -- JSON validation schema		
        [ServiceConfigurationSchema] [nvarchar](max), -- JSON validation schema
        [InstanceConfigurationSchema] [nvarchar](max), -- JSON validation schema
        [Editable] [bit] NOT NULL, -- Can the user configure data on the widget?
        [Disabled] [bit] NOT NULL, 
        [UAMApplication] [nvarchar](255),
        [UAMFunction] [nvarchar](255),
        CONSTRAINT [PK_Widgets] PRIMARY KEY CLUSTERED ( [Id] ASC ) 
    );	
    ALTER TABLE [dbo].[Widget] ADD CONSTRAINT [DF_Widgets_Configuration]  DEFAULT (N'{}') FOR [Configuration];
    ALTER TABLE [dbo].[Widget] ADD CONSTRAINT [DF_Widgets_ConfigurationSchema]  DEFAULT (N'{}') FOR [ConfigurationSchema];
    ALTER TABLE [dbo].[Widget] ADD CONSTRAINT [DF_Widgets_InstanceConfigurationSchema] DEFAULT (N'{}') FOR [InstanceConfigurationSchema];
    ALTER TABLE [dbo].[Widget] ADD CONSTRAINT [DF_Widgets_Editable]  DEFAULT (0) FOR [Editable];
    ALTER TABLE [dbo].[Widget] ADD CONSTRAINT [DF_Widgets_Disabled]  DEFAULT (0) FOR [Disabled];
    CREATE UNIQUE INDEX [IX_Widgets] ON [dbo].[Widget] ([Uid]);
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WidgetInstance' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[WidgetInstance] (
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Title] [nvarchar](255), 
        [Login] [nvarchar](255) NOT NULL,
        [Role] [int] NOT NULL,
        [Configuration] [nvarchar](max) NOT NULL,
        [DashboardColumn] [tinyint] NOT NULL, -- Column on dashboard
        [DashboardPriority] [tinyint] NOT NULL, -- Order in column on dashboard
        [Widget_Id] [int] NOT NULL,
        CONSTRAINT [PK_WidgetInstances] PRIMARY KEY CLUSTERED ( [Id] ASC )
    );
    ALTER TABLE [dbo].[WidgetInstance] ADD CONSTRAINT [DF_WidgetInstances_Configuration] DEFAULT (N'{}') FOR [Configuration];
    ALTER TABLE [dbo].[WidgetInstance] WITH CHECK ADD CONSTRAINT [FK_WidgetInstances_Widgets] FOREIGN KEY([Widget_Id]) REFERENCES [dbo].[Widget]([Id]);
    ALTER TABLE [dbo].[WidgetInstance] CHECK CONSTRAINT [FK_WidgetInstances_Widgets];
    CREATE NONCLUSTERED INDEX [IX_WidgetInstances] ON [dbo].[WidgetInstance] ([Login], [Role]);
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WidgetInstanceHistory' AND xtype='U')
BEGIN
	CREATE TABLE [dbo].[WidgetInstanceHistory] (
		[WidgetInstance_Id] [int] NOT NULL, 			-- Corresponds to an ID in the WidgetInstance table, which may or may not have been deleted (Important to have so that DeleteTime can be set)
		[Login]	[nvarchar](255) NOT NULL, 
		[Role] [int] NOT NULL, 
		[AddTime] [datetime] NOT NULL,					-- The time (UTC) of which it was added to the users dashboard
		[DeleteTime] [datetime],                        -- The time (UTC) of which it was removed from the users dashboard
		[Widget_Id] [int] NOT NULL,
		CONSTRAINT [PK_WidgetInstanceHistory] PRIMARY KEY CLUSTERED ( [WidgetInstance_Id] ASC ) 
	);
	CREATE INDEX [IX_WidgetInstanceHistory_AddTime] ON [dbo].[WidgetInstanceHistory] ([AddTime]);
	CREATE INDEX [IX_WidgetInstanceHistory_DeleteTime] ON [dbo].[WidgetInstanceHistory] ([DeleteTime]);
	CREATE INDEX [IX_WidgetInstanceHistory_Widget_Id] ON [dbo].[WidgetInstanceHistory] ([Widget_Id]);

	-- TODO: Ok? Does this cause cascade delete? Or does it prevent a widget from being deleted? TEST!
	ALTER TABLE [dbo].[WidgetInstanceHistory] WITH CHECK ADD CONSTRAINT [FK_WidgetInstancesHistory_Widgets] FOREIGN KEY([Widget_Id]) REFERENCES [dbo].[Widget]([Id]); 
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Setting' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Setting](
        [Id]		  INT IDENTITY(1,1) NOT NULL,
		[Name]        NVARCHAR(255) NOT NULL,
		[Value]       NVARCHAR(255) NOT NULL,
		[Description] NVARCHAR(max) NOT NULL,        
        CONSTRAINT [PK_Setting] PRIMARY KEY CLUSTERED ( [Id] ASC ) 
    );	
	CREATE UNIQUE INDEX [IX_Setting_Name] ON [dbo].[Setting] ([Name]);
END

IF NOT EXISTS (select 1 from Setting where [Name] = 'Piwik.Enable Tracking') 
BEGIN 
    INSERT INTO [dbo].[Setting] ([Name], [Value], [Description]) VALUES ('Piwik.Enable Tracking', 'false', 'If value is true, piwik tracking is enabled, otherwise it is disabled.');
END

IF NOT EXISTS (select 1 from Setting where [Name] = 'Piwik.SiteID') 
BEGIN
    INSERT INTO [dbo].[Setting] ([Name], [Value], [Description]) VALUES ('Piwik.SiteID', '', 'The SiteID of myDamco on the Piwik server (an integer or empty for none).');
END

IF NOT EXISTS (select 1 from Setting where [Name] = 'Piwik.ServerUrl') 
BEGIN
    INSERT INTO [dbo].[Setting] ([Name], [Value], [Description]) VALUES ('Piwik.ServerUrl', '', 'The URL of the Piwik server, without leading "http" or "https". Example: "://127.0.0.1:9999/piwik/". (empty for none)');
END

IF NOT EXISTS (select 1 from Setting where [Name] = 'Logging.EnableClientLogging') 
BEGIN
    INSERT INTO [dbo].[Setting] ([Name], [Value], [Description]) VALUES ('Logging.EnableClientLogging', 'true', 'If value is true, client-side JavaScript errors are logged to Elmah, otherwise they are not.');
END

IF NOT EXISTS (select 1 from Setting where [Name] = 'Navigation.CompressExternalMenuJavaScript') 
BEGIN
    INSERT INTO [dbo].[Setting] ([Name], [Value], [Description]) VALUES ('Navigation.CompressExternalMenuJavaScript', 'true', 'If value is true, our javascript for the external menu is compressed, otherwise it is not.');
END

IF NOT EXISTS (select 1 from Setting where [Name] = 'Google Analytics.Enable Tracking') 
BEGIN 
    INSERT INTO [dbo].[Setting] ([Name], [Value], [Description]) VALUES ('Google Analytics.Enable Tracking', 'false', 'If value is true, tracking by Google Analytics is enabled, otherwise it is disabled.');
END

IF NOT EXISTS (select 1 from Setting where [Name] = 'Google Analytics.Tracking ID') 
BEGIN 
    INSERT INTO [dbo].[Setting] ([Name], [Value], [Description]) VALUES ('Google Analytics.Tracking ID', '', 'The tracking ID UA-XXXXX-Y containing Google Analytics account and property number.');
END

IF NOT EXISTS (select 1 from Setting where [Name] = 'WalkMe.Enable Tracking') 
BEGIN 
    INSERT INTO [dbo].[Setting] ([Name], [Value], [Description]) VALUES ('WalkMe.Enable Tracking', 'false', 'If value is true, tracking by WalkMe is enabled, otherwise it is disabled.');
END

IF NOT EXISTS (select 1 from Setting where [Name] = 'WalkMe.Source') 
BEGIN 
    INSERT INTO [dbo].[Setting] ([Name], [Value], [Description]) VALUES ('WalkMe.Source', '', 'The URL specified for the walkme.src property, without leading "http" or "https". Example: "://cdn.walkme.com/users/414f5f215d9440309070e18f0539b26f/walkme_414f5f215d9440309070e18f0539b26f_https.js". (empty for none)');
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DashboardTemplate' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[DashboardTemplate] (
        [Id]		              INT IDENTITY(1,1) NOT NULL,
		[LoginCopiedFrom]         NVARCHAR(255) NOT NULL,		-- The user this dashboard template was copied from (for use in the admin-UI only)
		[RoleCopiedFrom]          INT NOT NULL,				    -- The role this dashboard template was copied from (for use in the admin-UI only) **TODO: Remove? Don't know yet if needed**
		[Role]                    INT NOT NULL,				    -- The role for which this dashboard template applies
		[Description]             NVARCHAR(max) NOT NULL,		-- A description of this dashboard template (for use in the admin-UI only)
        [UpdatedAt]				  DATETIME NOT NULL,			-- Date of creation or of last update
        [UpdatedBy]				  NVARCHAR(50) NOT NULL,		-- Admin user who created or last updated
		[CachedRoleName]          NVARCHAR(255) NOT NULL,		-- Cached name    of the role, for use in Admin UI. Reason: It will require many UAM-webservice calls to look this up by roleId (call getOrganizations() and then for each org, call getRolesByOrganization(), and search the returned roles for the roleId. Or could possibly use getAllUserRoles() for the user, but he might have changed roles since he created the dashboard template.)
		[CachedOrganizationId]    INT           NOT NULL,		-- Cached orgId   of the role, for use in Admin UI. Reason: It will require many UAM-webservice calls to look this up by roleId (call getOrganizations() and then for each org, call getRolesByOrganization(), and search the returned roles for the roleId. Or could possibly use getAllUserRoles() for the user, but he might have changed roles since he created the dashboard template.)
		[CachedOrganizationName]  NVARCHAR(255) NOT NULL,		-- Cached orgName of the org , for use in Admin UI. (**TODO? Not too hard to look up using the webservice - only 1 call**)
        CONSTRAINT [PK_DashboardTemplate] PRIMARY KEY CLUSTERED ( [Id] ASC ) 
    );	
	CREATE UNIQUE INDEX [IX_DashboardTemplate_Role] ON [dbo].[DashboardTemplate] ([Role]);
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Navigation' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Navigation] (
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UId] [nvarchar](255) NOT NULL,
        [Title] [nvarchar](255) NOT NULL,
        [Url] [nvarchar](max) NOT NULL,
        [Target] [nvarchar](255) NOT NULL,
        [IECompatibilityMode] [nvarchar](255) NOT NULL,
        [Priority] [tinyint] NOT NULL, -- The order listed in the top bar
        [UAMApplication] [nvarchar](255),
        [UAMFunction] [nvarchar](255),
		[NewTab] [bit] NOT NULL DEFAULT 0, -- 1 = open in new window/tab, 0 = open in this window/tab
        CONSTRAINT [PK_Navigation] PRIMARY KEY CLUSTERED ( [id] ASC )
    );

    ALTER TABLE [dbo].[Navigation] ADD CONSTRAINT [DF_Navigation_IECompatibilityMode]  DEFAULT (N'None') FOR [IECompatibilityMode]
    CREATE UNIQUE INDEX [IX_Navigation] ON [dbo].[Navigation] ([Uid]);
END

IF NOT EXISTS (select * from information_schema.columns where table_name = 'Navigation' and column_name = 'NewTab')
BEGIN
    ALTER TABLE [dbo].[Navigation] ADD [NewTab] [bit] NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsCategory' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[NewsCategory] (
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [Language] [nvarchar](255) NOT NULL,
        [Description] [nvarchar](max),
        [Downtime] [bit] NOT NULL,
        [editUAMApplication] [nvarchar](255),
        [editUAMFunction] [nvarchar](255),
        [showUAMApplication] [nvarchar](255),
        [showUAMFunction] [nvarchar](255),
		[Configuration] NVARCHAR(max) NOT NULL DEFAULT N'{}'
        CONSTRAINT [PK_NewsCategories] PRIMARY KEY CLUSTERED ( [id] ASC )
    );
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Configuration' AND [object_id] = OBJECT_ID(N'NewsCategory'))
BEGIN
    ALTER TABLE [NewsCategory] ADD [Configuration] NVARCHAR(max) NOT NULL DEFAULT N'{}'; -- Add "Configuration" column to the NewsCategory Column, if it does not exist already. 
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsItem' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[NewsItem] (
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Title] [nvarchar](255) NOT NULL,
        [Description] [nvarchar](max),
        [Body] [nvarchar](max),
        [From] [datetime] NOT NULL,
        [To] [datetime],
        [NewsCategory_Id] [int] NOT NULL,
        [CreatedAt] [datetime] NULL,
        [CreatedBy] [nvarchar](50) NULL,
        [UpdatedAt] [datetime] NULL,
        [UpdatedBy] [nvarchar](50) NULL,
    CONSTRAINT [PK_NewsItems] PRIMARY KEY CLUSTERED ( [id] ASC )
    );
    
    ALTER TABLE [dbo].[NewsItem] WITH CHECK ADD  CONSTRAINT [FK_NewsItems_NewsCategoies] FOREIGN KEY([NewsCategory_Id]) REFERENCES [dbo].[NewsCategory]([Id]);

    END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Downtime' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Downtime] (
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [From] [datetime] NOT NULL,
        [To] [datetime] NOT NULL,
        [NewsItem_Id] [int] NOT NULL,
        [UAMApplication] [nvarchar](255),
        [UAMFunction] [nvarchar](255),
        CONSTRAINT [PK_Downtime] PRIMARY KEY CLUSTERED ( [id] ASC )
    );
    ALTER TABLE [dbo].[Downtime] WITH CHECK ADD CONSTRAINT [FK_Downtime_NewsItems] FOREIGN KEY([NewsItem_Id]) REFERENCES [dbo].[NewsItem]([Id]);
END


IF NOT EXISTS(select * from sys.columns where Name = N'Message' and Object_ID = Object_ID(N'Downtime'))    
BEGIN
	ALTER TABLE [dbo].[Downtime] ADD [Message] [nvarchar](max);
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Page' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Page] (
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UId] [varchar](50) NOT NULL,
        [Title] [varchar](250) NOT NULL,
        [Body] [text] NOT NULL,
        CONSTRAINT [PK_Page] PRIMARY KEY CLUSTERED ( [id] ASC )
    );
    CREATE UNIQUE INDEX [IX_Page] ON [dbo].[Page] ([UId]);
END

IF NOT EXISTS(SELECT * FROM [dbo].[Widget] WHERE [Uid] = 'externalnews')
BEGIN
INSERT INTO [dbo].[Widget] ([Uid], [Title], [Description], [Category], [Icon], [Template], [Configuration], [ConfigurationSchema], [ServiceConfiguration], [ServiceURL], [UAMApplication], [UAMFunction], [Editable])
    VALUES('externalnews', 
    'External News', 
    'Displays the latest industry news', 
    'General', 
    'news', 
    'RSSNews', 
    '{
        "feeds": [
            {
                "id": "lloydslist",
                "title": "Lloyds List Ports and Logistics"
            },
            {
                "id": "drapersonline",
                "title": "Drapers Online"
            },
            {
                "id": "americanshippersupplychain",
                "title": "American Shipper Supply Chain"
            },
            {
                "id": "americanshippertransportation",
                "title": "American Shipper Transportation"
            },
            {
                "id": "americanshippertechnology",
                "title": "American Shipper Technology"
            }
        ],
        "headlinecount": 10
    }',
    '{
        "type":"object",
        "$schema": "http://json-schema.org/draft-03/schema",
        "id": "#",
        "required":false,
        "properties":{
            "feeds": {
                "type":"array",
                "id": "feeds",
                "required":false,
                "items": {
                    "type":"object",
                    "id": "0",
                    "required":false,
                    "properties": {
                        "id": {
                            "type":"string",
                            "id": "id",
                            "required":true
                        },
                        "title": {
                            "type":"string",
                            "id": "title",
                            "required":true
                        },
                    }
                }
            }
        }
    }', 
    '{
        "feeds": [
            {
                "id": "lloydslist",
                "url": "https://www.lloydslist.com/ll/sector/ports-and-logistics/?service=rss",
                "timeout": 10,
                "cache": 900
            },
            {
                "id": "drapersonline",
                "url": "http://www.drapersonline.com/XmlServers/navsectionRSS.aspx?navsectioncode=6",
                "timeout": 10,
                "cache": 900
            },
            {
                "id": "americanshippersupplychain",
                "url": "http://americanshipper.com/Rss.aspx?sn=NewsSupplyChain",
                "timeout": 10,
                "cache": 900
            },
            {
                "id": "americanshippertransportation",
                "url": "http://americanshipper.com/Rss.aspx?sn=NewsTransportation",
                "timeout": 10,
                "cache": 900
            },
            {
                "id": "americanshippertechnology",
                "url": "http://americanshipper.com/Rss.aspx?sn=NewsTechnology",
                "timeout": 10,
                "cache": 900
            }
        ]
    }',
	'Services/ExternalNews/selectedFeed',
    'MYDAMCO', 
    'USE', 
    1);
END
GO

IF NOT EXISTS(SELECT * FROM [dbo].[Widget] WHERE [Uid] = 'performancechart')
BEGIN
INSERT INTO [dbo].[Widget] ([Uid], [Title], [Description], [Category], [Icon], [Template], [Configuration], [UAMApplication], [UAMFunction], [Editable], [ServiceConfiguration])
    VALUES('performancechart', 'Performance Chart', 'Displays report data in a drillable chart', 'General', 'performancepiechart', 'PerformanceChart',
    '{"ReportingChartDataServiceUrl": "https://reporting.damco.com/Reporting/jsonp?action=ChartData&returnData={3}&definitionId={0}&filterString={1}&groupByColumn={2}",
        "ReportingColumnsServiceUrl": "https://reporting.damco.com/Reporting/jsonp?action=ColumnDefinitions&returnData={1}&definitionId={0}",
        "ReportingConfigurationServiceUrl": "https://reporting.damco.com/Reporting/jsonp?action=DefinitionList",
        "ReportingDetailsUrlTemplate": "?action=ds_extract_view&definitionId={0}&filterString={1}&groupByColumn={2}&returnData=",
		"targeturl": "https://reporting.damco.com/Reporting/page"
    }', 'REPORTING_TRACKTRACE', 'REPORTING_DASHBOARD', 1, '{}');
END
GO

IF NOT EXISTS(SELECT * FROM [dbo].[Widget] WHERE [Uid] = 'whoami')
BEGIN
INSERT INTO [dbo].[Widget] ([Uid], [Title], [Description], [Category], [Icon], [Template], [UAMApplication], [UAMFunction], [ServiceURL], [ServiceConfiguration], [Configuration])
	VALUES('whoami', 'Who Am I?', 'Displays user information', 'General', 'whoami', 'WhoAmI', 'MYDAMCO', 'USE', 'Services/WhoAmI', '{}', '{}');
END

IF NOT EXISTS(SELECT * FROM [dbo].[Widget] WHERE [Uid] = 'elearning')
BEGIN
INSERT INTO [dbo].[Widget] ([Uid], [Title], [Description], [Category], [Icon], [Template], [UAMApplication], [UAMFunction], [ServiceURL], [ServiceConfiguration], [Configuration]) 
    VALUES('elearning', 'E-Learning', 'Table of content for E-Learning modules', 'General', 'recentsearches', 'ELearning', 'MYDAMCO', 'USE',
	'Services/ELearning',
    '{
        "folders": [
            { "application": "Reporting", "folder": "\\\\10.255.220.19\\Production\\Elearning\\REPORTING", "UAMApplication": "REPORTING", "width": 1031, "height": 804 },
            { "application": "Booking", "folder": "\\\\10.255.220.19\\Production\\Elearning\\BOOKING", "UAMApplication": "BOOKING", "width": 1031, "height": 804 }
         ]
    }',
    '{
        "targeturl": "Services/ELearningShow"
    }');
END

IF NOT EXISTS(SELECT * FROM [dbo].[Widget] WHERE [Uid] = 'damconews')
BEGIN
INSERT INTO [dbo].[Widget] ([Uid], [Title], [Description], [Category], [Icon], [Template], [Configuration], [ConfigurationSchema], [ServiceURL], [ServiceConfiguration], [UAMApplication], [UAMFunction], [Editable])
    VALUES('damconews', 'Damco News', 'Displays the latest news from Damco', 'General', 'news', 'RSSNews',
    '{
    "feeds": [
        {
            "id": "2",
            "title": "General Damco News"
        }
    ],
    "headlinecount": 10,
	"showarchive": "true"
    }',
    '{
    "type":"object",
    "$schema": "http://json-schema.org/draft-03/schema",
    "id": "#",
    "required":false,
    "properties":{
        "feeds": {
            "type":"array",
            "id": "feeds",
            "required":false,
            "items":
                {
                    "type":"object",
                    "id": "0",
                    "required":false,
                    "properties":{
                        "id": {
                            "type":"string",
                            "id": "id",
                            "required":true
                        },
                        "title": {
                            "type":"string",
                            "id": "title",
                            "required":true
                        },
                    }
                }
        }
    }
	}',
	'Services/DamcoNews/selectedFeed',
	'{}', 
	'MYDAMCO', 'USE', 1);
END

IF NOT EXISTS(SELECT * FROM [dbo].[Widget] WHERE [Uid] = 'recentpouches')
BEGIN
INSERT INTO [dbo].[Widget] ([Uid], [Title], [Description], [Category], [Icon], [Template], [UAMApplication], [UAMFunction], [ServiceURL], [ServiceConfiguration], [Configuration])
    VALUES('recentpouches', 'Recent Pouches', 'Displays the latest released pouches in an overview table', 'Document Management', 'recentpouches', 'RecentPouches', 'DOCUMENT_MANAGEMENT', '', 
	'Services/RecentPouches',
    '{ 
        "remoteaddress": "http://documentationws-damco.apmoller.net/RecentPouchWidgetWS/RecentPouchWidgetWS?WSDL"
    }', 
    '{ 
        "targeturl": "https://documentation.damco.com/eDOCWeb/ExecuteWidgetSearch.do" 
    }');
END

IF NOT EXISTS(SELECT * FROM [dbo].[Widget] WHERE [Uid] = 'recentreports')
BEGIN
INSERT INTO [dbo].[Widget] ([Uid], [Title], [Description], [Category], [Icon], [Template], [UAMApplication], [UAMFunction], [ServiceURL], [ServiceConfiguration], [Configuration])
    VALUES('recentreports', 'Recent Reports', 'Displays the latest reports in an overview table', 'Reporting', 'recentpouches', 'RecentReports', 'REPORTING_TRACKTRACE', '', 
	'Services/RecentReports',
    '{
        "remoteaddress": "http://reporting-services.apmoller.net/ws/reporting/ReportingWebServce?WSDL"
    }',
    '{ 
        "targeturl": "http://reporting.damco.com/Reporting/page" 
    }');
END

IF NOT EXISTS(SELECT * FROM [dbo].[Widget] WHERE [Uid] = 'scheduledreports')
BEGIN
INSERT INTO [dbo].[Widget] ([Uid], [Title], [Description], [Category], [Icon], [Template], [UAMApplication], [UAMFunction], [ServiceURL], [ServiceConfiguration], [Configuration])
    VALUES('scheduledreports', 'Scheduled Reports', 'Displays scheduled reports', 'Reporting', 'default', 'ScheduledReports', 'REPORTING_TRACKTRACE', 'REPORT_USER', 
	'Services/ScheduledReports',
    '{
        "remoteaddress": "http://reporting-services.apmoller.net/ws/reporting/ReportingWebServce?WSDL"	
    }',
    '{ 
        "targeturl": "http://reporting.damco.com/Reporting/page" 
    }');
END

IF NOT EXISTS(SELECT * FROM [dbo].[Widget]  WHERE [Uid] = 'recentsearches')
BEGIN
INSERT INTO [dbo].[Widget] ([Uid], [Title], [Description], [Category], [Icon], [Template], [UAMApplication], [UAMFunction], [ServiceURL], [ServiceConfiguration], [Configuration])
    VALUES('recentsearches', 'Recent Searches', 'Displays information of the latest searches from within the last 5 days', 'Track & Trace', 'recentsearches', 'RecentSearches', 'REPORTING_TRACKTRACE', '', 
	'Services/RecentSearches',
    '{
        "remoteaddress": "http://reporting-services.apmoller.net/ws/reporting/ReportingWebServce?WSDL"
    }', 
    '{ 
        "targeturl": "http://reporting.damco.com/Reporting/page" 
    }');
END

IF NOT EXISTS(SELECT * FROM [dbo].[Widget] WHERE [Uid] = 'staticcontent')
BEGIN
INSERT INTO [dbo].[Widget] ([Uid], [Title], [Description], [Category], [Icon], [Template], [Configuration], [UAMApplication], [UAMFunction], [ServiceConfiguration])
    VALUES('staticcontent', 'Welcome', '', '', '', 'StaticContent', '
    { "content": "<p>myDamco now has little windows of information, called <strong>widgets</strong>, that will make myDamco easier to use. You can add widgets from the <span style=\"color: #54ab42;\">Add New Widget</span> dialog, and you can reposition and remove the widgets as you please. Our news is now also a <strong>widget</strong>. We recommend you have it visible so that you are updated on news from us. The number of <strong>widgets</strong> will continue to grow, providing easy access to the different functions on myDamco and from external sources.</p><p>We hope you like the new functionality.</p> <p><span style=\"color: #398fd1;\">Damco International.</span></p>"}
    ', '', '', '{}');
END

IF NOT EXISTS(SELECT * FROM [dbo].[Widget] WHERE [Uid] = 'announcements')
BEGIN
INSERT INTO [dbo].[Widget] ([Uid], [Title], [Description], [Category], [Icon], [Template], [Configuration], [UAMApplication], [UAMFunction], [Disabled], [ServiceConfiguration], [ServiceURL])
	VALUES('announcements', 'Announcements', '', '', '', '', '{}', 'MYDAMCO', 'USE', 1, '{"itemLimit":1}', 'Services/DamcoNews');
END

-- Navigation

IF NOT EXISTS(SELECT * FROM [dbo].[Navigation] WHERE [Title] = 'Shipper')
BEGIN
INSERT INTO [dbo].[Navigation] ([Title], [UId], [Url], [Target], [IECompatibilityMode] ,[Priority], [UAMApplication], [UAMFunction], [NewTab])
    VALUES('Shipper', 'shipper', 'https://booking.damco.com/ShipperPortalWeb/index.jsp', 'Portal', 'None', 1, 'BOOKING', '', 0);
END

IF NOT EXISTS(SELECT * FROM [dbo].[Navigation] WHERE [Title] = 'Track & Trace')
BEGIN
INSERT INTO [dbo].[Navigation] ([Title], [UId], [Url], [Target], [IECompatibilityMode] ,[Priority], [UAMApplication], [UAMFunction], [NewTab])
    VALUES('Track & Trace', 'tracktrace', 'https://reporting.damco.com/Reporting/page', 'External', 'None', 1, 'REPORTING_TRACKTRACE', '', 0);
END

IF NOT EXISTS(SELECT * FROM [dbo].[Navigation] WHERE [Title] = 'Reporting')
BEGIN
INSERT INTO [dbo].[Navigation] ([Title], [UId], [Url], [Target], [IECompatibilityMode] ,[Priority], [UAMApplication], [UAMFunction], [NewTab])
    VALUES('Reporting', 'reporting', 'https://reporting.damco.com/Reporting/page?action=extract_center', 'External', 'None', 1, 'REPORTING', '', 0);
END


IF NOT EXISTS(SELECT * FROM [dbo].[Navigation] WHERE [Title] = 'Document Management')
BEGIN
INSERT INTO [dbo].[Navigation] ([Title], [UId], [Url], [Target], [IECompatibilityMode] ,[Priority], [UAMApplication], [UAMFunction], [NewTab])
    VALUES('Document Management', 'documentmanagement', 'https://documentation.damco.com/eDOCWeb/', 'Portal', 'None', 1, 'DOCUMENT_MANAGEMENT', '', 0);
END

IF NOT EXISTS(SELECT * FROM [dbo].[Navigation] WHERE [Title] = 'Communication & Exceptions')
BEGIN
INSERT INTO [dbo].[Navigation] ([Title], [UId], [Url], [Target], [IECompatibilityMode] ,[Priority], [UAMApplication], [UAMFunction], [NewTab])
    VALUES('Communication & Exceptions', 'communcationexception', 'https://exceptions.damco-usa.com/oct/login.aspx', 'Portal', 'None', 1, 'C_E', '', 0);
END

IF NOT EXISTS(SELECT * FROM [dbo].[Navigation] WHERE [Title] = 'Dynamic Flow Control')
BEGIN
INSERT INTO [dbo].[Navigation] ([Title], [UId], [Url], [Target], [IECompatibilityMode] ,[Priority], [UAMApplication], [UAMFunction], [NewTab])
    VALUES('Dynamic Flow Control', 'dynamicflowcontrol', 'https://dfc.damco.com/AMI/', 'External', 'None', 1, 'DYNAMICFLOWCONTROL', '', 0);
END

IF NOT EXISTS(SELECT * FROM [dbo].[Navigation] WHERE [Title] = 'Administration')
BEGIN
INSERT INTO [dbo].[Navigation] ([Title], [UId], [Url], [Target], [IECompatibilityMode] ,[Priority], [UAMApplication], [UAMFunction], [NewTab])
    VALUES('Administration', 'administration', '', 'Admin', 'None', 1, 'MYDAMCO', 'ADMINISTRATION', 0);
END

IF NOT EXISTS(SELECT * FROM [dbo].[Navigation] WHERE [Title] = 'News Administration')
BEGIN
INSERT INTO [dbo].[Navigation] ([Title], [UId], [Url], [Target], [IECompatibilityMode] ,[Priority], [UAMApplication], [UAMFunction], [NewTab])
    VALUES('News Administration', 'newsadministration', '', 'Admin', 'None', 1, 'MYDAMCO', 'EDITOR', 0);
END

/* IF NOT EXISTS(SELECT * FROM [dbo].[Navigation] WHERE [Title] = 'BI Dashboards')
BEGIN
	DECLARE @reportingPriority tinyint;
	SET @reportingPriority = (SELECT [Priority] FROM [dbo].[Navigation] WHERE [Title] = 'Reporting');
	UPDATE [dbo].[Navigation] SET [Priority] = [Priority] + 1 WHERE [Priority] > @reportingPriority;
	INSERT INTO [dbo].[Navigation] ([Title], [UId], [Url], [Target], [IECompatibilityMode] ,[Priority], [UAMApplication], [UAMFunction], [NewTab])
		VALUES('BI Dashboards', 'bidashborards', 'https://cdwh.crb.apmoller.net', 'External', 'None', @reportingPriority + 1, 'BIDashboard', '', 1);
END */

-- Add default news categories
IF NOT EXISTS(SELECT * FROM [dbo].[NewsCategory] WHERE [Name] = 'MyDamco Announcements')
BEGIN
INSERT INTO [dbo].[NewsCategory] ([Name], [Description], [Language], [showUAMApplication], [showUAMFunction], [editUAMApplication], [editUAMFunction], [Downtime], [Configuration])
    VALUES('MyDamco Announcements', 'Important announcements that are shown to all users of MyDamco', 'en', 'MYDAMCO', 'USE', 'MYDAMCO', 'ADMINISTRATION', 1, '{}');
END

IF NOT EXISTS(SELECT * FROM [dbo].[NewsCategory] WHERE [Name] = 'Damco General News')
BEGIN
INSERT INTO [dbo].[NewsCategory] ([Name], [Description], [Language], [showUAMApplication], [showUAMFunction], [editUAMApplication], [editUAMFunction], [Downtime], [Configuration])
    VALUES('Damco General News', 'General internal Damco news.', 'en', 'MYDAMCO', 'USE', 'MYDAMCO', 'EDITOR', 0,'
            {
                "externalfeeds": [
                    {
                        "url": "http://www.damco.com/en/about%20damco/press/press%20releases/rss",
                        "timeout": 10,
                        "itemLimit": 3
                    }
                ]
            }');
END

IF NOT EXISTS(SELECT * FROM [dbo].[Page] WHERE [UId] = 'cookies')
BEGIN
INSERT INTO [dbo].[Page] ([UId], [Title], [Body])
    VALUES('cookies', 'Cookies', '<p><strong>This site contains tracking cookies</strong>. They are used to generate general visitor statistics on myDamco. For information on how to block or allow cookies in your browser, please see guidelines specifically for your browser. For example:&nbsp;Internet Explorer, please see <a href="http://windows.microsoft.com/en-us/windows-vista/block-or-allow-cookies">http://windows.microsoft.com/en-us/windows-vista/block-or-allow-cookies</a>.</p>');
END

IF NOT EXISTS(SELECT * FROM [dbo].[Page] WHERE [UId] = 'privacypolicy')
BEGIN
INSERT INTO [dbo].[Page] ([UId], [Title], [Body])
    VALUES('privacypolicy', 'Privacy Policy', '<b>Protecting your information</b>

<p>Damco recognises the importance of protecting the privacy of your personal information.</p>

<p>We provide this privacy policy to make you, as visitor to our website, aware of our practices in relation to the handling of your personal information. This privacy policy is available for viewing on every page of our website where we collect personal information.</p>

<b>Collection of personal information</b>
<p>We collect personal information to improve the quality of our website, and to communicate with you.</p>

<p>Our practice of collecting information to improve our website includes visitor profiling where, through the secured services of a supplier, we monitor the number of visitors to the website, the profiles of each user (including country, internet provider, browser, operating system, screen resolution) and how each user moves through the site (the click paths).</p>

<p>The data we collect for the purpose of communicating with you includes information provided by you when you make inquiries – such as requests for general information about Damco''s services or specific quotes or bookings. The personal information we collect may include your title, name, job description, full address and contact information (phone number, fax number and e-mail address).</p>

<b>Use of your personal information</b>
<p>Damco will use your personal information to communicate with you. This might be to respond to your query, complete your booking, or, where appropriate, to contact you in future.</p>

<p>will not, without your permission, use any of the personal information you provide through our website in ways other than described above. Damco will not, without your permission, share you personal information with third parties in ways other than described above.</p>

<b>Update or removal of personal information</b>
<p>may request to update, change, or remove information registered at www.damco.com by sending an email to cendammkt@damco.com.</p> 

<b>Hyperlinks to external websites</b>
<p>The Damco website may include hyperlinks to external websites. These are provided solely for your convenience. If you click one of the hyperlinks, you will leave the Damco website and go to an external website. Damco has not reviewed external websites and we do not control and are not responsible for any of the websites, their contents or the owner’s Privacy Policy. If you choose to access any external websites linked to the Damco website, you do so entirely at your own risk.</p>

<b>Contact</b>
<p>If you have any questions or comments about our Privacy Policy you can contact:<br /></p>
<p>
Damco International A/S<br />
Dampfaergevej 21,<br />
DK-2100 Copenhagen OE<br />
Denmark<br />
</p>
');
END

IF NOT EXISTS(SELECT * FROM [dbo].[Page] WHERE [UId] = 'termsofuse')
BEGIN
INSERT INTO [dbo].[Page] ([UId], [Title], [Body])
    VALUES('termsofuse', 'Terms of Use', '<p><b>1. Use of Damco website</b></p>

<p>1.1. These terms apply to all parts of the Damco website at http://www.damco.com. The website is made available by Damco International A/S and its authorised agents (in these terms, together “Damco”).</p>

<p>1.2. Any information, data, text, images, video or audio, or any other materials available from Damco via or generated by or posted to the website, or other services or facilities via the website, may only be accessed, browsed, downloaded, requested or received by any user subject to: (1) the terms and conditions set out below; and (2) any additional instructions, terms or conditions which appear on the website from time to time and apply to particular content or services which the user uses (together referred to as the “Terms”).  Access to or use of the website or the content or services by the user shall be deemed to constitute acceptance of and agreement to these terms by the user.</p>

<p>1.3. If the user accesses the website or receives or uses content or services on behalf of any other party (including any body corporate), that party shall also be bound by these Terms as if it were the User.  The User warrants and represents that he/she is authorised by any such party to bind that party to these terms. </p>

<p><b>2. Status of Terms</b></p>

<p>2.1. Other than as set out in clauses 2.2 to 2.4 below, these terms shall not affect or form part of the terms of any quotation or contract for freight forwarding services with Damco. “Transport Services” includes, without limitation: ocean freight and airfreight, as well as other land-based cargo handling such as trucking, customs house brokerage, and freight or transportation management services.</p>

<p>2.2. Damco transport services are provided by Damco and/or its authorised agents, and not by any other party.</p>

<p>2.3. Damco shall use reasonable endeavours to respond to any query or request for quotation received via the website, but shall not be responsible for any delay in so responding.</p>

<p>2.4. Any quotation or contract for transport services is provided in accordance with and subject to Damco’s published tariff and bill of lading terms and conditions, as may be updated from time to time by Damco. Copies of the bill of lading terms and conditions are available on the website.</p>

<p><b>3. Intellectual Property Rights and Other Rights</b></p>

<p>3.1. Ownership of all copyrights, database rights, patents, trade or service marks, product names or design rights (whether registered or unregistered), trade secrets and confidential information and any similar rights existing in any territory now or in the future (“Intellectual Property Rights”) and similar rights and interests in all domain names, trade marks, logos or branding appearing on the website or Content, or otherwise relating to the structure of the website and any Services offered by Damco via the website, shall vest in Damco or its licensors.</p>

<p>3.2. The User may use the website and the Content and Services available via the website only for the purposes reasonably anticipated by the website, or otherwise as might reasonably be expected in the course of any relationship with Damco, and in accordance with any requirements from time to time on the website. The User may not access restricted areas of the website unless the User has obtained appropriate authorisation and a password (or other access device) from Damco. The User may not: (1) use or permit any other party to use all or any part of the website, Content or Services in connection with activities that breach any relevant laws, infringe any third party’s rights, or breach any applicable standards, content requirements or codes; (2) post to or transmit through the website any information, materials or content that might be or might encourage conduct that might be unlawful, threatening, abusive, defamatory, obscene, vulgar, pornographic, profane or indecent; or (3) use the website for the purpose of or as a means to send ‘flame’ or ‘spam’ emails.</p>

<p>3.3. The User shall procure the waiver of any moral rights in any information, data or other content or materials posted or introduced by the User to the website (“User Materials”). The User irrevocably authorises Damco and its licensees to use any User Materials for all reasonable business purposes, including without limitation: copying, amending, incorporating into other materials, publishing or otherwise providing to third parties (and permitting such third parties to use and sublicense the User Materials) anywhere in the world. The User agrees to take any required steps (including completing documentation) in any jurisdiction to give effect to this clause.</p>

<p>3.4. Damco does not warrant or represent that the User’s or any other party’s use of the Content or the Services available via the website will not infringe the rights of third parties.</p>

<p><b>4. Errors</b></p>

<p>4.1. Damco will use all reasonable endeavours to ensure that the Content accurately reflects either: (1) relevant records held on Damco’s computer systems; or (2) information received from a party other than Damco, subject to clause 4.2 below.</p>

<p>4.2. The Content is provided for informative purposes only, and Damco makes no warranties, representations, guarantees or undertakings that any part of it is accurate, sufficient, error-free, complete or up-to-date at the time it is accessed. The User should make his own enquiries to satisfy himself of the accuracy and completeness of any Content before relying on it.</p>

<p>4.3. Except as set out in these Terms, Damco shall have no liability for any breach of an implied warranty or term, including (without limitation) any breaches in relation to the operation, quality or fitness for purpose of the website or any Content or Service.</p>

<p>4.4. The User is responsible for the accuracy and completeness of any User Materials. The User shall ensure that User Materials do not infringe any intellectual property or other rights of any third party and are not defamatory, unlawful, immoral or otherwise likely to breach or infringe any right or requirement or to give rise to any claim for loss or damage by any third party. The User shall indemnify Damco against any claims, losses, actions, proceedings, damages or other liabilities suffered by Damco as a result of any actual or potential breach by the User of its obligations.</p>

<p><b>5. Hyperlinked websites</b></p>

<p>5.1. The website may contain links or references to websites operated by third parties.  Damco makes no warranties or representations whatsoever regarding any third party websites.  All third party websites are wholly separate and independent from this website and Damco does not have any control over the content or operation of such websites. Damco does not endorse third party websites, and does not accept any responsibility for the existence, operation, content, or use of such third party websites.</p>

<p>5.2. A User may hyperlink to any unrestricted area of the website provided that the User complies with the following terms or any other terms posted on the website from time to time.</p>

<p>5.3. The User may not: (1) replicate in any way any Content appearing on the website unless with Damco’s prior written agreement; (2) create a border environment or browser around or otherwise frame any Content or create any impression that the Content is supplied or owned by any other party than Damco; (3) present misleading or false information about Damco, its services or Content; (4) misrepresent Damco’s relationship with the linking User (or any third party); (5) create any implication or inference that Damco endorses the linking User or its services (or any third party or its services); (6) use or reproduce Damco’s logo, trademarks or name; (7) contain or display any content that could be construed as obscene, libelous, defamatory, distasteful, offensive, pornographic, or inappropriate in any other way; (8) contain materials, content or anything else that might violate any laws of any jurisdiction.</p>

<p>5.4. The User must clearly indicate that the Damco website is operated by Damco and is not controlled by or otherwise associated or connected with the linked website, and that Damco’s terms and conditions apply in relation to any use of the Damco website.</p>

<p>5.5. On request, any User must immediately remove any link placed on or linking to any area of the website. The User shall not permit any third party to access or retrieve information from the website on the User’s behalf.</p>

<p>5.6. The User may not run software programs, scripts, macros or any other similar materials against or in relation to any part of the website. The User may not take any action to endanger, compromise or hamper the stability and operation of the website or infringe rights in or relating to the website or any materials appearing on it.</p>

<p><b>6. Security</b></p>

<p>6.1. The User agrees to comply with any reasonable instructions Damco may issue from time to time regarding security of the website.</p>

<p>6.2. The User must ensure that he does not compromise the security of the website or systems or the security of Damco or any other users of the website or any customers of Damco and its affiliates, associates or agents in any way.</p>

<p>6.3. Both the User and Damco shall each take all reasonable precautions to ensure that communications are not affected by computer viruses or other destructive or disruptive components, and to ensure no such components are transmitted to Damco, the website or the User, or via the website.</p>

<p><b>7. Liability</b></p>

<p>7.1. Damco, its affiliates, associates and agents shall have no liability whatsoever in respect of any use of the Content, Services or website, howsoever arising, for:</p>

<p>7.1.1. economic loss in any form, such as indirect or consequential loss or damage, loss of profit or loss of customers;</p>
<p>7.1.2. special or punitive damages;</p>
<p>7.1.3. business interruption, loss of data or delay; or</p>
<p>7.1.4. any decision made, or action taken, in reliance on any information or other repesentation made or published in relation to any Content, Services or the website.</p>

<p>7.2. The total liability of Damco, its affiliates, associates and agents to the User and any person acting on the User’s behalf, howsoever arising out of or in connection with these Terms and/or the website, Services or Content (including in relation to negligence) shall, in aggregate, in respect of any claim, or series of connected claims arising out of the same cause in any calendar year, not exceed USD 500 (United States Dollars Five Hundred).</p>

<p>7.3. The User shall not bring and shall ensure that no claims are brought against Damco, its affiliates, associates and agents, for amounts exceeding the aggregate limit of liability set out in clause 7.2.</p>

<p>7.4. The User is advised to obtain where appropriate, insurance cover at his own cost, to prevent any losses exceeding the limit set out in clause 7.2 above.</p>

<p>7.5. Nothing in these Terms shall exclude liability for death, personal injury or fraud.</p>

<p>7.6. Except as set out in these Terms, Damco and its affiliates, associates and agents shall have no liability whatsoever in respect of the use of the Content, Services or website, howsoever arising.</p>

<p><b>8. Miscellaneous</b></p>

<p>8.1. Use of the website or of the Content or Services may be subject to certain legal or regulatory requirements in particular jurisdictions. The User may only access or use the website, Content or Services to the extent such access or use is permitted by relevant laws or regulations in the User’s jurisdiction.</p>

<p>8.2. Damco and its affiliates, associates and agents will not be liable for any loss, damage, delay or failure in relation to the website, Content or any Services, caused in whole or in part by any equipment, system or network failure, interruption or unavailability of the Internet, interruption to power supplies, the action of any government or governmental agency, natural occurrence, law or regulation, industrial action, war, terrorist action, or anything else beyond its reasonable control.</p>

<p>8.3. These Terms supersede all previous agreements, communications, representations and discussions between the parties relating to the website, Content or Services.</p>

<p>8.4. Any purported modification or waiver of these Terms shall not bind Damco unless it is in writing and signed by an authorised representative of Damco.</p>

<p>8.5. References in these Terms to ‘in writing’ or ‘written’ include communication by email or other electronic form.</p>

<p>8.6. Each of the provisions of these Terms is severable from the others and if one or more of them is void, illegal or unenforceable, the remainder shall not be affected in any way.</p>

<p>8.7. The rights of Damco under these Terms may be exercised as often as necessary and are cumulative and not exclusive of its rights under any applicable law. Any delay in the exercise or non-exercise of any such right is not a waiver of that right.</p>

<p>8.8. The User may not sell, assign, or otherwise transfer or part with any right or benefit conferred by any provision of these Terms without Damco’s prior written consent.</p>

<p>8.9. Damco may change or remove any part of the website, Content or Services or change these Terms at any time without prior notice. The User agrees to periodically review the website and these Terms so that he is aware of any changes.</p>

<p>8.10. Damco may assist or co-operate with authorities in any jurisdiction in relation to any direction or request to disclose personal or other information regarding any User or the use of the website, Content or Services.</p>

<p>8.11. Damco and their respective agents, associates and affiliates (“Relevant Parties”) shall have the benefit of all provisions in these Terms expressed to be for the benefit of “Damco” as well as the law and jurisdiction clause</p>

<p>8.12. In entering into this agreement, A.P. Møller - Maersk A/S does so (to the extent of such provisions) not only on its own behalf but also as agent and trustee for such persons.</p>

<p>8.13. To the extent that clause 8.11 is not effective to give such benefit to any Relevant Parties, A.P. Møller - Maersk A/S may enforce such provisions in its own name pursuant to the Contracts (Rights of Third Parties) Act 1999. These Terms may be varied or rescinded by agreement or in accordance with its relevant provisions, without the consent of any Relevant Third Party.</p>

<p>8.14. These Terms shall be subject to English law and any dispute, claim, matter of construction or interpretation arising out of or relating to the website, including these Terms, shall be subject to the exclusive jurisdiction of the English courts.</p>
');
END

IF NOT EXISTS(SELECT * FROM [dbo].[Page] WHERE [UId] = 'disclaimer')
BEGIN
INSERT INTO [dbo].[Page] ([UId], [Title], [Body])
    VALUES('disclaimer', 'Disclaimer', ' <p>We have prepared all content, information, or other material on this website solely for the purpose of providing information about
     Damco, our subsidiaries, and partners to interested parties.</p>
    
    <p>We have compiled the information in good faith and we provide this without any warranty of any kind, either express or implied, 
    including but not limited to: any implied warranties or implied terms of merchantability, fitness for a particular purpose, or non-infringement.</p>
    
    <p>We reserve the right to change the information on this website without notice. </p>
    
    <p>We have taken reasonable care to ensure that the information on this website is accurate and up-to-date. But, before you rely on any 
    information, we ask you to verify its accuracy by calling our head office.</p>
    
    <p>Your use of this website is at your own risk. By using this website you agree not to hold Damco or any parties involved in creating, 
    producing, or delivering the website liable for any direct, indirect, incidental, consequential, or punitive damages or losses, costs 
    or expenses of any kind, including viruses, which may arise out of access to or use of this website or the Information, regardless of its accuracy or completeness.</p>
    
    <p>All intellectual property rights are the property of, and remain the property of Damco.</p>
    
    <p>You may download information from this site for your personal use only and you must seek written consent from Damco before you use 
    any of the information.</p>
    
    <p>The website is construed according to the laws of Denmark. Damco does not make any representation that the information on this website 
    complies with any laws or regulations outside of Denmark.</p>');
END


/* ------------------------------------------------------------------------ 
      ELMAH - Error Logging Modules and Handlers for ASP.NET 
   ------------------------------------------------------------------------ */
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ELMAH_Error' AND xtype='U')
BEGIN
CREATE TABLE [dbo].[ELMAH_Error]
(
    [ErrorId]     UNIQUEIDENTIFIER NOT NULL,
    [Application] NVARCHAR(60)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Host]        NVARCHAR(50)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Type]        NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Source]      NVARCHAR(60)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Message]     NVARCHAR(500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [User]        NVARCHAR(50)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [StatusCode]  INT NOT NULL,
    [TimeUtc]     DATETIME NOT NULL,
    [Sequence]    INT IDENTITY (1, 1) NOT NULL,
    [AllXml]      NTEXT COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) 
ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
ALTER TABLE [dbo].[ELMAH_Error] WITH NOCHECK ADD CONSTRAINT [PK_ELMAH_Error] PRIMARY KEY NONCLUSTERED ([ErrorId]) ON [PRIMARY];
ALTER TABLE [dbo].[ELMAH_Error] ADD CONSTRAINT [DF_ELMAH_Error_ErrorId] DEFAULT (NEWID()) FOR [ErrorId];
CREATE NONCLUSTERED INDEX [IX_ELMAH_Error_App_Time_Seq] ON [dbo].[ELMAH_Error] ( [Application] ASC, [TimeUtc] DESC, [Sequence] DESC ) ON [PRIMARY];

/* ------------------------------------------------------------------------ 
        STORED PROCEDURES                                                      
   ------------------------------------------------------------------------ */
exec('CREATE PROCEDURE [dbo].[ELMAH_GetErrorXml] (@Application NVARCHAR(60), @ErrorId UNIQUEIDENTIFIER)
AS

    SET NOCOUNT ON

    SELECT 
        [AllXml]
    FROM 
        [ELMAH_Error]
    WHERE
        [ErrorId] = @ErrorId
    AND
        [Application] = @Application');

exec('CREATE PROCEDURE [dbo].[ELMAH_GetErrorsXml]
(
    @Application NVARCHAR(60),
    @PageIndex INT = 0,
    @PageSize INT = 15,
    @TotalCount INT OUTPUT
)
AS 

    SET NOCOUNT ON

    DECLARE @FirstTimeUTC DATETIME
    DECLARE @FirstSequence INT
    DECLARE @StartRow INT
    DECLARE @StartRowIndex INT

    SELECT 
        @TotalCount = COUNT(1) 
    FROM 
        [ELMAH_Error]
    WHERE 
        [Application] = @Application

    -- Get the ID of the first error for the requested page

    SET @StartRowIndex = @PageIndex * @PageSize + 1

    IF @StartRowIndex <= @TotalCount
    BEGIN

        SET ROWCOUNT @StartRowIndex

        SELECT  
            @FirstTimeUTC = [TimeUtc],
            @FirstSequence = [Sequence]
        FROM 
            [ELMAH_Error]
        WHERE   
            [Application] = @Application
        ORDER BY 
            [TimeUtc] DESC, 
            [Sequence] DESC

    END
    ELSE
    BEGIN

        SET @PageSize = 0

    END

    -- Now set the row count to the requested page size and get
    -- all records below it for the pertaining application.

    SET ROWCOUNT @PageSize

    SELECT 
        errorId     = [ErrorId], 
        application = [Application],
        host        = [Host], 
        type        = [Type],
        source      = [Source],
        message     = [Message],
        [user]      = [User],
        statusCode  = [StatusCode], 
        time        = CONVERT(VARCHAR(50), [TimeUtc], 126) + ''Z''
    FROM 
        [ELMAH_Error] error
    WHERE
        [Application] = @Application
    AND
        [TimeUtc] <= @FirstTimeUTC
    AND 
        [Sequence] <= @FirstSequence
    ORDER BY
        [TimeUtc] DESC, 
        [Sequence] DESC
    FOR
        XML AUTO
');

exec('CREATE PROCEDURE [dbo].[ELMAH_LogError]
(
    @ErrorId UNIQUEIDENTIFIER,
    @Application NVARCHAR(60),
    @Host NVARCHAR(30),
    @Type NVARCHAR(100),
    @Source NVARCHAR(60),
    @Message NVARCHAR(500),
    @User NVARCHAR(50),
    @AllXml NTEXT,
    @StatusCode INT,
    @TimeUtc DATETIME
)
AS

    SET NOCOUNT ON

    INSERT
    INTO
        [ELMAH_Error]
        (
            [ErrorId],
            [Application],
            [Host],
            [Type],
            [Source],
            [Message],
            [User],
            [AllXml],
            [StatusCode],
            [TimeUtc]
        )
    VALUES
        (
            @ErrorId,
            @Application,
            @Host,
            @Type,
            @Source,
            @Message,
            @User,
            @AllXml,
            @StatusCode,
            @TimeUtc
        )

');
END


/* ------------------------------------------------------------------------ 
        MODIFICATIONS                                          
   ------------------------------------------------------------------------ */

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[NewsItem]') AND name = N'IX_NewsItem_From')
    CREATE INDEX [IX_NewsItem_From] ON [dbo].[NewsItem] ([From]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[NewsItem]') AND name = N'IX_NewsItem_To')
    CREATE INDEX [IX_NewsItem_To] ON [dbo].[NewsItem] ([To]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[NewsItem]') AND name = N'IX_NewsItem_NewsCategory_Id')
    CREATE INDEX [IX_NewsItem_NewsCategory_Id] ON [dbo].[NewsItem] ([NewsCategory_Id]);


/* ------------------------------------------------------------------------ 
        MIGRATION                                          
   ------------------------------------------------------------------------ */

/* if the WidgetInstanceHistory table is empty, insert rows from WidgetInstance into it (This if statement is only for performance, the below insert can run repeatedly) */
IF NOT EXISTS (select 1 from WidgetInstanceHistory) 
BEGIN 
    /* Insert rows from WidgetInstance into WidgetInstanceHistory, if they do not already exist in WidgetInstanceHistory */
	INSERT INTO WidgetInstanceHistory (WidgetInstance_Id, [Login], [Role], AddTime, DeleteTime, Widget_Id) 
	SELECT Id, [Login], [Role], GETUTCDATE(), NULL, Widget_Id
	FROM WidgetInstance
	WHERE Id NOT IN (
		SELECT WidgetInstance_Id
		FROM WidgetInstanceHistory
	);
END

/* UPDATE [dbo].[Widget] SET [ServiceConfiguration] = 
    '{
        "folders": [
            { "application": "Reporting", "folder": "\\\\10.255.220.19\\Production\\Elearning\\REPORTING", "UAMApplication": "REPORTING", "width": 1031, "height": 804 },
            { "application": "Booking", "folder": "\\\\10.255.220.19\\Production\\Elearning\\BOOKING", "UAMApplication": "BOOKING", "width": 1031, "height": 804 },
			{ "application": "Shipper", "folder": "\\\\10.255.220.19\\Production\\Elearning\\SHIPPING", "include": "Damco Shipper Online Course.*", "UAMApplication": "BOOKING", "width": 1031,"height": 804 },
			{ "application": "Document Management", "id": 1, "folder": "\\\\10.255.220.19\\Production\\Elearning\\DocMan\\Client", "include": "Document_Management_For*.*", "roles": [ { "orgName": "DocMan_eLearning", "roleName": "Client" } ], "width": 1031,"height": 804 },
			{ "application": "Document Management", "id": 2, "folder": "\\\\10.255.220.19\\Production\\Elearning\\DocMan\\Broker", "include": "Document_Management_For*.*", "roles": [ { "orgName": "DocMan_eLearning", "roleName": "DocMan_Broker" } ], "width": 1031,"height": 804 },
			{ "application": "Document Management", "id": 3, "folder": "\\\\10.255.220.19\\Production\\Elearning\\DocMan\\SDS", "include": "Document_Management_For*.*", "roles": [ { "orgName": "DocMan_eLearning", "roleName": "SDS_DocMan" } ], "width": 1031,"height": 804 }
         ]
    }'
WHERE [Uid] = 'elearning'; */

/* UPDATE [dbo].[Widget] SET [ServiceConfiguration] = 
    '{
        "folders": [
            { "application": "Reporting", "folder": "C:\\elearning\\REPORTING", "UAMApplication": "REPORTING", "width": 1031, "height": 804 },
            { "application": "Booking", "folder": "C:\\elearning\\BOOKING", "UAMApplication": "BOOKING", "width": 1031, "height": 804 },
			{ "application": "Shipper", "folder": "C:\\elearning\\SHIPPING", "include": "Damco Shipper Online Course.*", "UAMApplication": "BOOKING", "width": 1030,"height": 800, "resize": "clip" },
			{ "application": "Document Management", "id": 1, "folder": "C:\\elearning\\DocMan\\Client", "include": "Document_Management_For*.*", "UAMApplication": "DOCUMENT_MANAGEMENT", "UAMFunction": "CLIENT", "width": 1024,"height": 768, "resize": "clip" },
			{ "application": "Document Management", "id": 2, "folder": "C:\\elearning\\DocMan\\Broker", "include": "Document_Management_For*.*", "UAMApplication": "DOCUMENT_MANAGEMENT", "UAMFunction": "BROKER", "width": 1024,"height": 768, "resize": "clip" },
			{ "application": "Document Management", "id": 3, "folder": "C:\\elearning\\DocMan\\SDS", "include": "Document_Management_For*.*", "UAMApplication": "DOCUMENT_MANAGEMENT", "UAMFunction": "RELEASE_POUCH", "width": 1024,"height": 768, "resize": "clip" }
         ]
    }'
WHERE [Uid] = 'elearning';

UPDATE [dbo].[Widget] SET [Configuration] = 
    '{
        "targeturl": "Services/ELearningLoadFile"
    }'
WHERE [Uid] = 'elearning'; */
