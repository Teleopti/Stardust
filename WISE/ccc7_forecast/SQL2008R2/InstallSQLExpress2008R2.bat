@ECHO OFF 
CLS 
SET INSTANCENAME=TELEOPTIFORECAST
SET SECURITYMODE=SQL
SET SAPWD=38745jj#!Q_g
SET SQLCOLLATION=SQL_Latin1_General_Cp1_CI_AS
SET ServiceName=mssql$TeleoptiForecast
SET SetupFolder=%~1

::check if service already exist
sc query %ServiceName% | findstr /C:"state" /I > NUL
IF %errorlevel% EQU 0 GOTO ServiceExist

::install
SET MYERRLEVEL=0
ECHO Trying to install SQL Server 2008 R2 EXPRESS Edition 
"%SetupFolder%\Setup.exe" /qs /ACTION=Install /FEATURES=SQLENGINE /INSTANCENAME=%INSTANCENAME% /SQLSVCACCOUNT="NT AUTHORITY\SYSTEM" /SQLSVCStartupType=Automatic /SQLSYSADMINACCOUNTS="BUILTIN\ADMINISTRATORS" "%USERDOMAIN%\%USERNAME%" /SECURITYMODE=%SECURITYMODE% /SAPWD="%SAPWD%" /SQLCOLLATION="%SQLCOLLATION%" /IAcceptSQLServerLicenseTerms=True
SET MYERRLEVEL=%ERRORLEVEL%

::check some special error codes
IF %MYERRLEVEL%==3010 GOTO NeedReboot 
IF %MYERRLEVEL%==1641 GOTO NeedReboot

::else fail on everything <> 0
IF %MYERRLEVEL% NEQ 0 GOTO ShowError 

::service should now exist
sc query %ServiceName% | findstr /C:"state" /I > NUL
IF %errorlevel% NEQ 0 GOTO ServiceNotExist

::Done
ECHO Done!
GOTO Finished 

:ServiceNotExist
ECHO The SQL service name did not install correctly (%ServiceName%). Review the error logs!
GOTO ShowError

:ShowError 
ECHO %MYERRLEVEL% 
 
IF EXIST "%programFiles(x86)%" ( 
EXPLORER "%programFiles(x86)%\Microsoft SQL Server\100\Setup Bootstrap\Log" 
NOTEPAD "%programFiles(x86)%\Microsoft SQL Server\100\Setup Bootstrap\Log\Summary.txt" 
) ELSE ( 
EXPLORER "%programFiles%\Microsoft SQL Server\100\Setup Bootstrap\Log" 
NOTEPAD "%programFiles%\Microsoft SQL Server\100\Setup Bootstrap\Log\Summary.txt" 
) 
GOTO Finished 
:NeedReboot 
ECHO You need to reboot before you can continue and install Teleopti CCC! 
GOTO Finished

:ServiceExist
ECHO The SQL service name already exist (%ServiceName%). Stopping this script!
GOTO Finished

:Finished
PAUSE