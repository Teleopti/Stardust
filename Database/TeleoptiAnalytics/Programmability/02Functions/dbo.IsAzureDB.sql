IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IsAzureDB]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[IsAzureDB]
GO

-- =============================================
-- Author:		Per Andersson
-- Create date: 2011-06-09
-- Description:	This function returns True if the database is in Azure.
-- =============================================
CREATE FUNCTION [dbo].[IsAzureDB]()
returns bit
AS
BEGIN

 DECLARE @sqlEdition NVARCHAR(200)
 SELECT @sqlEdition = CONVERT(NVARCHAR(200), SERVERPROPERTY('edition'))

 -- Azure database version
 IF @sqlEdition = 'SQL Azure'
	RETURN 1
 RETURN 0
END

GO