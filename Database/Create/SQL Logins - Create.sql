-- =============================================
-- Author:		DJ
-- Create date: 2008-08-09
-- Description:	Add needed SQL Logins (Needs Mixed Mode!)
-- =============================================
/*
	:SETVAR SQLLOGIN TeleoptiCCC
	:SETVAR SQLPWD devpwd
*/

BEGIN TRY
USE master

--declare
DECLARE @securityadmin			int

--init
	SELECT @securityadmin	= CASE WHEN IS_SRVROLEMEMBER('securityadmin') = 1 THEN 1 ELSE 0 END

	IF @securityadmin = 1
	BEGIN
		PRINT 'Creating login: $(SQLLOGIN). Working ...'
		
		IF '$(SQLLOGIN)' <> 'sa'  --If user like to run the application with sa, don't add the login.
		BEGIN
			--Create login if it doesn't exists
			IF NOT EXISTS (SELECT * FROM syslogins WHERE name = '$(SQLLOGIN)')
			BEGIN	
				CREATE LOGIN [$(SQLLOGIN)]
				WITH PASSWORD=N'$(SQLPWD)',
				DEFAULT_DATABASE=[master],
				DEFAULT_LANGUAGE=[us_english],
				CHECK_EXPIRATION=OFF,
				CHECK_POLICY=OFF
			END
			
			--Bulk insert is needed in the ETL-tool so login needs to be part of the server fixed role [ADMINISTER BULK OPERATIONS]
			PRINT 'Add $(SQLLOGIN) to the fixed server role: [ADMINISTER BULK OPERATIONS]. Working ...'
				GRANT ADMINISTER BULK OPERATIONS TO [$(SQLLOGIN)]
			PRINT 'Add $(SQLLOGIN) to the fixed server role: [ADMINISTER BULK OPERATIONS]. Finished!'
		END
	END
	ELSE
		PRINT 'WARNING: Cannot create server login under the currect credetials!'
	
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

PRINT 'Creating login: $(SQLLOGIN). Finished!'

PRINT '---'