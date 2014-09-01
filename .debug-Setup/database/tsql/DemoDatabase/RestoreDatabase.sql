USE master
GO


/*
:SETVAR DATABASENAME testDatabaseName
:SETVAR DATAFOLDER D:\R2L\Data
:SETVAR BAKFILE D:\R2L\Baseline\DemoSales_TeleoptiCCC7.bak
*/
-----
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = '$(DATABASENAME)')
ALTER DATABASE [$(DATABASENAME)] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE
GO

declare @path varchar(1000)
declare @restoreCommand nvarchar(4000)

IF OBJECT_ID('tempdb..#tmp') IS NOT NULL DROP TABLE #tmp
create table #tmp
(
LogicalName nvarchar(128)
,PhysicalName nvarchar(260)
,Type char(1)
,FileGroupName nvarchar(128)
,Size numeric(20,0)
,MaxSize numeric(20,0),
FileId tinyint,
CreateLSN numeric(25,0),
DropLSN numeric(25, 0),
UniqueID uniqueidentifier,
ReadOnlyLSN numeric(25,0),
ReadWriteLSN numeric(25,0),
BackupSizeInBytes bigint,
SourceBlockSize int,
FileGroupId int,
LogGroupGUID uniqueidentifier,
DifferentialBaseLSN numeric(25,0),
DifferentialBaseGUID uniqueidentifier,
IsReadOnly bit,
IsPresent bit,
TDEThumbprint varbinary(32)
)
set @path = N'$(BAKFILE)'

DECLARE @DataDir nvarchar(260)
SET @DataDir=N'$(DATAFOLDER)'

IF (@DataDir='xp_instance_regread')
BEGIN
	EXEC	master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',N'Software\Microsoft\MSSQLServer\Setup',N'SQLDataRoot', @DataDir output, 'no_output'
	SELECT	@DataDir = @DataDir + N'\Data'
END

declare @mdfFile nvarchar(128)
declare @ldfFile nvarchar(128)

insert #tmp
EXEC ('restore filelistonly from disk = ''' + @path + '''')

select @mdfFile=LogicalName from #tmp where [Type]='D'
select @ldfFile=LogicalName from #tmp where [Type]='L'

PRINT 'Restoring $(DATABASENAME)'

SELECT @restoreCommand ='RESTORE DATABASE [$(DATABASENAME)]'+
' FROM  DISK = N''$(BAKFILE)''' +
' WITH  FILE = 1,' +
' MOVE N''' + @mdfFile + ''' TO N''' + @DataDir + '\$(DATABASENAME).mdf'','+
' MOVE N''' + @ldfFile + ''' TO N''' + @DataDir + '\$(DATABASENAME).ldf'','+
' NOUNLOAD,REPLACE,STATS = 10'

exec sp_executesql @restoreCommand

--waitfor database to open
DECLARE @MultiUserError int
SET @MultiUserError = 1
WHILE @MultiUserError <> 0
BEGIN
	PRINT 'waiting for $(DATABASENAME) database to recover ...'
	ALTER DATABASE [$(DATABASENAME)] SET  MULTI_USER
	SET @MultiUserError = @@error
	IF @MultiUserError = 0 BREAK
	WAITFOR DELAY '00:00:05.000'
END

print 'done!'

GO
