IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IndexMaintenance]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[IndexMaintenance]
GO

CREATE PROCEDURE [dbo].[IndexMaintenance]
WITH EXECUTE AS OWNER --Makes it possible for low permissions login (guest, readonly, execute role) to alter table/index in this DB
AS
SET NOCOUNT ON
DECLARE @Version numeric(18,10)
DECLARE @CurrentDatabase nvarchar(128)
DECLARE @ReturnCode int

SET @ReturnCode=0
SET @Version = CAST(LEFT(CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max)),CHARINDEX('.',CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max))) - 1) + '.' + REPLACE(RIGHT(CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max)), LEN(CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max))) - CHARINDEX('.',CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max)))),'.','') AS numeric(18,10))
SET @CurrentDatabase=db_name()

--Ola Hallengren dbo.IndexOptimize only works from the NEW! SQL Azure preview edition (12.0)
IF SERVERPROPERTY('EngineEdition') = 5 AND @Version < 12
BEGIN
	EXEC @ReturnCode = [dbo].[IndexMaintenance_Azure_v11]
END
ELSE
BEGIN
	EXEC @ReturnCode = [dbo].[IndexOptimize]
				@Databases=@CurrentDatabase,
				@LogToTable='Y',
				@PageCountLevel=100,
				@UpdateStatistics = 'ALL',
				@OnlyModifiedStatistics = 'Y'
END

IF @ReturnCode <> 0
BEGIN
	RETURN @ReturnCode
END

GO

