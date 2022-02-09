USE [MyDamcoV4]
GO

SET ANSI_NULLS ON
GO

/* Insert an additional news feed/category into damco news to get interesting test cases for the Selenium tests. */
UPDATE [dbo].[Widget] SET [Configuration] = '{
    "feeds": [
        {
			"id": "2",
			"title": "General Damco News"
		},
		{
			"id": "3",
			"title": "Extra Damco News"
		}
    ],
    "headlinecount": 10
	}'
WHERE [Uid] = 'damconews';

IF EXISTS (SELECT * FROM [dbo].[NewsItem] WHERE [dbo].[NewsItem].[NewsCategory_Id]=2)
BEGIN
    DELETE FROM [dbo].[NewsItem] WHERE [dbo].[NewsItem].[NewsCategory_Id]=2
END

INSERT INTO [dbo].[NewsItem] ([Title], [Description], [Body], [From], [NewsCategory_Id])
VALUES ('Test Damco News', 'This is a brief description of the test news.', 'This is the actual content of the test news', '2004-05-23T14:25:10',  2),
       ('Test Damco News 2', 'This is a brief description of the second test news.', 'This is the actual content of the second test news', '2004-05-23T14:25:10', 2);

IF NOT EXISTS(SELECT * FROM [dbo].[NewsCategory] WHERE [Name] = 'Damco Extra News' AND [id] = 3)
BEGIN
    SET IDENTITY_INSERT [dbo].[NewsCategory] ON; /* has to be at id=3 to match the id in the JSON stored in [Widget] */
    INSERT INTO [dbo].[NewsCategory] ([Id], [Name], [Language], [Description], [Downtime], [editUAMApplication], [editUAMFunction], [showUAMApplication], [showUAMFunction]) 
    VALUES (3, 'Extra Damco News', 'en', 'The news we forgot in the other category', 0, 'MYDAMCO', 'EDITOR', 'MYDAMCO', 'USE');
    SET IDENTITY_INSERT [dbo].[NewsCategory] OFF;
END

IF EXISTS (SELECT * FROM [dbo].[NewsItem] WHERE [dbo].[NewsItem].[NewsCategory_Id]=3)
BEGIN
    DELETE FROM [dbo].[NewsItem] WHERE [dbo].[NewsItem].[NewsCategory_Id]=3
END

/* insert enough newsitems for at least 3 pages */
INSERT INTO [dbo].[NewsItem] ([Title],[Description],[Body],[From],[To],[NewsCategory_Id],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) 
VALUES ('Extra Test Damco News', 'This is a brief description of this news item', 'This is the content of this news item', '2 December 2012 12:34:56', NULL, 3, NULL, NULL, NULL, NULL),
       ('Extra Test Damco News 2', 'This is a brief description of this second extra news item', 'This is the content of this second extra news item', '2 December 2012 11:22:33', NULL, 3, NULL, NULL, NULL, NULL),
       ('Extra Test Damco News 3', 'This is a brief description of this news item', 'This is the content of this news item', '1 January 2003 00:11:22', NULL, 3, NULL, NULL, NULL, NULL),
       ('Extra Test Damco News 4', 'This is a brief description of this news item', 'This is the content of this news item', '2 February 2003 00:11:22', NULL, 3, NULL, NULL, NULL, NULL),
       ('Extra Test Damco News 5', 'This is a brief description of this news item', 'This is the content of this news item', '2 March 2003 00:11:22', NULL, 3, NULL, NULL, NULL, NULL),
       ('Extra Test Damco News 6', 'This is a brief description of this news item', 'This is the content of this news item', '2 April 2003 00:11:22', NULL, 3, NULL, NULL, NULL, NULL),
	   ('Extra Test Damco News 6½', 'This is a brief description of this news item', 'This is the content of this news item', '23 April 2003 22:11:00', NULL, 3, NULL, NULL, NULL, NULL),
	   ('Extra Test Damco News 7', 'This is a brief description of this news item', 'This is the content of this news item', '2 May 2003 00:11:22', NULL, 3, NULL, NULL, NULL, NULL),
       ('Extra Test Damco News 8', 'This is a brief description of this news item', 'This is the content of this news item', '3 June 2003 00:11:22', NULL, 3, NULL, NULL, NULL, NULL),
       ('Extra Test Damco News 9', 'This is a brief description of this news item', 'This is the content of this news item', '10 July 2003 00:11:22', NULL, 3, NULL, NULL, NULL, NULL),
       ('Extra Test Damco News 10', 'This is a brief description of this news item', 'This is the content of this news item', '25 August 2003 00:11:22', NULL, 3, NULL, NULL, NULL, NULL),
       ('Extra Test Damco News 11', 'This is a brief description of this news item', 'This is the content of this news item', '8 September 2003 00:11:22', NULL, 3, NULL, NULL, NULL, NULL),
       ('Extra Test Damco News 12', 'This is a brief description of this news item', 'This is the content of this news item', '29 October 2003 00:11:22', NULL, 3, NULL, NULL, NULL, NULL),
       ('Extra Test Damco News 13', 'This is a brief description of this news item', 'This is the content of this news item', '16 November 2003 00:11:22', NULL, 3, NULL, NULL, NULL, NULL),
	   ('Old News 1', 'This is a brief description of this news item', 'This is the content of this news item', '25 September 1987 12:34:56', NULL, 3, NULL, NULL, NULL, NULL),
	   ('Old News 2', 'This is a brief description of this news item', 'This is the content of this news item', '25 September 1987 13:34:56', NULL, 3, NULL, NULL, NULL, NULL),
	   ('Old News 3', 'This is a brief description of this news item', 'This is the content of this news item', '25 September 1987 14:34:56', NULL, 3, NULL, NULL, NULL, NULL),
	   ('Old News 4', 'This is a brief description of this news item', 'This is the content of this news item', '25 September 1987 15:34:56', NULL, 3, NULL, NULL, NULL, NULL),
	   ('Old News 5', 'This is a brief description of this news item', 'This is the content of this news item', '25 September 1987 16:34:56', NULL, 3, NULL, NULL, NULL, NULL),
	   ('Old News 6', 'This is a brief description of this news item', 'This is the content of this news item', '25 September 1987 17:34:56', NULL, 3, NULL, NULL, NULL, NULL),
	   ('Old News 7', 'This is a brief description of this news item', 'This is the content of this news item', '25 September 1987 18:34:56', NULL, 3, NULL, NULL, NULL, NULL);


