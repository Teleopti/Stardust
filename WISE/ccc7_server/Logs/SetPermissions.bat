@ECHO off
::SetPermssions.bat 501 1 7 "someDomain\SQLWinAuth" "someDomain\PMSvcAccount"
SET /A WindowsNT=%~1
SET /A SPLevel=%~2
SET IISVersion=%~3
SET WinSvcLogin=%~4
SET WinEtlLogin=%~5

::Use first IIS digit only
SET /A IISVersion=%IISVersion:~0,1%

SET TargetFolder=%~dp0

::remove trailer slash, this is critial to icacls!
SET TargetFolder=%TargetFolder:~0,-1%

SET BatchLogFile=%TargetFolder%\SetPermissionsSub.log

::Get driveletter for installation and systemdrive
SET DRIVELETTER=%TargetFolder:~0,2%
SET mySystemDrive=%systemdrive:~0,2%

::make uppercase
for %%a in (%DRIVELETTER%) do set DRIVELETTER=%%~da
for %%a in (%mySystemDrive%) do set mySystemDrive=%%~da

SET /A localError=0
SET BatchPath=%TargetFolder%

::Switch to drive letter
%DRIVELETTER%
::Set folder permissions for Log4Net
ECHO ============== >> "%BatchLogFile%"
date /t >> "%BatchLogFile%"
time /t >> "%BatchLogFile%"
ECHO "%BatchPath%\SetPermissionsSub.bat" "%WindowsNT%" "%SPLevel%" "%WinSvcLogin%" "%WinEtlLogin%" "%BatchPath%" >> "%BatchLogFile%"
CALL "%BatchPath%\SetPermissionsSub.bat" "%WindowsNT%" "%SPLevel%" "%WinSvcLogin%" "%WinEtlLogin%" "%BatchPath%" >> "%BatchLogFile%"
ECHO The errorcode from SetPermissionsSub is: %localError% >> "%BatchLogFile%"
ECHO ============== >> "%BatchLogFile%"

IF %errorlevel% NEQ 0 (
ECHO The errorcode from Log file permission is: %errorlevel%
ECHO Check this file and try to understand what went wrong: "%BatchLogFile%"
ECHO Possible cause is UAC ^(User Access Control^) or some local security policy
ping 127.0.0.1 -n 6 >NUL
) ELSE (
ECHO Permission on Logs folder OK: "%TargetFolder%"
ping 127.0.0.1 -n 2 >NUL
)

::Create Event Log source for all our applications
::Note: These names are hardcoded here _and_ in BuildArtifacts
"%TargetFolder%\RegisterEventLogSource.exe" "TeleoptiAnalyticsWebPortal"
"%TargetFolder%\RegisterEventLogSource.exe" "TeleoptiETLService"
"%TargetFolder%\RegisterEventLogSource.exe" "TeleoptiETLTool"
"%TargetFolder%\RegisterEventLogSource.exe" "TeleoptiRtaWebService"
"%TargetFolder%\RegisterEventLogSource.exe" "TeleoptiSdkWebService"
"%TargetFolder%\RegisterEventLogSource.exe" "Teleopti Service Bus"
"%TargetFolder%\RegisterEventLogSource.exe" "TeleoptiWebApps"
"%TargetFolder%\RegisterEventLogSource.exe" "TeleoptiWebBroker"
"%TargetFolder%\RegisterEventLogSource.exe" "TeleoptiPMService"

::Set folder permissions for ConfigurationFiles
ECHO ============== >> "%BatchLogFile%"
date /t >> "%BatchLogFile%"
time /t >> "%BatchLogFile%"
ECHO "%BatchPath%\SetPermissionsSub.bat" "%WindowsNT%" "%SPLevel%" "%WinSvcLogin%" "%WinEtlLogin%" "%BatchPath%\..\ConfigurationFiles" >> "%BatchLogFile%"
CALL "%BatchPath%\SetPermissionsSub.bat" "%WindowsNT%" "%SPLevel%" "%WinSvcLogin%" "%WinEtlLogin%" "%BatchPath%\..\ConfigurationFiles" >> "%BatchLogFile%"
ECHO The errorcode from SetPermissionsSub is: %localError% >> "%BatchLogFile%"
ECHO ============== >> "%BatchLogFile%"

IF %errorlevel% NEQ 0 (
ECHO The errorcode from Log file permission is: %errorlevel%
ECHO Check this file and try to understand what went wrong: "%BatchLogFile%"
ECHO Possible cause is UAC ^(User Access Control^) or some local security policy
ping 127.0.0.1 -n 6 >NUL
) ELSE (
ECHO Permission on ConfigurationFiles folder OK: "%BatchPath%\..\ConfigurationFiles%"
ping 127.0.0.1 -n 2 >NUL
)

::Set folder permissions for TeleoptiCCC
ECHO ============== >> "%BatchLogFile%"
date /t >> "%BatchLogFile%"
time /t >> "%BatchLogFile%"
ECHO "%BatchPath%\SetPermissionsSubForIISRoot.bat" "%WindowsNT%" "%SPLevel%" "%BatchPath%\..\TeleoptiCCC" >> "%BatchLogFile%"
CALL "%BatchPath%\SetPermissionsSubForIISRoot.bat" "%WindowsNT%" "%SPLevel%" "%BatchPath%\..\TeleoptiCCC" >> "%BatchLogFile%"
ECHO "%BatchPath%\SetPermissionsSubForIISRoot.bat" "%WindowsNT%" "%SPLevel%" "%BatchPath%\..\DatabaseInstaller" >> "%BatchLogFile%"
CALL "%BatchPath%\SetPermissionsSubForIISRoot.bat" "%WindowsNT%" "%SPLevel%" "%BatchPath%\..\DatabaseInstaller" >> "%BatchLogFile%"
ECHO The errorcode from SetPermissionsSubForIISRoot is: %localError% >> "%BatchLogFile%"
ECHO ============== >> "%BatchLogFile%"

IF %errorlevel% NEQ 0 (
ECHO The errorcode from Log file permission is: %errorlevel%
ECHO Check this file and try to understand what went wrong: "%BatchLogFile%"
ECHO Possible cause is UAC ^(User Access Control^) or some local security policy
ping 127.0.0.1 -n 6 >NUL
) ELSE (
ECHO Permission on TeleoptiCCC folder OK: "%BatchPath%\..\TeleoptiCCC%"
ping 127.0.0.1 -n 2 >NUL
)

ECHO Adding permissions to listen to ports for Stardust Manger and Node
netsh http add urlacl url=http://+:14100/ user=Everyone listen=yes

GOTO :EOF