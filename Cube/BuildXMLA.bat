@ECHO off

::Init some static paths
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

SET IDEBUILD=Microsoft Visual Studio 9.0\Common7\IDE
SET IDETF=Microsoft Visual Studio 10.0\Common7\IDE
SET XmlaFile=%ROOTDIR%\XMLA\CreateDatabase.xmla

::Set path to Visual Studio Team Foundation
IF %PROCESSOR_ARCHITECTURE% == AMD64 (
ECHO OS is 64bit
SET AsDBDeploy="%ProgramFiles(x86)%\Microsoft SQL Server\100\Tools\Binn\VSShell\Common7\IDE\Microsoft.AnalysisServices.Deployment.exe"
SET DEVENV="%ProgramFiles(x86)%\%IDEBUILD%\devenv.exe"
) ELSE (
ECHO OS is 32bit
SET AsDBDeploy="%ProgramFiles%\Microsoft SQL Server\100\Tools\Binn\VSShell\Common7\IDE\Microsoft.AnalysisServices.Deployment.exe"
SET DEVENV="%ProgramFiles%\%IDEBUILD%\devenv.exe"
)

SET Project=%ROOTDIR%\TeleoptiAnalytics
SET Solution=TeleoptiAnalytics.sln

%DEVENV% "%Project%\%Solution%" /rebuild CreateDatabase2005 /out "%ROOTDIR%\BuildOutputLog.log"

%AsDBDeploy% "%Project%\bin\TeleoptiAnalytics.asdatabase" /d /o:"%XmlaFile%"

PAUSE
cscript replace.vbs "Provider=SQLNCLI10.1;Data Source=.;Integrated Security=SSPI;Initial Catalog=TeleoptiAnalytics_Demo" "$(SQL_CONN_STRING)" "%XmlaFile%"
cscript replace.vbs "TeleoptiAnalytics" "$(SQL_DATABASE_NAME)" "%XmlaFile%"

CLS
ECHO Remember to check in the XMLA FILE
PAUSE