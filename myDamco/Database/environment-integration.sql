IF DB_NAME() = 'Master' 
BEGIN
    RaisError ('The current database is master', 20, -1) with log
    set noexec on
END

SET ANSI_NULLS ON
GO

/* UPDATE [dbo].[NewsCategory] SET [Configuration] = '
            {
                "externalfeeds": [
                    {
                        "url": "http://www.damco.com/en/about%20damco/press/press%20releases/rss",
                        "timeout": 10,
                        "itemLimit": 3
                    }
                ]
            }'
WHERE [Name] = 'Damco General News'; */

UPDATE [dbo].[Widget] SET [Configuration] = 
    '{"ReportingChartDataServiceUrl": "https://dev-reporting-damco.apmoller.net:10121/Reporting/jsonp?action=ChartData&returnData={3}&definitionId={0}&filterString={1}&groupByColumn={2}",
        "ReportingColumnsServiceUrl": "https://dev-reporting-damco.apmoller.net:10121/Reporting/jsonp?action=ColumnDefinitions&returnData={1}&definitionId={0}",
        "ReportingConfigurationServiceUrl": "https://dev-reporting-damco.apmoller.net:10121/Reporting/jsonp?action=DefinitionList",
        "ReportingDetailsUrlTemplate": "?action=ds_extract_view&definitionId={0}&filterString={1}&groupByColumn={2}&returnData=",
		"targeturl": "https://dev-reporting-damco.apmoller.net:10121/Reporting/page"
    }'
WHERE [Uid] = 'performancechart';

-- Elearning
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

-- Recent Pouches
UPDATE [dbo].[Widget] SET
    [ServiceConfiguration] = 
        '{ 
            "remoteaddress": "http://powta02a.apmoller.net:10040/RecentPouchWidgetWS/RecentPouchWidgetWS"
        }'
    ,[Configuration] = 
        '{
			"targeturl": "https://test-documentation.damco.com/eDOCWeb/ExecuteWidgetSearch.do"
        }'
WHERE [Uid] = 'recentpouches';

-- Recent Reports
UPDATE [dbo].[Widget] SET 
    [ServiceConfiguration] = 
        '{
            "remoteaddress": "http://dev-reporting-damco.apmoller.net:10110/ws/reporting/ReportingWebServce?WSDL"
        }'
    ,[Configuration] = 
        '{
            "targeturl": "https://dev-reporting-damco.apmoller.net:10121/Reporting/page"
        }'
WHERE [Uid] = 'recentreports';

-- Scheduled Reports
UPDATE [dbo].[Widget] SET 
    [ServiceConfiguration] = 
        '{
            "remoteaddress": "http://dev-reporting-damco.apmoller.net:10110/ws/reporting/ReportingWebServce?WSDL"
        }'
    ,[Configuration] = 
        '{
            "targeturl": "https://dev-reporting-damco.apmoller.net:10121/Reporting/page"
        }'
WHERE [Uid] = 'scheduledreports';

-- Recent Searches
UPDATE [dbo].[Widget] SET 
    [ServiceConfiguration] = 
        '{
            "remoteaddress": "http://dev-reporting-damco.apmoller.net:10110/ws/reporting/ReportingWebServce?WSDL"
        }'
    ,[Configuration] = 
        '{
            "targeturl": "https://dev-reporting-damco.apmoller.net:10121/Reporting/page"
        }'
WHERE [Uid] = 'recentsearches';

-- Menus
UPDATE [dbo].[Navigation] SET [Url] = 'https://dev-reporting-damco.apmoller.net:10121/Reporting/page?action=extract_center' WHERE [UId] = 'reporting';
UPDATE [dbo].[Navigation] SET [Url] = 'https://demo-booking.damco.com/ShipperPortalWeb/index.jsp' WHERE [UId] = 'shipper';
UPDATE [dbo].[Navigation] SET [Url] = 'https://dev-reporting-damco.apmoller.net:10121/Reporting/page' WHERE [UId] = 'tracktrace';
UPDATE [dbo].[Navigation] SET [Url] = 'https://test-documentation.damco.com/eDOCWeb/' WHERE [UId] = 'documentmanagement';
UPDATE [dbo].[Navigation] SET [Url] = 'https://test-exceptions.damco-usa.com/oct/login.aspx' WHERE [UId] = 'communcationexception';
UPDATE [dbo].[Navigation] SET [Url] = 'https://dev-reporting-damco.apmoller.net:10121/AMI' WHERE [UId] = 'dynamicflowcontrol';

/* IF NOT EXISTS(SELECT * FROM [dbo].[Navigation] WHERE [Title] = 'BI Dashboards')
BEGIN
	DECLARE @reportingPriority tinyint;
	SET @reportingPriority = (SELECT [Priority] FROM [dbo].[Navigation] WHERE [Title] = 'Reporting');
	UPDATE [dbo].[Navigation] SET [Priority] = [Priority] + 1 WHERE [Priority] > @reportingPriority;
	INSERT INTO [dbo].[Navigation] ([Title], [UId], [Url], [Target], [IECompatibilityMode] ,[Priority], [UAMApplication], [UAMFunction], [NewTab])
		VALUES('BI Dashboards', 'bidashborards', 'https://cdwh.crb.apmoller.net', 'External', 'None', @reportingPriority + 1, 'BIDashboard', '', 1);
END */