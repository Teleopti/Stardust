@ECHO off

SET /P Customer=Customer on RaptorWin2008: 

SET TeleoptiAnalytics_Stage=%Customer%_TeleoptiAnalytics_Stage
SET TeleoptiAnalytics=%Customer%_TeleoptiAnalytics
SET TeleoptiCCC7=%Customer%_TeleoptiCCC7
SET TeleoptiCCCAgg=%Customer%_TeleoptiCCCAgg

::Fejka BOStore
SET BOStore=%TeleoptiAnalytics_Stage%

SET MyServerInstance=RaptorWin2008

::=======================
::Init
::=======================
::Get path to this batchfile
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

::Move one level up in the folder structure
SET ROOTDIR=%ROOTDIR%\..

::Get latest Database from TFS
"C:\Program Files\Microsoft Visual Studio 9.0\Common7\IDE\tf.exe" get $/raptorScrum/Database /all /recursive

::Deploy databases
::"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiAnalytics_Stage% -E -OTeleoptiAnalytics_Stage

"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiAnalytics% -E -OTeleoptiAnalytics

"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiCCC7% -E -OTeleoptiCCC7

::Deploy all cross database views
::SQLCMD -S%MyServerInstance% -E -d%TeleoptiAnalytics% -i"%ROOTDIR%\TeleoptiAnalytics\ApplicationConfig\CrossDatabase Views - load.sql"

ECHO Done upgrading!
PAUSE

::Run the DBConverter here with correct config-file
::\\ccctestts\RaptorLatestClient\....
