SET version=%1

IF "%version%"=="" GOTO :noinput

::-------------
set SOURCEUSER=bcpUser
set SOURCEPWD=abc123456

set PREVIOUSBUILD=\\hebe\Installation\PreviousBuilds
set DESTSERVER=s8v4m110k9.database.windows.net
set DESTSERVERPREFIX=s8v4m110k9
set DESTUSER=TeleoptiDemoUser
set DESTPWD=TeleoptiDemoPwd2
set DESTANALYTICS=Demo_TeleoptiAnalytics
set SRCANALYTICS=TeleoptiAnalytics_Demo
set DESTCCC7=Demo_TeleoptiCCC7
set SRCCCC7=TeleoptiCCC7_Demo
set SRCAGG=TeleoptiCCC7Agg_Demo

set AZUREADMINUSER=teleopti
set AZUREADMINPWD=T3l30pt1

::--------------
set workingdir=c:\temp\AzureRestore
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%

::--------
::Get source files for this version
::--------
if exist "%workingdir%" RMDIR "%workingdir%" /S /Q
mkdir "%workingdir%"

::stop any local running Demo installation (might messup data)
net stop TeleoptiETLService
net stop TeleoptiBrokerService
net stop TeleoptiServiceBus

::Get files needed
ROBOCOPY "\\hebe\Installation\Dependencies\ccc7_server\DemoDatabase" "%workingdir%\DatabaseInstaller\DemoDatabase" *.bak
ROBOCOPY "%PREVIOUSBUILD%\%version%\DemoDatabase" "%workingdir%\DatabaseInstaller\DemoDatabase" /E
ROBOCOPY "%PREVIOUSBUILD%\%version%\Database" "%workingdir%\DatabaseInstaller" /E

::--------
::Local database
::--------
::Restore Demo to Local SQL Server
SQLCMD -S. -E -v BakDir = "%workingdir%\DatabaseInstaller\DemoDatabase" -i"%workingdir%\DatabaseInstaller\DemoDatabase\RestoreDemo.sql"
SQLCMD -S. -E -v BakDir = "%workingdir%\DatabaseInstaller\DemoDatabase" -i"%workingdir%\DatabaseInstaller\DemoDatabase\RestoreUsers.sql"

::Patch local database
"%workingdir%\DatabaseInstaller\DBManager.exe" -S. -D%SRCANALYTICS% -OTeleoptiAnalytics -E -T
"%workingdir%\DatabaseInstaller\DBManager.exe" -S. -D%SRCCCC7% -OTeleoptiCCC7 -E -T
"%workingdir%\DatabaseInstaller\DBManager.exe" -S. -D%SRCAGG% -OTeleoptiCCCAgg -E -T

::encrypt Pwd in local database
"%workingdir%\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe" -DS. -DD%SRCCCC7% -EE

::Prepare (clean out and prepare data) in local database
SQLCMD -S. -E -d%SRCCCC7% -i"%ROOTDIR%\TeleoptiCCC7-PrepareData.sql"
SQLCMD -S. -E -d%SRCANALYTICS% -i"%ROOTDIR%\TeleoptiAnalytics-PrepareData.sql"

::Generate BCP in+out batch files
SQLCMD -S. -E -d%SRCCCC7% -i"%ROOTDIR%\DropCircularFKs.sql"
SQLCMD -S. -E -d%SRCCCC7% -i"%ROOTDIR%\GenerateBCPStatements.sql" -v DESTDB = "%DESTCCC7%" WORKINGDIR = "%workingdir%" SOURCEUSER = "%SOURCEUSER%" SOURCEPWD = "%SOURCEPWD%" DESTSERVER = "tcp:%DESTSERVER%" DESTUSER = "%DESTUSER%@%DESTSERVERPREFIX%" DESTPWD = "%DESTPWD%"
SQLCMD -S. -E -d%SRCCCC7% -i"%ROOTDIR%\CreateCircularFKs.sql"
SQLCMD -S. -E -d%SRCANALYTICS% -i"%ROOTDIR%\GenerateBCPStatements.sql" -v DESTDB = "%DESTANALYTICS%" WORKINGDIR = "%workingdir%" SOURCEUSER = "%SOURCEUSER%" SOURCEPWD = "%SOURCEPWD%" DESTSERVER = "tcp:%DESTSERVER%" DESTUSER = "%DESTUSER%@%DESTSERVERPREFIX%" DESTPWD = "%DESTPWD%"

::Execute bcp export from local databases
CMD /C "%workingdir%\%SRCANALYTICS%\Out.bat"
CMD /C "%workingdir%\%SRCCCC7%\Out.bat"

::--------
::SQL Azure
::--------
::Drop current Demo in Azure
echo dropping Azure Dbs...
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -P%AZUREADMINPWD% -dmaster -Q"DROP DATABASE %DESTANALYTICS%"
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -P%AZUREADMINPWD% -dmaster -Q"DROP DATABASE %DESTCCC7%"
echo dropping Azure. Done!

::Create Azure Demo
"%workingdir%\DatabaseInstaller\DBManager.exe" -S%DESTSERVER% -D%DESTANALYTICS% -OTeleoptiAnalytics -U%AZUREADMINUSER% -P%AZUREADMINPWD% -C -L%DESTUSER%:%DESTPWD%
"%workingdir%\DatabaseInstaller\DBManager.exe" -S%DESTSERVER% -D%DESTCCC7% -OTeleoptiCCC7 -U%AZUREADMINUSER% -P%AZUREADMINPWD% -C -L%DESTUSER%:%DESTPWD%

::Prepare Azure DB = totally clean in out!
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -d%DESTCCC7% -i"%ROOTDIR%\DropCircularFKs.sql"
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -d%DESTCCC7% -i"%ROOTDIR%\DeleteAllData.sql"
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -d%DESTCCC7% -i"%ROOTDIR%\CreateCircularFKs.sql"
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -d%DESTANALYTICS% -i"%ROOTDIR%\DeleteAllData.sql"

::Import To Azure Demo
CMD /C "%workingdir%\%SRCANALYTICS%\In.bat"
if exist "%workingdir%\%SRCANALYTICS%\Logs\*.log" EXPLORER "%workingdir%\%SRCANALYTICS%\Logs"

CMD /C "%workingdir%\%SRCCCC7%\in.bat"
if exist "%workingdir%\%SRCCCC7%\Logs\*log" EXPLORER "%workingdir%\%SRCCCC7%\Logs"

::Re-add Agg-views in Azure
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -d%DESTANALYTICS% -Q"update mart.sys_crossdatabaseview_target set confirmed = 1"
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -d%DESTANALYTICS% -Q"exec mart.sys_crossDatabaseView_load"

::Add ETL schedules and extra statistics
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -d%DESTANALYTICS% -i"%ROOTDIR%\EtlScheduleAndExtraStats.sql"

::------------
::Done
::------------
PAUSE
GOTO :eof

:noinput
ECHO Sorry, you need to provide a version!
ping 127.0.0.1 -n 5 > NUL
exit /b
