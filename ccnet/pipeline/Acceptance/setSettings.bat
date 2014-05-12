::usage
::setSettings.bat [db_ccc7] [db_analytics]
@ECHO off

SETLOCAL

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-27%

set configuration=Release
SET TOGGLEURL=ALL
SET TOGGLE_FILE=ALL
SET DB_CCC7=%1
SET DB_ANALYTICS=%2
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
SET MySettings=%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\settings.txt
IF EXIST "%MySettings%" DEL "%MySettings%" /F

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
ECHO $(TOGGLE_FILE)^|%TOGGLE_FILE%>>"%MySettings%"
ECHO $(SQL_SERVER_NAME)^|%SQL_SERVER_NAME%>>"%MySettings%"

::Prepare the file(s) to be SerachedAndReplaced
if not exist "%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\ConfigFiles" mkdir "%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\ConfigFiles"
ECHO %ROOTDIR%\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\bin\%configuration%\Teleopti.Ccc.WebBehaviorTest.dll.config,%ROOTDIR%\BuildArtifacts\Teleopti.Ccc.TestCommon.App.config>%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\ConfigFiles\BuildServerConfigFiles.txt

ENDLOCAL
