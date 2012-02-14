--:SETVAR SQLLOGIN teleopti

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$(SQLLOGIN)')
DROP USER [$(SQLLOGIN)]

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$(SQLLOGIN)')
DROP USER [$(SQLLOGIN)]

CREATE USER [$(SQLLOGIN)] FOR LOGIN [$(SQLLOGIN)] WITH DEFAULT_SCHEMA=[dbo]
EXEC sp_addrolemember N'db_owner', N'$(SQLLOGIN)'