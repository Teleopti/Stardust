/*********************************************************************************
Description:	Create a local windows groups and add it as login in SQL Server
				Create a SQL server login based on the WINLOGIN sent as input from WISE
				Give both login database access and proper permissions
				Note: The local IT needs to add End users in the local windows group at the SQL Server

Created:	2008-09-02 David Jonsson
---------------------------------
Date		Who				What
---------------------------------
2010-10-25	David Jonsson	Adding this file to WISE for the first time

********************************************************************************/
--This script is executed by DBManager when installing CCC 7 for the very first time
--You can execute this script manually later on to create the needed SQL login for the corresponding Windows account

--1) In SQL Server Management Studio
--	a) Go to Query-menu
--	b) enable "SQLCMD mode"
--2) uncomment the next section
--3) Edit the MyDomain\MyServiceAccount

/* --remove this line

--Edit the next line
:SETVAR WINLOGIN TOPTINET\CCCTestSvc

*/ --remove this line

USE master
BEGIN TRY
	
	PRINT 'Create local Windows group and corresponding SQL Login. Working ...'
	SET NOCOUNT ON
	
	--declare
	DECLARE	@ReturnCode				int
	DECLARE	@CommandShellStatus		int
	DECLARE	@AdvancedOptionsStatus	int
	DECLARE	@CmdReturnCode			int
	DECLARE	@GroupName				nvarchar(1000)
	DECLARE	@CmdString				nvarchar(1000)
	DECLARE @InstanceName			nvarchar(36)
	DECLARE @Login					nvarchar(200)
	DECLARE @SqlCommand				nvarchar(1000)
	DECLARE @sysAdmin				int
	DECLARE @securityadmin			int
	
	--init
	SET @GroupName			= 'TeleoptiCCC_Users'
	SELECT @sysAdmin		= CASE WHEN IS_SRVROLEMEMBER('sysadmin') = 1 THEN 1 ELSE 0 END
	SELECT @securityadmin	= CASE WHEN IS_SRVROLEMEMBER('securityadmin') = 1 THEN 1 ELSE 0 END
		  
	--1) First Add the Service Account as login
	IF @securityadmin = 1
	BEGIN
		PRINT '	Creating Windows login for: $(WINLOGIN). Working ...'
		
		IF @securityadmin = 1
		BEGIN
			--Create Win login if it doesn't exists
			IF NOT EXISTS (SELECT * FROM master.sys.syslogins WHERE name = '$(WINLOGIN)')
			BEGIN
				CREATE LOGIN [$(WINLOGIN)] FROM WINDOWS
				WITH DEFAULT_DATABASE=[master],
				DEFAULT_LANGUAGE=[us_english]
			END
			
			--Bulk insert is needed in the ETL-tool so login needs to be part of the server fixed role [ADMINISTER BULK OPERATIONS]
			PRINT '		Add $(WINLOGIN) to the fixed server role: [ADMINISTER BULK OPERATIONS]. Working ...'
				GRANT ADMINISTER BULK OPERATIONS TO [$(WINLOGIN)]
			PRINT '		Add $(WINLOGIN) to the fixed server role: [ADMINISTER BULK OPERATIONS]. Finished!'
		END
		PRINT '	Creating Windows login for: $(WINLOGIN). Finished'
	END
	ELSE
		PRINT 'WARNING: Cannot create server login under the currect credetials!'
		

	--2) Create local group (Windows) + Add the local a corresponding login (SQL server)
	IF @sysAdmin = 1 
	BEGIN
		PRINT '	Create a local Windows group and server logins. Working...'
			
		-- Get the original settings for the configuration options
		SELECT @AdvancedOptionsStatus	= CAST(value_in_use AS INT) FROM master.sys.configurations WHERE name = 'show advanced options'
		SELECT @CommandShellStatus		= CAST(value_in_use AS INT) FROM master.sys.configurations WHERE name = 'xp_cmdshell'

		-- Enable the usage of xp_cmdshell (temporary)
		PRINT '		Enable the usage of xp_cmdshell. Working...'
		EXEC sp_configure 'show advanced options', 1
		RECONFIGURE
		EXEC sp_configure 'xp_cmdshell', 1
		RECONFIGURE
		PRINT '		Enable the usage of xp_cmdshell. Finished!'

		--get instance name
		SELECT @InstanceName = COALESCE(CAST(SERVERPROPERTY('InstanceName') AS VARCHAR), 'MSSQLSERVER')

		--If named instance then add the instance name to groupname
		IF @InstanceName = 'MSSQLSERVER'
		BEGIN
			SELECT @Login		= CAST(SERVERPROPERTY('MachineName') AS VARCHAR) + '\' + @GroupName
			SELECT @GroupName	= @GroupName
		END
		ELSE
		BEGIN
			SELECT @Login		= CAST(SERVERPROPERTY('MachineName') AS VARCHAR) + '\' + @GroupName + '_' + @InstanceName
			SELECT @GroupName	= @GroupName + '_' + @InstanceName
		END
		
		-- Check if the group already exists
		SET @ReturnCode = 0
		SET @CmdString = 'NET LOCALGROUP "' + @GroupName + '"'
		EXEC @CmdReturnCode = master.dbo.xp_cmdshell @CmdString, no_output

		-- Create a group group for this instance if it doesn't exists
		IF (@CmdReturnCode <> 0)
		BEGIN
			PRINT '		Creating a local group in Windows: ' + @GroupName + '. Working...'
			SET @CmdString = 'NET LOCALGROUP "' + @GroupName + '" /ADD /COMMENT:"Provides database access for client users of Teleopti CCC7"'
			EXEC @ReturnCode = master.dbo.xp_cmdshell @CmdString, NO_OUTPUT
			IF (@ReturnCode <> 0) RAISERROR ('Unable to create the local Windows group in Operating system', 16, 1)
			PRINT '	Creating the local group ' + @GroupName + '. Finished!'
		END
		ELSE
			PRINT '		The local group ' + @GroupName + ' already exists. Continue...'

		-- Restore the settings for the configuration options
		PRINT 'Restore the settings for the configuration options. Working...'
		EXEC sp_configure 'xp_cmdshell', @CommandShellStatus
		RECONFIGURE
		EXEC sp_configure 'show advanced options', @AdvancedOptionsStatus
		RECONFIGURE
		PRINT 'Restore the settings for the configuration options. Finished!'
		
		-- Add the local windows group as a login in this instance
		PRINT '	Create a local Windows group and server logins. Working...'
	
		PRINT '		Add the local group as login in SQL Server. Working...'
		IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = @Login)
		BEGIN
			EXEC sp_grantlogin @Login
			EXEC sp_defaultdb @Login, 'master'
		END
		ELSE
			PRINT '	' + @Login + ' already existed. Continue...'
		PRINT '		Add the local group as login in SQL Server. Finished!'
		PRINT 'Create local Windows group and corresponding SQL Login. Finished!'

		
		--Bulk insert is needed for the ETL-tool so the login needs to be part of the server fixed role [ADMINISTER BULK OPERATIONS]
		PRINT '		Add $(WINLOGIN) to the fixed server role: [ADMINISTER BULK OPERATIONS]. Working ...'
			SELECT @SqlCommand = 'GRANT ADMINISTER BULK OPERATIONS TO [' + @Login +']'
			EXEC sp_executesql @SqlCommand
			
		PRINT '		Add $(WINLOGIN) to the fixed server role: [ADMINISTER BULK OPERATIONS]. Finished!'

	END
	ELSE
		PRINT 'WARNING: Cannot create Windows local group under the currect credetials!'
		
/*		
	-- Check if the web server Network Serivce Accoun exists in that group
	SET @ReturnCode = 0
	SET @CmdString = 'NET LOCALGROUP "' + @GroupName +'" | FINDSTR /C:"$(DOMAIN)\$(SERVICEACCOUNT)" > NUL'
	EXEC @ReturnCode = master.dbo.xp_cmdshell @CmdString, no_output


	--Add the web server Service account to the local windows group
	IF (@CmdReturnCode <> 0)
	BEGIN
		PRINT '	Adding web server Service Account to local group. Working...'
		SET @CmdString = 'NET LOCALGROUP "' + @GroupName + '" "$(DOMAIN)\$(SERVICEACCOUNT)" /ADD"'
		EXEC @ReturnCode = master.dbo.xp_cmdshell @CmdString, NO_OUTPUT
		IF (@ReturnCode <> 0) RAISERROR ('Unable to create the [$(GROUPNAME)] group.', 16, 1)
		PRINT '	Adding web server Service Account to local group. Finished!'
	END
	ELSE
		PRINT '	The Service Account already exists in group [$(GROUPNAME)]. Continue...'
*/
	
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

PRINT '---'
/*******************************************************************************/
