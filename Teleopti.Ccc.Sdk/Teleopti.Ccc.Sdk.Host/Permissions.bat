@ECHO off
SET WindowsNT=%~1
SET SPLevel=%~2
SET IISVersion=%~3
SET SVCLOGIN=%~4

SET ROOTDIR=%~dp0
CALL "%ROOTDIR%PermissionsSub.bat" "%WindowsNT%" "%SPLevel%" "%IISVersion%" "%SVCLOGIN%" >"%ROOTDIR%Permissions.log"