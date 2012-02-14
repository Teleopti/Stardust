--DJ
--Clean up tables as part of every release
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NetworkRunnerRead]') AND type in (N'U'))
DROP TABLE [dbo].[NetworkRunnerRead]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NetworkRunnerWrite]') AND type in (N'U'))
DROP TABLE [dbo].[NetworkRunnerWrite]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2010-10-07
-- Description: The main pupose is to generate a fixed amount of data to the client. Currently 67 Mb
--				The sp will also test the SQL Server performance,
--				by putting a pretty agressive I/O operation + CPU intensive load on the SQL server
--
--				Current Metrix:
--				CPU		Reads	Writes	Duration (Teleopti VPN)
--				9453	955777	24593	102128
--
-- Change Log
-----------------------------------------------
-- Who	When		What
-----------------------------------------------
-- DJ	1020-10-07	Casting Mb as int, add Exucute as
-- DJ	2011-04-20	Make Azure compatible
--
-- =============================================
-- SET STATISTICS IO ON
-- SET STATISTICS TIME ON
-- [dbo].[sp_NetworkRunnerRead] 0 --one Result Sets  back to client (number of bytes in table)
-- [dbo].[sp_NetworkRunnerRead] 1 --two Result Sets  back to client (number of bytes in table + data)
-- =============================================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_NetworkRunnerSetup]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_NetworkRunnerSetup]
GO

CREATE PROCEDURE [dbo].[sp_NetworkRunnerSetup]
WITH EXECUTE AS OWNER --Makes it possible for low permissions login (guest, readonly, execute role) to create a table in this DB
AS

	--Read table
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NetworkRunnerRead]') AND type in (N'U'))
	BEGIN
		CREATE TABLE [dbo].[NetworkRunnerRead] (
			x int NOT NULL
			,x2 int NOT NULL
			,y char(10) NOT NULL DEFAULT ('')
			,z char(10) NOT NULL DEFAULT('')
			)

		ALTER TABLE [dbo].[NetworkRunnerRead] ADD  CONSTRAINT [PK_NetworkRunnerRead] PRIMARY KEY CLUSTERED 
			(
				[x] ASC
			)
	END
	
	--write table
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NetworkRunnerWrite]') AND type in (N'U'))
	BEGIN
		CREATE TABLE [dbo].[NetworkRunnerWrite](
			[Id] [uniqueidentifier] NOT NULL,
			[Parent] [uniqueidentifier] NOT NULL,
			[Value] [float] NOT NULL,
			[Seconds] [float] NOT NULL,
			[MaxOccupancy] [float] NOT NULL,
			[MinOccupancy] [float] NOT NULL,
			[PersonBoundary_Maximum] [int] NOT NULL,
			[PersonBoundary_Minimum] [int] NOT NULL,
			[Shrinkage] [float] NOT NULL,
			[StartDateTime] [datetime] NOT NULL,
			[EndDateTime] [datetime] NOT NULL,
			[Efficiency] [float] NOT NULL
			)

		ALTER TABLE [dbo].[NetworkRunnerWrite] ADD  CONSTRAINT [PK_NetworkRunnerWrite] PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		)
	END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_NetworkRunnerRead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_NetworkRunnerRead]
GO

CREATE PROCEDURE [dbo].[sp_NetworkRunnerRead]
@ReturnData bit = 1
WITH EXECUTE AS OWNER --Makes it possible for low permissions login (guest, readonly, execute role) to create a table in this DB

AS
SET NOCOUNT ON

	EXEC [sp_NetworkRunnerSetup]
	
	--declares
	DECLARE @rows int
	DECLARE @toKeep int
	DECLARE @i int
	DECLARE @j int

	--init
	SET @j = 2500
	SET @i = 0

	--init
	SELECT @rows = 100000 --100.000 will return ~63 Mb of data
	SELECT @toKeep = @rows*2/3
	
	--spt_values (corresponding to master.dbo.spt_values)
	DECLARE @spt_values TABLE (intCol int NOT NULL)

	--fill up the temp table with data
	WHILE @i < @j
	BEGIN            
		INSERT @spt_values (intCol)            
		SELECT @i
		SET @i = @i + 1
	CONTINUE            
	END

	--insert dummy values
	INSERT [dbo].[NetworkRunnerRead]  (x, x2)
	SELECT TOP(@rows)
	ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS r
	,ROW_NUMBER() OVER (ORDER BY (SELECT 1)) % 10 AS s
	FROM @spt_values a CROSS JOIN @spt_values b

	--Create some potential page splits
	ALTER TABLE [dbo].[NetworkRunnerRead] ALTER COLUMN y char(892)
	ALTER TABLE [dbo].[NetworkRunnerRead] ALTER COLUMN z char(100)

	--Put some more dummy data into the table
	UPDATE [dbo].[NetworkRunnerRead]
	SET y=NEWID(),z=NEWID()

	--fragment the table
	DELETE TOP(@rows - @toKeep) FROM [dbo].[NetworkRunnerRead] WHERE x2 IN(2, 4, 6, 8)

	--return number of bytes
	SELECT CAST(SUM(DATALENGTH(x)+DATALENGTH(x2)+DATALENGTH(y)+DATALENGTH(z)) AS INT) AS NumberOfBytes
	FROM [dbo].[NetworkRunnerRead] 

	--resultset
	IF @ReturnData = 1
	SELECT * FROM [dbo].[NetworkRunnerRead]

	--truncate to avoid extra data in DB-backups
	TRUNCATE TABLE [dbo].[NetworkRunnerRead]

RETURN 0
