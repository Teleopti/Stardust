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

::Switch to drive letter
%DRIVELETTER%
ECHO ============== >> "%BatchLogFile%"
date /t >> "%BatchLogFile%"
time /t >> "%BatchLogFile%"
CALL "%TargetFolder%\SetPermissionsSub.bat" "%WindowsNT%" "%SPLevel%" "%WinSvcLogin%" "%WinEtlLogin%" "%TargetFolder%" >> "%BatchLogFile%"
ECHO The errorcode from SetPermissionsSub is: %localError% >> "%BatchLogFile%"
ECHO ============== >> "%BatchLogFile%"

IF %errorlevel% NEQ 0 (
ECHO The errorcode from Log file permission is: %errorlevel%
ECHO Check this file and try to understand what went rong: "%BatchLogFile%"
ECHO Possible cause is UAC ^(User Access Control^) or some local security policy
ping 127.0.0.1 -n 6 >NUL
) ELSE (
ECHO Permission on Logs folder OK: "%TargetFolder%"
ping 127.0.0.1 -n 2 >NUL
)

GOTO :EOF