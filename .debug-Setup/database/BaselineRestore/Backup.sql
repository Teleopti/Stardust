BACKUP DATABASE [$(customer)_TeleoptiAnalytics]
TO DISK = N'$(BACKUPFOLDER)\$(customer)_TeleoptiAnalytics.bak'
WITH NOFORMAT, INIT,
SKIP, NOREWIND, NOUNLOAD,  STATS = 10
GO

BACKUP DATABASE [$(customer)_TeleoptiCCC7]
TO DISK = N'$(BACKUPFOLDER)\$(customer)_TeleoptiCCC7.bak'
WITH NOFORMAT, INIT,
SKIP, NOREWIND, NOUNLOAD,  STATS = 10
GO

BACKUP DATABASE [$(customer)_TeleoptiCCCAgg]
TO DISK = N'$(BACKUPFOLDER)\$(customer)_TeleoptiCCCAgg.bak'
WITH NOFORMAT, INIT,
SKIP, NOREWIND, NOUNLOAD,  STATS = 10
GO