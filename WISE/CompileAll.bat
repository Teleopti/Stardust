@ECHO OFF

::The path to this file
SET WISEDIR=%~dp0

::Get CCNet input
SET Version=%1
SET ProductId=%2
SET OUTDIR=%3

::Re-set errorlevel
SET errorlevel=0

::Get the number of input params
SET /a argc=0
:_argcLoop
  if [%1]==[] goto _exitargcLoop
  set /a argc+=1
  set arg%argc%=%1
  shift
  goto _argcLoop
:_exitargcLoop

::Check if we have a user or if its CCnet calling us
IF %argc%==4 (
SET Silent=1
GOTO StartProcess
)
IF %argc%==0 (
SET Silent=0
GOTO ManualInput
) ELSE (
ECHO Wrong number of input parameters!
PING 127.0.0.1 -n 6 > NUL
GOTO EOF
)

::Get user input
:ManualInput
IF "%Version%"=="" SET /P Version=Provide version to build: 
IF "%ProductId%"=="" SET /P ProductId=[All=0, Server=1, Forecast=2, AdminClient=3, MyTime=4, AzureConfigFiles=5]
GOTO StartProcess

:StartProcess
::remove trailer slash
SET WISEDIR=%WISEDIR:~0,-1%

::remove \wise
SET SRCDIR=%WISEDIR:~0,-5%

IF EXIST "%WISEDIR%\Machines\%COMPUTERNAME%.bat" (
ECHO Setting machine config
CALL "%WISEDIR%\Machines\%COMPUTERNAME%.bat"
) ELSE (
ECHO Could not find file for  machine config
SET ERRORLEVEL=99
GOTO EOF
)
IF %errorlevel% NEQ 0 (
ECHO Failed to create machine config
SET ERRORLEVEL=99
GOTO EOF
)

::This is were we put src files after successful build
SET HISTORYDIR=%DEPLOYSHARE%\%Version%

::Hardcoded drive letter used inside WISE project.
SET WISEDRIVELETTER=K:

::un-SUBST - remove virtual driveletter
ECHO. > "%temp%\SUBST.txt"
SUBST > "%temp%\SUBST.txt"
findstr /b /C:"%WISEDRIVELETTER%" "%temp%\SUBST.txt"
if %errorlevel% EQU 0 (
ECHO Un-SUBST driveletter: %WISEDRIVELETTER%
SUBST %WISEDRIVELETTER% /D
)

::SUBST - create virtual driveletter
IF EXIST %WISEDRIVELETTER% (
ECHO Drive %WISEDRIVELETTER%  probably exist as fixed drive letter. Abort!
SET ERRORLEVEL=99
EXIT %ERRORLEVEL%
) ELSE (
ECHO Adding new virtual driveletter:
ECHO SUBST %WISEDRIVELETTER% %WORKINGDIR%
SUBST %WISEDRIVELETTER% %WORKINGDIR%
)

::Drop and Create folder
IF EXIST %WORKINGDIR% (
RMDIR %WORKINGDIR% /S /Q
)
MKDIR %WORKINGDIR%

::Do NOT change this, unless you also update the wise project files!
SET WISESOURCEFILE=%WISEDRIVELETTER%\Src
SET WISEPROJFILE=%WISESOURCEFILE%\WISE
SET DYNAMICCONTENT=%WISESOURCEFILE%\WiseArtifact
SET DEPENDENCIES=%WISEDRIVELETTER%\Dependencies
ECHO WISESOURCEFILE: %WISESOURCEFILE%
ECHO WISEPROJFILE: %WISEPROJFILE%
ECHO DYNAMICCONTENT: %DYNAMICCONTENT%

::Set Product folders. note: Folder namd must correspond to .wsi-file name. See: sub batch file: CompileWise.bat
SET ccc7_server=ccc7_server
SET ccc7_forecast=ccc7_forecast
SET ccc7_client=ccc7_client
SET ccc7_mytime=ccc7_mytime
SET ccc7_azure=ccc7_azure

ECHO Start wise compile for Version=%Version%. Productid is %ProductId%
SET OUTDIR=%OUTDIR%\%Version%

::Get this release into Wise src dir
ECHO Getting files from Release into Wise Compile folder, Working ...
ROBOCOPY "%SRCDIR%" "%WISESOURCEFILE%" /MIR /XF *.pdb*
ECHO Getting files from Release into Wise Compile folder. Done!

