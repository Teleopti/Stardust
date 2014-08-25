USE [master]
GO
-----
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = '$(CUSTOMER)_TeleoptiAnalytics')
ALTER DATABASE [$(CUSTOMER)_TeleoptiAnalytics] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE
GO
IF $(LOADSTAT) = 1
BEGIN
	PRINT 'Restoring $(CUSTOMER)_TeleoptiAnalytics'

	RESTORE DATABASE [$(CUSTOMER)_TeleoptiAnalytics]
	FROM  DISK = N'$(RARFOLDER)\$(CUSTOMER)_TeleoptiAnalytics.BAK'
	WITH  FILE = 1,
	MOVE N'TeleoptiAnalytics_Primary' TO N'$(DATAFOLDER)\$(CUSTOMER)_TeleoptiAnalytics_Primary.mdf',
	MOVE N'TeleoptiAnalytics_Log' TO N'$(DATAFOLDER)\$(CUSTOMER)_TeleoptiAnalytics_Log.ldf',
	MOVE N'TeleoptiAnalytics_Stage' TO N'$(DATAFOLDER)\$(CUSTOMER)_TeleoptiAnalytics_Stage.ndf',
	MOVE N'TeleoptiAnalytics_Mart' TO N'$(DATAFOLDER)\$(CUSTOMER)_TeleoptiAnalytics_Mart.ndf',
	MOVE N'TeleoptiAnalytics_Msg' TO N'$(DATAFOLDER)\$(CUSTOMER)_TeleoptiAnalytics_Msg.ndf',
	MOVE N'TeleoptiAnalytics_Rta' TO N'$(DATAFOLDER)\$(CUSTOMER)_TeleoptiAnalytics_Rta.ndf',
	NOUNLOAD,
	REPLACE,
	STATS = 10

	--waitfor database to open
	DECLARE @MultiUserError int
	SET @MultiUserError = 1
	WHILE @MultiUserError <> 0
	BEGIN
		PRINT 'waiting for $(CUSTOMER)_TeleoptiAnalytics database to recover ...'
		ALTER DATABASE [$(CUSTOMER)_TeleoptiAnalytics] SET  MULTI_USER
		SET @MultiUserError = @@error
		IF @MultiUserError = 0 BREAK
		WAITFOR DELAY '00:00:05.000'
	END

	print 'done!'

END
GO
---
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = '$(CUSTOMER)_TeleoptiCCCAgg')
ALTER DATABASE [$(CUSTOMER)_TeleoptiCCCAgg] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE
GO

IF $(LOADSTAT) = 1
BEGIN
	PRINT 'Restoring $(CUSTOMER)_TeleoptiCCCAgg'

	RESTORE DATABASE [$(CUSTOMER)_TeleoptiCCCAgg]
	FROM  DISK = N'$(RARFOLDER)\$(CUSTOMER)_TeleoptiCCCAgg.BAK'
	WITH  FILE = 1,
	MOVE N'TeleoptiCCCAgg_Data' TO N'$(DATAFOLDER)\$(CUSTOMER)_TeleoptiCCCAgg.mdf',
	MOVE N'TeleoptiCCCAgg_Log' TO N'$(DATAFOLDER)\$(CUSTOMER)_TeleoptiCCCAgg.ldf',
	NOUNLOAD,
	REPLACE,
	STATS = 10

	--waitfor database to open
	DECLARE @MultiUserError int
	SET @MultiUserError = 1
	WHILE @MultiUserError <> 0
	BEGIN
		PRINT 'waiting for $(CUSTOMER)_TeleoptiCCCAgg database to recover ...'
		ALTER DATABASE [$(CUSTOMER)_TeleoptiCCCAgg] SET  MULTI_USER
		SET @MultiUserError = @@error
		IF @MultiUserError = 0 BREAK
		WAITFOR DELAY '00:00:05.000'
	END

	print 'done!'

END

GO

--
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = '$(CUSTOMER)_TeleoptiCCC7')
ALTER DATABASE [$(CUSTOMER)_TeleoptiCCC7] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE
GO
PRINT 'Restoring $(CUSTOMER)_TeleoptiCCC7'

RESTORE DATABASE [$(CUSTOMER)_TeleoptiCCC7]
FROM DISK = N'$(RARFOLDER)\$(CUSTOMER)_TeleoptiCCC7.BAK'
WITH  FILE = 1,
MOVE N'TeleoptiCCC7_Data' TO N'$(DATAFOLDER)\$(CUSTOMER)_TeleoptiCCC7.mdf',
MOVE N'TeleoptiCCC7_Log' TO N'$(DATAFOLDER)\$(CUSTOMER)_TeleoptiCCC7.ldf',
NOUNLOAD,
REPLACE,
STATS = 10
GO

--waitfor database to open
DECLARE @MultiUserError int
SET @MultiUserError = 1
WHILE @MultiUserError <> 0
BEGIN
	PRINT 'waiting for $(CUSTOMER)_TeleoptiCCC7 database to recover ...'
	ALTER DATABASE [$(CUSTOMER)_TeleoptiCCC7] SET  MULTI_USER
	SET @MultiUserError = @@error
	IF @MultiUserError = 0 BREAK
	WAITFOR DELAY '00:00:05.000'
END

print 'done!'
GO

--Remove wrong ApplicationFunctions
delete $(CUSTOMER)_TeleoptiCCC7.dbo.ApplicationFunctionInRole
where ApplicationFunction in ('FB00845F-FBD9-44C5-A4C1-A2CE00E48520','8B2575E0-BBE0-4401-AE6D-E3BE6C6BF254','0A1A8D3C-3773-4B03-92AB-F60335FE5C2C')

delete $(CUSTOMER)_TeleoptiCCC7.dbo.ApplicationFunction
where Id in ('FB00845F-FBD9-44C5-A4C1-A2CE00E48520','8B2575E0-BBE0-4401-AE6D-E3BE6C6BF254','0A1A8D3C-3773-4B03-92AB-F60335FE5C2C')