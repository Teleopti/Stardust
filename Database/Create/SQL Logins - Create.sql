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
	BEGIN
		IF '$(SQLLOGIN)' <> 'sa'  --If user like to run the application with sa, don't add the login.
		BEGIN
		PRINT 'Creating login: $(SQLLOGIN). Working ...'
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
			ELSE
			BEGIN
				ALTER LOGIN [$(SQLLOGIN)] WITH PASSWORD=N'$(SQLPWD)'
			END
		PRINT 'Creating login: $(SQLLOGIN). Finished!'
		END
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