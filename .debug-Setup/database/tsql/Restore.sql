USE [master]
GO

-----
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = '$(BRANCH)_$(CUSTOMER)_TeleoptiAnalytics')
ALTER DATABASE [$(BRANCH)_$(CUSTOMER)_TeleoptiAnalytics] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE
GO
IF $(LOADSTAT) = 1
BEGIN
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
	set @path = N'$(RARFOLDER)\$(CUSTOMER)_TeleoptiAnalytics.BAK'

	declare @TeleoptiAnalytics_Primary nvarchar(128)
	declare @TeleoptiAnalytics_Stage nvarchar(128)
	declare @TeleoptiAnalytics_Mart nvarchar(128)
	declare @TeleoptiAnalytics_Msg nvarchar(128)
	declare @TeleoptiAnalytics_Rta nvarchar(128)
	declare @TeleoptiAnalytics_Log nvarchar(128)

	insert #tmp
	EXEC ('restore filelistonly from disk = ''' + @path + '''')

	select @TeleoptiAnalytics_Primary	=LogicalName from #tmp where FileGroupName='PRIMARY'
	select @TeleoptiAnalytics_Stage		=LogicalName from #tmp where FileGroupName='STAGE'
	select @TeleoptiAnalytics_Mart		=LogicalName from #tmp where FileGroupName='MART'
	select @TeleoptiAnalytics_Msg		=LogicalName from #tmp where FileGroupName='MSG'
	select @TeleoptiAnalytics_Rta		=LogicalName from #tmp where FileGroupName='RTA'
	select @TeleoptiAnalytics_Log		=LogicalName from #tmp where [Type]='L'

	PRINT 'Restoring $(BRANCH)_$(CUSTOMER)_TeleoptiAnalytics'

	SELECT @restoreCommand ='RESTORE DATABASE [$(BRANCH)_$(CUSTOMER)_TeleoptiAnalytics]'+
	' FROM  DISK = N''$(RARFOLDER)\$(CUSTOMER)_TeleoptiAnalytics.BAK'''+
	' WITH  FILE = 1,'+
	' MOVE N'''+@TeleoptiAnalytics_Primary+''' TO N''$(DATAFOLDER)\$(BRANCH)_$(CUSTOMER)_TeleoptiAnalytics_Primary.mdf'','+
	' MOVE N''' +@TeleoptiAnalytics_Log + ''' TO N''$(DATAFOLDER)\$(BRANCH)_$(CUSTOMER)_TeleoptiAnalytics_Log.ldf'','+
	' MOVE N''' +@TeleoptiAnalytics_Stage + ''' TO N''$(DATAFOLDER)\$(BRANCH)_$(CUSTOMER)_TeleoptiAnalytics_Stage.ndf'','+
	' MOVE N''' +@TeleoptiAnalytics_Mart + ''' TO N''$(DATAFOLDER)\$(BRANCH)_$(CUSTOMER)_TeleoptiAnalytics_Mart.ndf'','+
	' MOVE N''' +@TeleoptiAnalytics_Msg + ''' TO N''$(DATAFOLDER)\$(BRANCH)_$(CUSTOMER)_TeleoptiAnalytics_Msg.ndf'','+
	' MOVE N''' +@TeleoptiAnalytics_Rta + ''' TO N''$(DATAFOLDER)\$(BRANCH)_$(CUSTOMER)_TeleoptiAnalytics_Rta.ndf'','+
	' NOUNLOAD,	REPLACE, STATS = 10'

	exec sp_executesql @restoreCommand

	--waitfor database to open
	DECLARE @MultiUserError int
	SET @MultiUserError = 1
	WHILE @MultiUserError <> 0
	BEGIN
		PRINT 'waiting for $(BRANCH)_$(CUSTOMER)_TeleoptiAnalytics database to recover ...'
		ALTER DATABASE [$(BRANCH)_$(CUSTOMER)_TeleoptiAnalytics] SET  MULTI_USER
		SET @MultiUserError = @@error
		IF @MultiUserError = 0 BREAK
		WAITFOR DELAY '00:00:05.000'
	END

	print 'done!'

END
GO
---
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = '$(BRANCH)_$(CUSTOMER)_TeleoptiCCCAgg')
ALTER DATABASE [$(BRANCH)_$(CUSTOMER)_TeleoptiCCCAgg] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE
GO

IF $(LOADSTAT) = 1
BEGIN
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
	set @path = N'$(RARFOLDER)\$(CUSTOMER)_TeleoptiCCCAgg.BAK'

	declare @mdfFile nvarchar(128)
	declare @ldfFile nvarchar(128)

	insert #tmp
	EXEC ('restore filelistonly from disk = ''' + @path + '''')

	select @mdfFile=LogicalName from #tmp where [Type]='D'
	select @ldfFile=LogicalName from #tmp where [Type]='L'

	PRINT 'Restoring $(BRANCH)_$(CUSTOMER)_TeleoptiCCCAgg'

	SELECT @restoreCommand ='RESTORE DATABASE [$(BRANCH)_$(CUSTOMER)_TeleoptiCCCAgg]'+
	' FROM  DISK = N''$(RARFOLDER)\$(CUSTOMER)_TeleoptiCCCAgg.BAK''' +
	' WITH  FILE = 1,' +
	' MOVE N''' + @mdfFile + ''' TO N''$(DATAFOLDER)\$(BRANCH)_$(CUSTOMER)_TeleoptiCCCAgg.mdf'','+
	' MOVE N''' + @ldfFile + ''' TO N''$(DATAFOLDER)\$(BRANCH)_$(CUSTOMER)_TeleoptiCCCAgg.ldf'','+
	' NOUNLOAD,REPLACE,STATS = 10'

	exec sp_executesql @restoreCommand

	--waitfor database to open
	DECLARE @MultiUserError int
	SET @MultiUserError = 1
	WHILE @MultiUserError <> 0
	BEGIN
		PRINT 'waiting for $(BRANCH)_$(CUSTOMER)_TeleoptiCCCAgg database to recover ...'
		ALTER DATABASE [$(BRANCH)_$(CUSTOMER)_TeleoptiCCCAgg] SET  MULTI_USER
		SET @MultiUserError = @@error
		IF @MultiUserError = 0 BREAK
		WAITFOR DELAY '00:00:05.000'
	END

	print 'done!'
END

GO
--
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = '$(BRANCH)_$(CUSTOMER)_TeleoptiCCC7')
ALTER DATABASE [$(BRANCH)_$(CUSTOMER)_TeleoptiCCC7] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE
GO
PRINT 'Restoring $(BRANCH)_$(CUSTOMER)_TeleoptiCCC7'

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

	set @path = N'$(RARFOLDER)\$(CUSTOMER)_TeleoptiCCC7.BAK'

	declare @mdfFile nvarchar(128)
	declare @ldfFile nvarchar(128)

	insert #tmp
	EXEC ('restore filelistonly from disk = ''' + @path + '''')

	select @mdfFile=LogicalName from #tmp where [Type]='D'
	select @ldfFile=LogicalName from #tmp where [Type]='L'

	SELECT @restoreCommand ='RESTORE DATABASE [$(BRANCH)_$(CUSTOMER)_TeleoptiCCC7]'+
	' FROM  DISK = N''$(RARFOLDER)\$(CUSTOMER)_TeleoptiCCC7.BAK''' +
	' WITH  FILE = 1,' +
	' MOVE N''' + @mdfFile + ''' TO N''$(DATAFOLDER)\$(BRANCH)_$(CUSTOMER)_TeleoptiCCC7.mdf'','+
	' MOVE N''' + @ldfFile + ''' TO N''$(DATAFOLDER)\$(BRANCH)_$(CUSTOMER)_TeleoptiCCC7.ldf'','+
	' NOUNLOAD,REPLACE,STATS = 10'

	exec sp_executesql @restoreCommand

--waitfor database to open
DECLARE @MultiUserError int
SET @MultiUserError = 1
WHILE @MultiUserError <> 0
BEGIN
	PRINT 'waiting for $(BRANCH)_$(CUSTOMER)_TeleoptiCCC7 database to recover ...'
	ALTER DATABASE [$(BRANCH)_$(CUSTOMER)_TeleoptiCCC7] SET  MULTI_USER
	SET @MultiUserError = @@error
	IF @MultiUserError = 0 BREAK
	WAITFOR DELAY '00:00:05.000'
END

print 'done!'
GO

--Adding current windows user to the SuperUser
DECLARE @WinUser varchar(50)
DECLARE @WinDomain varchar(50)
DECLARE @delim varchar(1)
DECLARE @commaindex int
DECLARE @csv varchar(100)
DECLARE @userid uniqueidentifier

SET @delim = '\'
SELECT @csv=system_user

SELECT @commaindex = CHARINDEX(@delim, @csv)
	
SELECT @WinDomain = LEFT(@csv, @commaindex-1)

SELECT @WinUser = RIGHT(@csv, LEN(@csv) - @commaindex)

SELECT @userid = Id FROM [$(BRANCH)_$(CUSTOMER)_TeleoptiCCC7].dbo.Person WHERE DomainName=@WinDomain AND WindowsLogOnName=@WinUser --If you already exists in this database
--ELSE
IF @userid IS NULL 
	BEGIN
		--Declare
		DECLARE @now		AS datetime
		DECLARE @adminLogon	AS nvarchar(50)
		DECLARE @adminPwd	AS nvarchar(50)
		DECLARE @roleId		AS uniqueidentifier

		--Init
		SET @now	= getdate()
		SET @userid	= newid()
		SET @adminLogon	= N''
		SET @adminPwd	= N''
		SET @roleId	= '193AD35C-7735-44D7-AC0C-B8EDA0011E5F' --_SuperRole

		--Nhib-style
		DECLARE @p0 int,@p1 uniqueidentifier,@p2 datetime,@p3 nvarchar(4000),@p4 nvarchar(4000),@p5 nvarchar(4000),@p6 datetime,@p7 nvarchar(12),@p8 nvarchar(20),@p9 uniqueidentifier,@p10 nvarchar(4000),@p11 int,@p12 int,@p13 nvarchar(4000),@p14 nvarchar(4000),@p15 nvarchar(4000),@p16 nvarchar(4000),@p17 bit,@p18 uniqueidentifier,@p19 uniqueidentifier,@p20 bit
		SET @p0=1
		SET @p1=@userid
		SET @p2=@now
		SET @p3=N''
		SET @p4=N''
		SET @p5=N''
		SET @p6=NULL
		SET @p7=RTRIM(LTRIM(@WinUser))
		SET @p8=N'(Admin)'
		SET @p9=NULL
		SET @p10='W. Europe Standard Time'
		SET @p11=NULL
		SET @p12=NULL
		SELECT @p13=RTRIM(LTRIM(@WinUser))
		SELECT @p14=RTRIM(LTRIM(@WinDomain))
		SET @p15=@adminLogon
		SET @p16=@adminPwd
		SET @p17=0
		SET @p18=@userid  --Admin CreatedBy Admin
		SET @p19 = @roleId
		SET @p20 = 0 --Can be change by user

		INSERT INTO [$(BRANCH)_$(CUSTOMER)_TeleoptiCCC7].dbo.Person
		(Version, CreatedBy, CreatedOn, UpdatedBy, UpdatedOn, Email, Note, EmploymentNumber, TerminalDate, FirstName, LastName, PartOfUnique, DefaultTimeZone, Culture, UiCulture, WindowsLogOnName, DomainName, ApplicationLogOnName, Password, IsDeleted, Id, BuiltIn)
		VALUES (@p0, @p1, @p2, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16, @p17, @p18,@p20)
		IF @@ERROR <> 0 PRINT 'Error inserting Admin user'
		ELSE PRINT 'Inserted Admin user'

		INSERT INTO [$(BRANCH)_$(CUSTOMER)_TeleoptiCCC7].dbo.PersonInApplicationRole (Person, ApplicationRole)
		VALUES (@p18, @p19)
		IF @@ERROR <> 0 PRINT 'Error connecting person and application role'
		ELSE PRINT 'Inserted relation in PersonInApplicationRole'
	END

--Add currect user to IIS-users: update aspnet_users
UPDATE [$(BRANCH)_$(CUSTOMER)_TeleoptiAnalytics].dbo.aspnet_Users
SET UserName=system_user,LoweredUserName=system_user
WHERE userid=@userid