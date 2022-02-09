IF DB_NAME() = 'Master' 
BEGIN
	RaisError ('The current database is master', 20, -1) with log
	set noexec on
END

SET ANSI_NULLS ON
GO

UPDATE [dbo].[Navigation] SET Url = 'http://localhost:2930/debug/debug/reporting/page.xml' WHERE [Uid] = 'reporting';

UPDATE [dbo].[Widget] SET [Configuration] = 
    '{"ReportingChartDataServiceUrl": "/mockservices/Performance/?action=ChartData&returnData={3}&definitionId={0}&filterString={1}&groupByColumn={2}",
        "ReportingColumnsServiceUrl": "/mockservices/Performance/?action=ColumnDefinitions&returnData={1}&definitionId={0}",
        "ReportingConfigurationServiceUrl": "/mockservices/Performance/?action=DefinitionList",
        "ReportingDetailsUrlTemplate": "?action=ds_extract_view&definitionId={0}&filterString={1}&groupByColumn={2}&returnData=",
		"targeturl": "http://powua03a.apmoller.net:10120/Reporting/page"
    }'
WHERE [Uid] = 'performancechart';

UPDATE [dbo].[Widget] SET [ServiceConfiguration] = 
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

UPDATE [dbo].[Widget] SET [ServiceConfiguration] = 
    '{ 
        "remoteaddress": "http://localhost:2930/MockServices/DocumentManagement/RecentPouchWidgetWS.asmx"
    }'
WHERE [Uid] = 'recentpouches';

UPDATE [dbo].[Widget] SET [ServiceConfiguration] = 
    '{
        "remoteaddress": "http://localhost:2930/MockServices/Reporting/ReportingWebServices.asmx"
    }'
WHERE [Uid] = 'recentreports';

UPDATE [dbo].[Widget] SET [ServiceConfiguration] = 
    '{
        "remoteaddress": "http://localhost:2930/MockServices/Reporting/ReportingWebServices.asmx"
    }'
WHERE [Uid] = 'scheduledreports';

UPDATE [dbo].[Widget] SET [ServiceConfiguration] = 
    '{
        "remoteaddress": "http://localhost:2930/MockServices/Reporting/ReportingWebServices.asmx"
    }'
WHERE [Uid] = 'recentsearches';

-- Test data
INSERT INTO [dbo].[WidgetInstance] ([Title], [Login], [Role], [DashboardColumn], [DashboardPriority], [Widget_Id]) VALUES('MyDashBoard Widget','tlb013', 25, 0, 0, 1);

INSERT INTO [dbo].[WidgetInstance] ([Login], [Role], [DashboardColumn], [DashboardPriority], [Widget_Id]) VALUES('tlb013', 25, 0, 0, 10);
INSERT INTO [dbo].[WidgetInstance] ([Title], [Login], [Role], [DashboardColumn], [DashboardPriority], [Widget_Id], [Configuration]) VALUES('News Supply Chain', 'tlb013', 25, 0, 1, 1, '{ "selectedfeed": "nsc" }');
INSERT INTO [dbo].[WidgetInstance] ([Login], [Role], [DashboardColumn], [DashboardPriority], [Widget_Id]) VALUES('tlb013', 25, 0, 2, 2);
INSERT INTO [dbo].[WidgetInstance] ([Login], [Role], [DashboardColumn], [DashboardPriority], [Widget_Id]) VALUES('tlb013', 25, 0, 3, 3);

INSERT INTO [dbo].[WidgetInstance] ([Login], [Role], [DashboardColumn], [DashboardPriority], [Widget_Id]) VALUES('tlb013', 25, 1, 0, 4);
INSERT INTO [dbo].[WidgetInstance] ([Title], [Login], [Role], [DashboardColumn], [DashboardPriority], [Widget_Id], [Configuration]) VALUES('General Damco News', 'tlb013', 10, 1, 1, 5, '{ "selectedfeed": "2" }');
INSERT INTO [dbo].[WidgetInstance] ([Login], [Role], [DashboardColumn], [DashboardPriority], [Widget_Id]) VALUES('tlb013', 25, 1, 2, 6);

INSERT INTO [dbo].[WidgetInstance] ([Login], [Role], [DashboardColumn], [DashboardPriority], [Widget_Id]) VALUES('tlb013', 25, 2, 0, 7);
INSERT INTO [dbo].[WidgetInstance] ([Login], [Role], [DashboardColumn], [DashboardPriority], [Widget_Id]) VALUES('tlb013', 25, 2, 1, 8);
INSERT INTO [dbo].[WidgetInstance] ([Login], [Role], [DashboardColumn], [DashboardPriority], [Widget_Id]) VALUES('tlb013', 25, 2, 2, 9);
    
INSERT INTO [dbo].[NewsItem] ([Title], [Description], [Body], [From], [NewsCategory_Id])
    VALUES('Test Announcement', 'This is a test announcement, if you can see it you probably hate its ugly red/bold style ;)', 'This is the actual content of the test announcement', '2004-05-23T14:25:10', 1); 
    
INSERT INTO [dbo].[NewsItem] ([Title], [Description], [Body], [From], [NewsCategory_Id])
    VALUES('Test Damco News', 'This is a brief description of the test news.', 'This is the actual content of the test news', '2004-05-23T14:25:10',  2); 

INSERT INTO [dbo].[NewsItem] ([Title], [Description], [Body], [From], [NewsCategory_Id])
    VALUES('Test Damco News 2', 'This is a brief description of the second test news.', 'This is the actual content of the second test news', '2004-05-23T14:25:10', 2);




