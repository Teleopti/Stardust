set configuration=Release
set src=%~1
set target=%~2
set localDebug=%3
::=============
::build server
::=============
if %localDebug% equ 1 (
set src=C:\data\main
set target=c:\temp\localPipeLine
ECHO "%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" "%src%\CruiseControl.sln" /t:Build /P:configuration=%configuration%
"%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" "%src%\CruiseControl.sln" /t:Build /P:configuration=%configuration%
)

::add fixed encryption/decryption keys to match the ones used in WebTest
SET validationKey=754534E815EF6164CE788E521F845BA4953509FA45E321715FDF5B92C5BD30759C1669B4F0DABA17AC7BBF729749CE3E3203606AB49F20C19D342A078A3903D1
SET decryptionKey=3E1AD56713339011EB29E98D1DF3DBE1BADCF353938C3429
ECHO %validationKey%>"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%configuration%\validation.key"
ECHO %decryptionKey%>"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%configuration%\decryption.key"

ROBOCOPY "%src%" "%target%" /MIR

cd "%target%"
.nuget\nuget.exe install .nuget\packages.config -o packages -source "http://hestia/nuget";"https://nuget.org/api/v2"
echo %target%

SET TOGGLEURL=ALL
SET DB_CCC7=a
SET DB_ANALYTICS=b
SET AS_DATABASE=%AnalyticsDB%
SET SQL_SERVER_NAME=.
SET AS_SERVER_NAME=%SQL_SERVER_NAME%
SET SQL_AUTH_STRING=Data Source=%SQL_SERVER_NAME%;Integrated Security=SSPI
SET AGENTPORTALWEB_nhibConfPath=bin
SET nhibFolder=c:\temp\nhib\notUsedByAnyTestButWillFail
SET SDK_nhibConfPath=%nhibFolder%
SET ETL_SERVICE_nhibConfPath=%nhibFolder%
SET ETL_TOOL_nhibConfPath=%nhibFolder%

::the settings file
SET MySettings=Teleopti.Support.Tool\bin\%configuration%\settings.txt
DEL "%MySettings%" /F

::hardcoded parameters for config
ECHO $(IIS_AUTH)^|Windows>>"%MySettings%"
ECHO $(LOCAL_WIKI)^|c:\temp>>"%MySettings%"
ECHO $(PM_ASMX)^|http://localhost/analyzer/services/Analyzer2005.asmx>>"%MySettings%"
ECHO $(PM_AUTH_MODE)^|Windows>>"%MySettings%"
ECHO $(PM_ANONYMOUS_DOMAINUSER)^|YourDomain\theUser>>"%MySettings%"
ECHO $(PM_ANONYMOUS_PWD)^|pw>>"%MySettings%"
ECHO $(PM_INSTALL)^|False>>"%MySettings%"
ECHO $(PM_SERVICE)^|http://localhost:53396/PMService.svc>>"%MySettings%"
ECHO $(SDK_SSL_SECURITY_MODE)^|TransportCredentialOnly>>"%MySettings%"
ECHO $(SDK_CRED_PROT)^|Ntlm>>"%MySettings%"
ECHO $(AGENT_SERVICE)^|http://localhost:1335/TeleoptiCccSdkService.svc>>"%MySettings%"
ECHO $(RTA_SERVICE)^|http://localhost:52130/TeleoptiRtaService.svc>>"%MySettings%"
ECHO $(RTA_QUEUE_ID)^|2001,2002,0063,2000,0019,0068,0085,0202,0238,2003>>"%MySettings%"
ECHO $(RTA_STATE_CODE)^|Ready,InCall,ACW,AUX1,AUX2,AUX3,AUX4,AUX5>>"%MySettings%"
ECHO $(HTTPGETENABLED)^|true>>"%MySettings%"
ECHO $(HTTPSGETENABLED)^|false>>"%MySettings%"
ECHO $(SDK_SSL_MEX_BINDING)^|mexHttpBinding>>"%MySettings%"
ECHO $(MATRIX_WEB_SITE_URL)^|http://localhost:52500>>"%MySettings%"
ECHO $(LocalWiki)^|http://localhost/TeleoptiCCC/LocalWiki/>>"%MySettings%"
ECHO $(WEB_BROKER_FOR_WEB)^|http://localhost:52858>>"%MySettings%"
ECHO $(WEB_BROKER)^|http://localhost:54903/>>"%MySettings%"
ECHO $(WEB_BROKER_BACKPLANE)^|>>"%MySettings%"
ECHO $(DATASOURCE_NAME)^|TestData>>"%MySettings%"

::dynamic parameters for config eg. used via InifileHelper
ECHO $(SQL_AUTH_STRING)^|%SQL_AUTH_STRING%>>"%MySettings%"
ECHO $(AS_SERVER_NAME)^|%AS_SERVER_NAME%>>"%MySettings%"
ECHO $(SDK_nhibConfPath)^|%SDK_nhibConfPath%>>"%MySettings%"
ECHO $(ETL_SERVICE_nhibConfPath)^|%ETL_SERVICE_nhibConfPath%>>"%MySettings%"
ECHO $(ETL_TOOL_nhibConfPath)^|%ETL_TOOL_nhibConfPath%>>"%MySettings%"
ECHO $(AGENTPORTALWEB_nhibConfPath)^|%AGENTPORTALWEB_nhibConfPath%>>"%MySettings%"
ECHO $(DB_CCC7)^|%DB_CCC7%>>"%MySettings%"
ECHO $(DB_ANALYTICS)^|%DB_ANALYTICS%>>"%MySettings%"
ECHO $(AS_DATABASE)^|%AS_DATABASE%>>"%MySettings%"
ECHO $(TOGGLE_URL)^|%TOGGLE_URL%>>"%MySettings%"
ECHO $(SQL_SERVER_NAME)^|%SQL_SERVER_NAME%>>"%MySettings%"


::Prepare the file(s) to be SerachedAndReplaced
mkdir Teleopti.Support.Tool\bin\%configuration%\ConfigFiles
ECHO ..\..\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\bin\%configuration%\Teleopti.Ccc.WebBehaviorTest.dll.config,BuildArtifacts\Teleopti.Ccc.TestCommon.App.config>Teleopti.Support.Tool\bin\%configuration%\ConfigFiles\BuildServerConfigFiles.txt

Teleopti.Support.Tool\bin\%configuration%\Teleopti.Support.Tool.exe -MOTest

ECHO packages\NUnit.Runners.2.6.2\tools\nunit-console.exe "Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\bin\%configuration%\Teleopti.Ccc.WebBehaviorTest.dll"
packages\NUnit.Runners.2.6.2\tools\nunit-console.exe "Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\bin\%configuration%\Teleopti.Ccc.WebBehaviorTest.dll"
PAUSE

