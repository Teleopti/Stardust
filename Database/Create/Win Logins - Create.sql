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
	
	SET NOCOUNT ON
	
	--declare
	DECLARE @Login					nvarchar(200)
	DECLARE @securityadmin			int
	
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
		END
		PRINT '	Creating Windows login for: $(WINLOGIN). Finished'
	END
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
