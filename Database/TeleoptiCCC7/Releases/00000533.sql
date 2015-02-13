-- =============================================
-- Author: Jianguang Fang
-- Create date: 2015-02-13
-- Description:	remove the redundant foreign key of table dbo.request.
-- =============================================

--FK_Request_PersonRequest and FK_RequestPart_PersonRequest are identical FK, remove one.
IF OBJECT_ID('FK_Request_PersonRequest', 'F') IS NOT NULL 
	AND OBJECT_ID('FK_RequestPart_PersonRequest', 'F') IS  NOT NULL
	ALTER TABLE dbo.request DROP CONSTRAINT FK_RequestPart_PersonRequest
