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
	SET @Login		= '$(LOGIN)'

	-------------
	--Permissions
	-------------
	--Drop existing role:  N'db_executor'
	DECLARE @RoleName sysname
	set @RoleName = N'db_executor'
	IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = @RoleName AND type = 'R')
	Begin
		DECLARE @RoleMemberName sysname
		DECLARE Member_Cursor CURSOR FOR
		select [name]
		from sys.database_principals 
		where principal_id in ( 
			select member_principal_id 
			from sys.database_role_members 
			where role_principal_id in (
				select principal_id
				FROM sys.database_principals where [name] = @RoleName  AND type = 'R' ))

		OPEN Member_Cursor;

		FETCH NEXT FROM Member_Cursor
		into @RoleMemberName

		WHILE @@FETCH_STATUS = 0
		BEGIN

			exec sp_droprolemember @rolename=@RoleName, @membername= @RoleMemberName

			FETCH NEXT FROM Member_Cursor
			into @RoleMemberName
		END;

		CLOSE Member_Cursor;
		DEALLOCATE Member_Cursor;
	End

	IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'db_executor' AND type = 'R')
	DROP ROLE [db_executor]

	--Add DBrole
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
