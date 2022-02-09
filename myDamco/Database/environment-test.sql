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
        "remoteaddress": "http://localhost/MockServices/DocumentManagement/RecentPouchWidgetWS.asmx"
    }'
WHERE [Uid] = 'recentpouches';

UPDATE [dbo].[Widget] SET [ServiceConfiguration] = 
    '{
        "remoteaddress": "http://localhost/MockServices/Reporting/ReportingWebServices.asmx"
    }'
WHERE [Uid] = 'recentreports';

UPDATE [dbo].[Widget] SET [ServiceConfiguration] = 
    '{
        "remoteaddress": "http://localhost/MockServices/Reporting/ReportingWebServices.asmx"
    }'
WHERE [Uid] = 'scheduledreports';

UPDATE [dbo].[Widget] SET [ServiceConfiguration] = 
    '{
        "remoteaddress": "http://localhost/MockServices/Reporting/ReportingWebServices.asmx"
    }'
WHERE [Uid] = 'recentsearches';


-- Add some production widgets
/* DELETE FROM [dbo].[Widget] WHERE [Uid] = 'elearning-prod'; */
IF NOT EXISTS(SELECT * FROM [dbo].[Widget] WHERE [Uid] = 'elearning-prod')
BEGIN
INSERT INTO [dbo].[Widget] ([Uid], [Title], [Description], [Category], [Icon], [Template], [UAMApplication], [UAMFunction], [ServiceURL], [ServiceConfiguration], [Configuration]) 
    VALUES('elearning-prod', 'E-Learning Prod', 'Table of content for E-Learning modules', 'General', 'recentsearches', 'ELearning', 'MYDAMCO', 'USE',
    'ServiceURL',
    '{
        "folders": [
            { "application": "Reporting", "folder": "C:\\elearning\\REPORTING", "UAMApplication": "REPORTING", "width": 1031, "height": 804 },
            { "application": "Booking", "folder": "C:\\elearning\\BOOKING", "UAMApplication": "BOOKING", "width": 1031, "height": 804 },
			{ "application": "Shipper", "folder": "C:\\elearning\\SHIPPING", "include": "Damco Shipper Online Course.*", "UAMApplication": "BOOKING", "width": 1030,"height": 800 },
			{ "application": "Document Management", "id": 1, "folder": "C:\\elearning\\DocMan\\Client", "include": "Document_Management_For*.*", "UAMApplication": "DOCUMENT_MANAGEMENT", "UAMFunction": "CLIENT", "width": 1024,"height": 768 },
			{ "application": "Document Management", "id": 2, "folder": "C:\\elearning\\DocMan\\Broker", "include": "Document_Management_For*.*", "UAMApplication": "DOCUMENT_MANAGEMENT", "UAMFunction": "BROKER", "width": 1024,"height": 768 },
			{ "application": "Document Management", "id": 3, "folder": "C:\\elearning\\DocMan\\SDS", "include": "Document_Management_For*.*", "UAMApplication": "DOCUMENT_MANAGEMENT", "UAMFunction": "RELEASE_POUCH", "width": 1024,"height": 768 }
         ]
    }',
    '{
        "serviceurl": "Services/ELearning",
        "targeturl": "Services/ELearningLoadFile"
    }');
END


