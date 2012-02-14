--Re-deploy views, since we might restore from BAK files with different database names
--Switch context
USE [TeleoptiAnalytics_Demo]

--config sys_crossdatabaseview_target
EXEC sys_crossdatabaseview_target_update 'TeleoptiAnalytics_Stage', 'TeleoptiAnalytics_Stage_Demo'
EXEC sys_crossdatabaseview_target_update 'TeleoptiAnalytics', 'TeleoptiAnalytics_Demo'
EXEC sys_crossdatabaseview_target_update 'TeleoptiCCCAgg', 'TeleoptiCCC7Agg_Demo'
EXEC sys_crossdatabaseview_target_update 'BOStore', 'TeleoptiAnalytics_Stage_Demo'

--deploy all cross database views
EXEC dbo.sys_crossdatabaseview_load

--Adding TeleoptiDemoUser
USE [TeleoptiAnalytics_Demo]
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'TeleoptiDemoUser')
DROP USER [TeleoptiDemoUser]
CREATE USER [TeleoptiDemoUser] FOR LOGIN [TeleoptiDemoUser] WITH DEFAULT_SCHEMA=[dbo]
EXEC sp_addrolemember N'db_owner', N'TeleoptiDemoUser'

USE [TeleoptiAnalytics_Stage_Demo]
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'TeleoptiDemoUser')
DROP USER [TeleoptiDemoUser]
CREATE USER [TeleoptiDemoUser] FOR LOGIN [TeleoptiDemoUser] WITH DEFAULT_SCHEMA=[dbo]
EXEC sp_addrolemember N'db_owner', N'TeleoptiDemoUser'

USE [TeleoptiMessaging_Demo]
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'TeleoptiDemoUser')
DROP USER [TeleoptiDemoUser]
CREATE USER [TeleoptiDemoUser] FOR LOGIN [TeleoptiDemoUser] WITH DEFAULT_SCHEMA=[dbo]
EXEC sp_addrolemember N'db_owner', N'TeleoptiDemoUser'

USE [TeleoptiCCC7Agg_Demo]
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'TeleoptiDemoUser')
DROP USER [TeleoptiDemoUser]
CREATE USER [TeleoptiDemoUser] FOR LOGIN [TeleoptiDemoUser] WITH DEFAULT_SCHEMA=[dbo]
EXEC sp_addrolemember N'db_owner', N'TeleoptiDemoUser'

USE [TeleoptiCCC7_Demo]
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'TeleoptiDemoUser')
DROP USER [TeleoptiDemoUser]
CREATE USER [TeleoptiDemoUser] FOR LOGIN [TeleoptiDemoUser] WITH DEFAULT_SCHEMA=[dbo]
EXEC sp_addrolemember N'db_owner', N'TeleoptiDemoUser'