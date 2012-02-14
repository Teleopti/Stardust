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
:SETVAR SQLLOGIN NotUserByWindows
*/ --remove this line

BEGIN TRY
SET NOCOUNT ON
	--declare
	DECLARE @InstanceName			nvarchar(36)
	DECLARE	@GroupName				nvarchar(55)
	DECLARE @Login					nvarchar(200)
	DECLARE @SvcLogin				nvarchar(200)
	DECLARE @SqlCommand				nvarchar(1000)
	DECLARE @AuthType				char(3)

	--init
	SET @Login		= '$(SQLLOGIN)'
	SET @SvcLogin	= '$(SVCLOGIN)'
	SET @AuthType	= '$(AUTHTYPE)'
	
	SELECT 'Adding permission for $(AUTHTYPE)-login in database: ' + name FROM master.sys.databases where database_id = db_id()
	SET @GroupName	= 'TeleoptiCCC_Users'
	
	EXEC sp_changedbowner @loginame = N'sa', @map = false

	IF @AuthType = 'SQL'
	BEGIN
	--init
	
		PRINT 'Adding permission for $(SQLLOGIN) in database. Working...'
		IF '$(SQLLOGIN)' <> 'sa'  --If user like to run the application with sa, don't add the user
		BEGIN

			--Create User for Login: $(SQLLOGIN)
			IF NOT EXISTS (SELECT * FROM sys.sysusers su INNER JOIN master.sys.syslogins SL ON su.sid = sl.sid WHERE SL.name = @Login)
			SELECT @SqlCommand = 'CREATE USER [' + @Login + '] FOR LOGIN ['+@Login+']'
			PRINT @SqlCommand
			EXEC sp_executesql @SqlCommand
			
			--Add users as dbo
			IF NOT EXISTS (SELECT * FROM sys.sysusers su INNER JOIN master.sys.syslogins SL ON su.sid = sl.sid WHERE SL.name = @Login AND su.name = 'dbo')
			SELECT @SqlCommand = 'sp_addrolemember N''db_owner'', N''' +@Login+''''
			PRINT @SqlCommand
			EXEC sp_executesql @SqlCommand
				
		END
		PRINT 'Adding permission for $(SQLLOGIN) in database. Finished!'
	END
	
	IF @AuthType = 'WIN'--Windows Login
	BEGIN

		--get instance name
		SELECT @InstanceName = COALESCE(CAST(SERVERPROPERTY('InstanceName') AS VARCHAR), 'MSSQLSERVER')
		
		--If named instance add instance name to groupname
		IF @InstanceName = 'MSSQLSERVER'
			SELECT @Login		= CAST(SERVERPROPERTY('MachineName') AS VARCHAR) + '\' + @GroupName
		ELSE
			SELECT @Login		= CAST(SERVERPROPERTY('MachineName') AS VARCHAR) + '\' + @GroupName + '_' + @InstanceName

		--If named instance add instance name to groupname
		IF NOT EXISTS (SELECT * FROM sys.sysusers su INNER JOIN master.sys.syslogins SL ON su.sid = sl.sid WHERE SL.name = @Login)
		BEGIN
			PRINT 'Adding permission for '+@Login+' in database. Working...'		
			SELECT @SqlCommand = 'CREATE USER [' + @Login + '] FOR LOGIN ['+@Login+']'
			EXEC sp_executesql @SqlCommand
			PRINT 'Adding permission for '+@Login+' in database. Finished!'		
		END
		
		IF NOT EXISTS (SELECT * FROM sys.sysusers su INNER JOIN master.sys.syslogins SL ON su.sid = sl.sid WHERE SL.name = @SvcLogin)
		BEGIN
			PRINT 'Adding permission for '+@SvcLogin+' in database. Working...'		
			SELECT @SqlCommand = 'CREATE USER [' + @SvcLogin + '] FOR LOGIN ['+@SvcLogin+']'
			EXEC sp_executesql @SqlCommand
			PRINT 'Adding permission for '+@SvcLogin+' in database. Finished!'
		END
	END
	
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
	IF '$(SQLLOGIN)' <> 'sa'  --If user like to run the application with sa, don't try to add it
	BEGIN
		EXEC sp_addrolemember @rolename=N'db_executor', @membername=@Login
		IF '$(AUTHTYPE)' = 'WIN'
			EXEC sp_addrolemember @rolename=N'db_executor', @membername=@SvcLogin

		EXEC sp_addrolemember @rolename='db_datawriter', @membername=@Login
		IF '$(AUTHTYPE)' = 'WIN'
			EXEC sp_addrolemember @rolename='db_datawriter', @membername=@SvcLogin
			
		EXEC sp_addrolemember @rolename='db_datareader', @membername=@Login
		IF '$(AUTHTYPE)' = 'WIN'
				EXEC sp_addrolemember @rolename='db_datareader', @membername=@SvcLogin
		
		--Grant Exec to DBrole
		GRANT EXECUTE TO db_executor
	END
	--Remove existing dbo membership
	--EXEC sp_droprolemember @rolename='db_owner', @membername=@Login
	
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