ECHO Setting Read/Write attribute on source files, Working ...
attrib -R "%WISESOURCEFILE%\*" /S
ECHO Setting Read/Write attribute on source files, Done!

::Remove read-only attrib from WISE-project in order to compile
ECHO Setting Read/Write atribute on Wise project, Working ...
ECHO attrib -R "%WISEPROJFILE%\*" /S
attrib -R "%WISEPROJFILE%\*" /S
ECHO Setting Read/Write atribute on Wise project, Done!

::Clean and Copy Artifacts
ECHO Delete all current artifacts from Source
ECHO D:

::Switch to driveletter
%WISEDRIVELETTER%
ECHO CD "%WISESOURCEFILE%"

::--------
::Clean out unwanted files
::--------
CD "%WISESOURCEFILE%"

::Destroy Dev Log4Net.config
::FOR /R %%I IN (*log4net.config*) DO DEL %%I /F /Q

::--------
::Get None Source Controlled stuff
::--------
CALL "%WISEDIR%\GetNoneTFSFiles.bat"

::--------
::Create WISE artifact structure
::--------
::This one is needed because WISE can not handle two features sharing the same file (DynamicContent vs. any other given feature)
SET ARTIFACTDIR=%SRCDIR%\BuildArtifacts
ECHO ARTIFACTDIR: %ARTIFACTDIR%

MKDIR "%DYNAMICCONTENT%"
MKDIR "%DYNAMICCONTENT%"ApplicationConfiguration"
MKDIR "%DYNAMICCONTENT%\Analytics"
MKDIR "%DYNAMICCONTENT%\ETL"
MKDIR "%DYNAMICCONTENT%\ETL\Service"
MKDIR "%DYNAMICCONTENT%\ETL\Tool"
MKDIR "%DYNAMICCONTENT%\Client"
MKDIR "%DYNAMICCONTENT%\Client\Forecasts"
MKDIR "%DYNAMICCONTENT%\Client\StandAlone"
MKDIR "%DYNAMICCONTENT%\Client\ClickOnce"
MKDIR "%DYNAMICCONTENT%\MessageBroker"
MKDIR "%DYNAMICCONTENT%\MyTime"
MKDIR "%DYNAMICCONTENT%\MyTime\StandAlone"
MKDIR "%DYNAMICCONTENT%\MyTime\ClickOnce"
MKDIR "%DYNAMICCONTENT%\RTA"
MKDIR "%DYNAMICCONTENT%\RTATools"
MKDIR "%DYNAMICCONTENT%\RTATools\Testapplication"
MKDIR "%DYNAMICCONTENT%\RTATools\Testapplication\Web"
MKDIR "%DYNAMICCONTENT%\SDK"
MKDIR "%DYNAMICCONTENT%\ServiceBus"
MKDIR "%DYNAMICCONTENT%\PMService"
MKDIR "%DYNAMICCONTENT%\Web"

::Del Checked In and Add corresponding Dynamic Artifacts
DEL /F /Q "%WISESOURCEFILE%\ETL\Service\TeleoptiCCC7.nhib.xml"
COPY "%ARTIFACTDIR%\TeleoptiCCC7.nhib.xml" "%DYNAMICCONTENT%\ETL\Service\TeleoptiCCC7.nhib.xml"

DEL /F /Q "%WISESOURCEFILE%\ETL\Service\Teleopti.Analytics.Etl.ServiceHost.exe.config
COPY "%ARTIFACTDIR%\AppETLService.config" "%DYNAMICCONTENT%\ETL\Service\Teleopti.Analytics.Etl.ServiceHost.exe.config"

DEL /F /Q "%WISESOURCEFILE%\ETL\Tool\TeleoptiCCC7.nhib.xml"
COPY "%ARTIFACTDIR%\TeleoptiCCC7.nhib.xml" "%DYNAMICCONTENT%\ETL\Tool\TeleoptiCCC7.nhib.xml"

DEL /F /Q "%WISESOURCEFILE%\ETL\Tool\Teleopti.Analytics.Etl.ConfigTool.exe.config"
COPY "%ARTIFACTDIR%\AppETLTool.config" "%DYNAMICCONTENT%\ETL\Tool\Teleopti.Analytics.Etl.ConfigTool.exe.config"

COPY "%ARTIFACTDIR%\TeleoptiCCC7.nhib.xml" "%DYNAMICCONTENT%\Client\Forecasts\TeleoptiCCC7.nhib.xml"

