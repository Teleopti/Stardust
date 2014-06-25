set version=7.1.330.48173

::-------------
set SOURCEUSER=bcpUser
set SOURCEPWD=abc123456

set DESTSERVER=Antonov\SQL2005Dev
set DESTUSER=TeleoptiDemoUser
set DESTPWD=TeleoptiDemoPwd2
set DESTANALYTICS=TeleoptiAnalytics_Demo
set SRCANALYTICS=TeleoptiAnalytics_Demo
set DESTCCC7=TeleoptiCCC7_Demo
set SRCCCC7=TeleoptiCCC7_Demo
set SRCAGG=TeleoptiCCC7Agg_Demo
set DESTAGG=TeleoptiCCC7Agg_Demo

set AZUREADMINUSER=sa
set AZUREADMINPWD=cadadi

::--------------
set workingdir=c:\temp\AzureRestore
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%

::--------
::Get source files for this version
::--------

::stop any local running Demo installation (might messup data)
net stop TeleoptiETLService
net stop TeleoptiBrokerService
net stop TeleoptiServiceBus

::Get files needed
::ROBOCOPY "\\hebe\Installation\Dependencies\ccc7_server\DemoDatabase" "%workingdir%\DatabaseInstaller\DemoDatabase" *.bak
::ROBOCOPY "\\hebe\Installation\PreviousBuilds\%version%\DemoDatabase" "%workingdir%\DatabaseInstaller\DemoDatabase" /E
::ROBOCOPY "\\hebe\Installation\PreviousBuilds\%version%\Database" "%workingdir%\DatabaseInstaller" /E


::--------
::Local database
::--------
::Restore Demo to Local SQL Server
SQLCMD -S. -E -v BakDir = "%workingdir%\DatabaseInstaller\DemoDatabase" -i"%workingdir%\DatabaseInstaller\DemoDatabase\RestoreDemo.sql"
SQLCMD -S. -E -v BakDir = "%workingdir%\DatabaseInstaller\DemoDatabase" -i"%workingdir%\DatabaseInstaller\DemoDatabase\RestoreUsers.sql" -v CurrentUser=""

::Patch local database
"%workingdir%\DatabaseInstaller\DBManager.exe" -S. -D%SRCANALYTICS% -OTeleoptiAnalytics -E -T
"%workingdir%\DatabaseInstaller\DBManager.exe" -S. -D%SRCCCC7% -OTeleoptiCCC7 -E -T
"%workingdir%\DatabaseInstaller\DBManager.exe" -S. -D%SRCAGG% -OTeleoptiCCCAgg -E -T

::encrypt Pwd in local database
"%workingdir%\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe" -DS. -DD%SRCCCC7% -EE

::Generate BCP in+out batch files
SQLCMD -S. -E -d%SRCCCC7% -i"%ROOTDIR%\DropCircularFKs.sql"
SQLCMD -S. -E -d%SRCCCC7% -i"%ROOTDIR%\GenerateBCPStatements.sql" -v DESTDB = "%DESTCCC7%" WORKINGDIR = "%workingdir%" SOURCEUSER = "%SOURCEUSER%" SOURCEPWD = "%SOURCEPWD%" DESTSERVER = "tcp:%DESTSERVER%" DESTUSER = "%DESTUSER%" DESTPWD = "%DESTPWD%"
SQLCMD -S. -E -d%SRCCCC7% -i"%ROOTDIR%\CreateCircularFKs.sql"
SQLCMD -S. -E -d%SRCANALYTICS% -i"%ROOTDIR%\GenerateBCPStatements.sql" -v DESTDB = "%DESTANALYTICS%" WORKINGDIR = "%workingdir%" SOURCEUSER = "%SOURCEUSER%" SOURCEPWD = "%SOURCEPWD%" DESTSERVER = "tcp:%DESTSERVER%" DESTUSER = "%DESTUSER%" DESTPWD = "%DESTPWD%"
SQLCMD -S. -E -d%SRCAGG% -i"%ROOTDIR%\GenerateBCPStatements.sql" -v DESTDB = "%DESTAGG%" WORKINGDIR = "%workingdir%" SOURCEUSER = "%SOURCEUSER%" SOURCEPWD = "%SOURCEPWD%" DESTSERVER = "tcp:%DESTSERVER%" DESTUSER = "%DESTUSER%" DESTPWD = "%DESTPWD%"
PAUSE

::Execute bcp export from local databases
CMD /C "%workingdir%\%SRCANALYTICS%\Out.bat"
CMD /C "%workingdir%\%SRCCCC7%\Out.bat"
CMD /C "%workingdir%\%SRCAGG%\Out.bat"

::--------
::SQL Azure
::--------
::Drop current Demo in Azure
echo dropping Azure Dbs...
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER% -U%AZUREADMINUSER% -P%AZUREADMINPWD% -dmaster -Q"DROP DATABASE %DESTANALYTICS%"
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER% -U%AZUREADMINUSER% -P%AZUREADMINPWD% -dmaster -Q"DROP DATABASE %DESTCCC7%"
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER% -U%AZUREADMINUSER% -P%AZUREADMINPWD% -dmaster -Q"DROP DATABASE %DESTAGG%"
echo dropping Azure. Done!

::Create Azure Demo
"%workingdir%\DatabaseInstaller\DBManager.exe" -S%DESTSERVER% -D%DESTANALYTICS% -OTeleoptiAnalytics -U%AZUREADMINUSER% -P%AZUREADMINPWD% -C -L%DESTUSER%:%DESTPWD%
"%workingdir%\DatabaseInstaller\DBManager.exe" -S%DESTSERVER% -D%DESTCCC7% -OTeleoptiCCC7 -U%AZUREADMINUSER% -P%AZUREADMINPWD% -C -L%DESTUSER%:%DESTPWD%
"%workingdir%\DatabaseInstaller\DBManager.exe" -S%DESTSERVER% -D%DESTAGG% -OTeleoptiCCCAgg -U%AZUREADMINUSER% -P%AZUREADMINPWD% -C -L%DESTUSER%:%DESTPWD%
PAUSE
::Prepare Azure DB = totally clean in out!
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER% -P%DESTPWD% -d%DESTCCC7% -i"%ROOTDIR%\DropCircularFKs.sql"
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER% -P%DESTPWD% -d%DESTCCC7% -i"%ROOTDIR%\DeleteAllData.sql"
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER% -P%DESTPWD% -d%DESTCCC7% -i"%ROOTDIR%\CreateCircularFKs.sql"
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER% -P%DESTPWD% -d%DESTANALYTICS% -i"%ROOTDIR%\DeleteAllData.sql"
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER% -P%DESTPWD% -d%DESTAGG% -i"%ROOTDIR%\DeleteAllData.sql"

::Import To Azure Demo
CMD /C "%workingdir%\%SRCANALYTICS%\In.bat"
if exist "%workingdir%\%SRCANALYTICS%\Logs\*.log" EXPLORER "%workingdir%\%SRCANALYTICS%\Logs"

CMD /C "%workingdir%\%SRCCCC7%\in.bat"
if exist "%workingdir%\%SRCCCC7%\Logs\*log" EXPLORER "%workingdir%\%SRCCCC7%\Logs"

CMD /C "%workingdir%\%SRCAGG%\in.bat"
if exist "%workingdir%\%SRCAGG%\Logs\*log" EXPLORER "%workingdir%\%SRCAGG%\Logs"

::------------
::Done
::------------
PAUSE
GOTO :eof