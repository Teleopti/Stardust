
BEGIN TRY

/*==============================================
Prepare statements according to server settings
and given input
==============================================*/

	-- DECLARES
	DECLARE @rc				int
	DECLARE @DataDir		nvarchar(4000)
	DECLARE @LogDir 		nvarchar(4000)
	DECLARE @LogFileName	nvarchar(4000)
	DECLARE @DataFileName	nvarchar(4000)
	DECLARE @CMD 			nvarchar(4000)

	-- Read reg values to get default datapath
	EXEC @rc = master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',N'Software\Microsoft\MSSQLServer\MSSQLServer',N'DefaultData', @DataDir output, 'no_output' 
	
	-- Check if value is NULL. In that case no changes has been done to default setup. Read from SQLDataRoot instead
	IF (@DataDir is null) 
	BEGIN
		EXEC	@rc = master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',N'Software\Microsoft\MSSQLServer\Setup',N'SQLDataRoot', @DataDir output, 'no_output'
		SELECT	@DataDir = @DataDir + N'\Data'
	END

	-- Read reg values to get default logpath
	EXEC @rc = master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',N'Software\Microsoft\MSSQLServer\MSSQLServer',N'DefaultLog', @LogDir output, 'no_output' 

	-- Check if value is NULL. In that case no changes has been done to default setup. Read from SQLDataRoot instead
	IF (@LogDir is null)
	BEGIN
		EXEC	@rc = master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',N'Software\Microsoft\MSSQLServer\Setup',N'SQLDataRoot', @LogDir output, 'no_output'
		SELECT	@LogDir = @LogDir + N'\Data'
	END

	-- Add filenames to the folders
	SET @DataFileName	= @DataDir + '\$(DBNAME)_Data.mdf'
	SET @LogFileName	= @LogDir + '\$(DBNAME)_Log.ldf'
	
	-- Prepare statement
	SELECT @CMD =
	'CREATE DATABASE $(DBNAME)
	ON
		(
		NAME = $(DBNAME)_Data,
		FILENAME = ''' + @DataFileName + ''',
		SIZE = 50,
		FILEGROWTH = 10
		)
	LOG ON	(
		NAME = $(DBNAME)_Log,
		FILENAME = ''' + @LogFileName + ''',
		SIZE = 100,
		FILEGROWTH = 20
		)
	'
/*==============================================
This is were the action starts
==============================================*/
	PRINT	'Adding database $(DBNAME). Working...'
	
	-- Create database
	EXEC (@CMD)

	PRINT	'Adding database $(DBNAME). Finished!'

	PRINT	'Adding settings and properties for $(DBNAME). Working...'
	-- Add DBTYPE as extended property
	EXEC $(DBNAME).sys.sp_addextendedproperty @name=N'DatabaseType', @value=N''

	--Disable fulltext
	IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
	BEGIN
		EXEC $(DBNAME).[dbo].[sp_fulltext_database] @action = 'disable'
	END

	-- Set recovery model
	ALTER DATABASE $(DBNAME) SET RECOVERY FULL

	ALTER DATABASE $(DBNAME) SET MULTI_USER
	
	--All other database settings are based on the "model" database in local instance
	--see: sp_configure

	-- Set owner = sa (cosmetics only)
	EXEC $(DBNAME).dbo.sp_changedbowner @loginame = N'sa', @map = false

	PRINT	'Adding settings and properties for $(DBNAME). Finished'

END TRY

BEGIN CATCH
	DECLARE	@ErrorMessage			NVARCHAR(4000)
	DECLARE	@ErrorNumber			INT
	DECLARE	@ErrorSeverity			INT
	DECLARE	@ErrorState				INT
	DECLARE	@ErrorLine				INT

	IF ERROR_NUMBER() IS NOT NULL
	BEGIN
		SET	@ErrorNumber	= ERROR_NUMBER()
		SET	@ErrorSeverity	= ERROR_SEVERITY()
		SET	@ErrorState		= ERROR_STATE()
		SET	@ErrorLine		= ERROR_LINE()

		-- Return an error with state 127 since it will abort SQLCMD
		SET @ErrorMessage = 'Error %d, Severity %d, State %d, Line %d, Message: '+ ERROR_MESSAGE()
		RAISERROR (@ErrorMessage, 16, 127, @ErrorNumber, @ErrorSeverity, @ErrorState, @ErrorLine)
	END

END CATCH
PRINT	'Finished'

/*******************************************************************************/