DEL /F /Q "%WISESOURCEFILE%\Client\Forecasts\Teleopti.Ccc.SmartClientPortal.Shell.exe.config"
COPY "%ARTIFACTDIR%\AppForecasts.config"  "%DYNAMICCONTENT%\Client\Forecasts\Teleopti.Ccc.SmartClientPortal.Shell.exe.config"

DEL /F /Q "%WISESOURCEFILE%\Client\ClickOnce\Teleopti.Ccc.SmartClientPortal.Shell.exe.config.deploy"
COPY "%ARTIFACTDIR%\AppRaptor.config" "%DYNAMICCONTENT%\Client\ClickOnce\Teleopti.Ccc.SmartClientPortal.Shell.exe.config.deploy"

DEL /F /Q "%WISESOURCEFILE%\Client\StandAlone\Teleopti.Ccc.SmartClientPortal.Shell.exe.config"
COPY "%ARTIFACTDIR%\AppRaptor.config" "%DYNAMICCONTENT%\Client\StandAlone\Teleopti.Ccc.SmartClientPortal.Shell.exe.config"

DEL /F /Q "%WISESOURCEFILE%\MyTime\StandAlone\Teleopti.Ccc.AgentPortal.exe.config"
COPY "%ARTIFACTDIR%\AppMytime.config" "%DYNAMICCONTENT%\MyTime\StandAlone\Teleopti.Ccc.AgentPortal.exe.config"

DEL /F /Q "%WISESOURCEFILE%\MyTime\ClickOnce\Teleopti.Ccc.AgentPortal.exe.config.deploy"
COPY "%ARTIFACTDIR%\AppMytime.config" "%DYNAMICCONTENT%\MyTime\ClickOnce\Teleopti.Ccc.AgentPortal.exe.config.deploy"

DEL /F /Q "%WISESOURCEFILE%\SDK\TeleoptiCCC7.nhib.xml"
COPY "%ARTIFACTDIR%\TeleoptiCCC7.nhib.xml" "%DYNAMICCONTENT%\SDK\TeleoptiCCC7.nhib.xml"

DEL /F /Q "%WISESOURCEFILE%\MessageBroker\Teleopti.Messaging.Management.exe.config"
COPY "%ARTIFACTDIR%\Teleopti.Messaging.Management.exe.config" "%DYNAMICCONTENT%\MessageBroker\Teleopti.Messaging.Management.exe.config"

DEL /F /Q "%WISESOURCEFILE%\MessageBroker\Teleopti.Messaging.Svc.exe.config"
COPY "%ARTIFACTDIR%\Teleopti.Messaging.Svc.exe.config" "%DYNAMICCONTENT%\MessageBroker\Teleopti.Messaging.Svc.exe.config"

DEL /F /Q "%WISESOURCEFILE%\RTATools\Testapplication\Web\Teleopti.Ccc.Rta.TestApplication.exe.config"
COPY "%ARTIFACTDIR%\AppRTATestApplication.config" "%DYNAMICCONTENT%\RTATools\Testapplication\Web\Teleopti.Ccc.Rta.TestApplication.exe.config"

DEL /F /Q "%WISESOURCEFILE%\ServiceBus\Teleopti.Ccc.Sdk.ServiceBus.Host.exe.config"
COPY "%ARTIFACTDIR%\ServiceBusHost.config" "%DYNAMICCONTENT%\ServiceBus\Teleopti.Ccc.Sdk.ServiceBus.Host.exe.config"

DEL /F /Q "%WISESOURCEFILE%\ServiceBus\GeneralQueue.config"
COPY "%ARTIFACTDIR%\ServiceBusHostGeneralQueue.config" "%DYNAMICCONTENT%\ServiceBus\GeneralQueue.config"

DEL /F /Q "%WISESOURCEFILE%\ServiceBus\RequestQueue.config"
COPY "%ARTIFACTDIR%\ServiceBusHostRequestQueue.config" "%DYNAMICCONTENT%\ServiceBus\RequestQueue.config"

DEL /F /Q "%WISESOURCEFILE%\ServiceBus\DenormalizeQueue.config"
COPY "%ARTIFACTDIR%\ServiceBusHostDenormalizeQueue.config" "%DYNAMICCONTENT%\ServiceBus\DenormalizeQueue.config"

