/*

DO NOT!! run this on your production database, that will drop all users!

--manuell instructions
Run this script in both your restored/copied databases:
e.g
Bug28762_Acme_TeleoptiAnalytics
Bug28762_Acme_TeleoptiApp

See instruction for what to change below!

This script will update the tenant info in your db copy to point to MySelf, (rather that poiting back to the production DB)

*/

declare @DESTANALYTICS VARCHAR(100) = '$(DESTANALYTICS)' -- <-- Edit me e.g: Bug28762_Acme_TeleoptiAnalytics
declare @DESTUSER VARCHAR(100) = '$(DESTUSER)'  -- < -- put your new debug SQL login here
declare @DESTPWD VARCHAR(100) = '$(DESTPWD)' -- < -- put your new debug SQL password here
--========================

SET NOCOUNT ON
print '---'
declare @stmt nvarchar(max) = 'CREATE USER '+@DESTUSER+' WITH PASSWORD = '''+@DESTPWD+''''

declare @ApplicationConnectionString nVARCHAR(200)
declare @AnalyticsConnectionString nVARCHAR(200)

select @ApplicationConnectionString = 'Data Source='+@@servername+'.database.windows.net;Initial Catalog='+DB_NAME()+';User ID='+@DESTUSER+';Password='+@DESTPWD+';Current Language=us_english'
select @AnalyticsConnectionString = 'Data Source='+@@servername+'.database.windows.net;Initial Catalog='+@DESTANALYTICS+';User ID='+@DESTUSER+';Password='+@DESTPWD+';Current Language=us_english'

if exists (select * from sys.tables t join sys.schemas s on (t.schema_id = s.schema_id) where s.name = 'tenant' and t.name = 'tenant')
BEGIN
	
	update tenant.tenant
	set
		ApplicationConnectionString=@ApplicationConnectionString,
		AnalyticsConnectionString=@AnalyticsConnectionString
	where Name = 'Teleopti WFM'
END

DECLARE @username VARCHAR(64)
DECLARE @dropUser nvarchar(max)
  DECLARE c1 CURSOR FOR 
    SELECT name   
    FROM sysusers
    WHERE name NOT IN('dbo','guest','INFORMATION_SCHEMA','sys','public')
      AND LEFT(name,3) <> 'db_'
  OPEN c1
  FETCH next FROM c1 INTO @username
  WHILE @@fetch_status <> -1
   BEGIN
      PRINT 'Dropping ' + @username
	  SELECT @dropUser = 'DROP USER '+@username
		exec sp_executesql @dropUser
     FETCH next FROM c1 INTO @username
   END
CLOSE c1
DEALLOCATE c1

--create new user
exec sp_executesql @stmt

/********************************************************************************
$Revision: 
$Archive: 
$Author: 
$Date: 
$Modtime: 
$Workfile: 
*********************************************************************************
Description: Adds user and db_owner

Created:	 2008-09-02 David Jonsson
ChangedLog:
---------------------------------
Date		Who				What
---------------------------------
2010-10-25	David Jonsson	Adding Svc-Account for Windows Auth
2012-11-06	David Jonsson	use only one login
2012-11-28	David Jonsson	Create DBUser from DBManager, merge Azure permission into this script
********************************************************************************/
--This script is executed by DBManager when installing CCC 7 for the very first time
--You can execute this script manually later on to create the needed SQL user.

--Prerequisite: "Win Logins - Create.sql"
--Note: You need to execute this script 3 times:
-- => TelepotiCCC7 + TelepotiCCCAgg + TelepotiAnalytics

--1) In SQL Server Management Studio
--	a) Go to Query-menu
--	b) enable "SQLCMD mode"
--2) uncomment the section below
--3) Edit the MyDomain\MyServiceAccount to match a domain account in your domain

/* --remove this line
--Edit next line
:SETVAR SVCLOGIN MyDomain\MyServiceAccount

--Keep next 2 lines unchanged!
:SETVAR AUTHTYPE WIN
:SETVAR LOGIN NotUserByWindows
*/ --remove this line

BEGIN TRY
SET NOCOUNT ON
	--declare
	DECLARE @Login					nvarchar(200)
	DECLARE @SqlCommand				nvarchar(1000)

	--init
	SET @Login		= @DESTUSER

	-------------
	--Permissions
	-------------
	IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'db_executor' AND type = 'R')
	CREATE ROLE [db_executor] AUTHORIZATION [dbo]

	--Add login to DBrole
	EXEC sp_addrolemember @rolename=N'db_executor', @membername=@Login
	EXEC sp_addrolemember @rolename='db_datawriter', @membername=@Login
	EXEC sp_addrolemember @rolename='db_datareader', @membername=@Login
	
	--Grant Exec to DBrole
	GRANT EXECUTE TO db_executor
	
	PRINT 'DONE!'

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

PRINT	'---'

