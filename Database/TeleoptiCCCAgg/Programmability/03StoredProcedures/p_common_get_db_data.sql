IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_common_get_db_data]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_common_get_db_data]
GO

CREATE PROCEDURE [dbo].[p_common_get_db_data]

AS

SELECT CONVERT(nvarchar(50),SERVERPROPERTY('ServerName')) + '/' + DB_NAME() as aggdb

GO