DEL /F /Q "%WISESOURCEFILE%\ServiceBus\log4net.config"
COPY "%ARTIFACTDIR%\ServiceBuslog4net.config" "%DYNAMICCONTENT%\ServiceBus\log4net.config"

DEL /F /Q "%WISESOURCEFILE%\Analytics\web.config"
COPY "%ARTIFACTDIR%\AnalyticsWeb.config" "%DYNAMICCONTENT%\Analytics\web.config"

DEL /F /Q "%WISESOURCEFILE%\PMService\web.config"
COPY "%ARTIFACTDIR%\PMWeb.config" "%DYNAMICCONTENT%\PMService\web.config"

DEL /F /Q "%WISESOURCEFILE%\
COPY "%ARTIFACTDIR%\RTAWeb.config" "%DYNAMICCONTENT%\RTA\web.config"

DEL /F /Q "%WISESOURCEFILE%\
COPY "%ARTIFACTDIR%\SDKWeb.config" "%DYNAMICCONTENT%\SDK\web.config"

DEL /F /Q "%WISESOURCEFILE%\SDK\Teleopti.Ccc.Sdk.ServiceBus.Client.config"
COPY "%ARTIFACTDIR%\ServiceBusClient.config" "%DYNAMICCONTENT%\SDK\Teleopti.Ccc.Sdk.ServiceBus.Client.config"

DEL /F /Q "%WISESOURCEFILE%\Web\web.config"
COPY "%ARTIFACTDIR%\Web.Root.Web.config" "%DYNAMICCONTENT%\Web\web.config"

DEL /F /Q "%WISESOURCEFILE%\AgentPortalWeb\TeleoptiCCC7.nhib.xml"
COPY "%ARTIFACTDIR%\TeleoptiCCC7Web.nhib.xml" "%DYNAMICCONTENT%\Web\TeleoptiCCC7.nhib.xml"

::Non-Dynamic Artifacts
COPY "%ARTIFACTDIR%\CccAppConfig.config" "%WISESOURCEFILE%\ApplicationConfiguration\CccAppConfig.exe.config"
COPY "%ARTIFACTDIR%\LegalNotice.txt" "%WISESOURCEFILE%\SDK\LegalNotice.txt"
COPY "%ARTIFACTDIR%\licensecontext.slf" "%WISESOURCEFILE%\Client\Forecasts\licensecontext.slf"
COPY "%ARTIFACTDIR%\licensecontext.slf" "%WISESOURCEFILE%\Client\StandAlone\licensecontext.slf"
COPY "%ARTIFACTDIR%\Web.MyTime.Web.config" "%WISESOURCEFILE%\Web\Areas\MyTime\Views\web.config"
COPY "%ARTIFACTDIR%\Web.Start.Web.config" "%WISESOURCEFILE%\Web\Areas\Start\Views\web.config"

::Create msi output folder
IF NOT EXIST %OUTDIR% MKDIR %OUTDIR%

::This is were the action starts
ECHO.
ECHO --------------------------------------------------------------------
ECHO Compiling msi/exe for %Version%
ECHO Note: This will take several minutes (~10). Check Task Manager for WFWI.exe
ECHO Be aware of WISE popups!
ECHO Remember to mark: Exlude all, for assembly referencies that WISE finds
ECHO --------------------------------------------------------------------

::reset ERRORLEVEL
SET ERRORLEVEL=0

::Compile
IF %ProductId%==0 (
SET ProductId=3
CALL "%WISEDIR%\CompileWise.bat"
SET ProductId=4
CALL "%WISEDIR%\CompileWise.bat"
SET ProductId=5
CALL "%WISEDIR%\CompileWise.bat"
SET ProductId=2
CALL "%WISEDIR%\CompileWise.bat"
SET ProductId=1
CALL "%WISEDIR%\CompileWise.bat"
) ELSE (
CALL "%WISEDIR%\CompileWise.bat"
)
IF %ERRORLEVEL% NEQ 0 GOTO Error

ECHO All products done, check files in out dir
IF %Silent%==0 EXPLORER "%OUTDIR%"
GOTO EOF

:VersionExist
ECHO Sorry I'm in silent mode. Abort!
SET ERRORLEVEL=99
GOTO EOF

:Error
ECHO ProductId=%ProductId% did not compile!
SET ERRORLEVEL=99
GOTO EOF

:EOF
IF %Silent%==0 PAUSE
EXIT %ERRORLEVEL%
