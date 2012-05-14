@ECHO off
::=======================
::Init
::=======================
::Get path to this batchfile
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

::Move one level up in the folder structure
SET ROOTDIR=%ROOTDIR%\..

ECHO Rootdir is: "%ROOTDIR%"
SET /P ActiveBranchVersion= < "%ROOTDIR%\..\ActiveBranchVersion.txt"
ECHO Previous build number is: %ActiveBranchVersion%
SET /a Build=%ActiveBranchVersion% + 1
ECHO New version will be: %Build%

SET SYSTEMVERSION=7.2.%Build%

::Global myError
SET myError=0

::create a filename
SET "ReleaseFile="
call:ZeroPad8 ReleaseFile %Build%

::check length
SET BuildLen=0
call:Len %ReleaseFile% BuildLen
if %BuildLen% NEQ 8 GOTO :WrongFormat

::check total size
SET size=0
call :sizeAdd "%ROOTDIR%\TeleoptiCCC7\Trunk\Trunk.sql" size
call :sizeAdd "%ROOTDIR%\TeleoptiAnalytics\Trunk\Trunk.sql" size
call :sizeAdd "%ROOTDIR%\TeleoptiCCCAgg\Trunk\Trunk.sql" size
ECHO total size is: %size%

IF %size% EQU 0 goto :NothingToBuild

::pull latest from hg (don't know if this is necessary really...)
hg pull
hg update default

echo.%Build% > "%ROOTDIR%\..\ActiveBranchVersion.txt"

::Build each DB
if %myError% EQU 0 call:CreateRelease TeleoptiAnalytics %ReleaseFile% "%SYSTEMVERSION%" myError
if %myError% EQU 0 call:CreateRelease TeleoptiCCC7 %ReleaseFile% "%SYSTEMVERSION%" myError
if %myError% EQU 0 call:CreateRelease TeleoptiCCCAgg %ReleaseFile% "%SYSTEMVERSION%" myError

::Commit changes
if %myError% EQU 0 (
hg commit -m "Automated database build: %SYSTEMVERSION%"
set myError=%errorlevel%
)

::no push of changes here...
::hg push

if %myError% NEQ 0 GOTO:error

GOTO:success

::--------------------------------------------------------
::-- Function section
::--------------------------------------------------------
:CreateRelease
SETLOCAL
::Set sources and target
SET TRUNKFILE=%ROOTDIR%\%1\Trunk\Trunk.sql
SET BUILDPATH=%ROOTDIR%\%1\Releases\
SET BUILDFILE=%ROOTDIR%\%1\Releases\%2.sql

::local myError
SET /a "myError=0"

::Build exist? Then exist
IF EXIST "%BUILDFILE%" (
ECHO Version already exist, release is NOT created for DB: %1 releasefile: %2
set /a "myError=%myError%+1"
)

::Move  Trunk  into a Releasefile(Tables and Data)
if %myError% EQU 0 (
COPY "%TRUNKFILE%"  "%BUILDPATH%"
RENAME "%BUILDPATH%\Trunk.sql" %2.sql
ATTRIB -R "%BUILDFILE%"

::Add line break at the end
ECHO.>> "%BUILDFILE%"
ECHO GO>> "%BUILDFILE%"
ECHO.>> "%BUILDFILE%"

::If TeleoptiAnalytics; Add EXEC sys_crossdatabaseview_load
IF "%1"=="TeleoptiAnalytics" ECHO. >> "%BUILDFILE%" & ECHO EXEC mart.sys_crossdatabaseview_load >> "%BUILDFILE%" & ECHO GO >> "%BUILDFILE%" & ECHO. >> "%BUILDFILE%"

::Add Version control in database
ECHO.
ECHO PRINT 'Adding build number to database' >> "%BUILDFILE%"
ECHO INSERT INTO DatabaseVersion^(BuildNumber, SystemVersion^) VALUES ^(%Build%,'%SYSTEMVERSION%'^) >> "%BUILDFILE%"
)

::Add the new release file to hg
if %myError% EQU 0 hg add "%BUILDFILE%"
if %errorlevel% NEQ 0 (
set /a "myError=%myError%+1"
echo Sorry, I cannot add new release.
)

::Re-init  the Trunk
if %myError% EQU 0 (
ECHO The trunk will now be re-initated (CTRL-C to abort)
TYPE NUL > "%TRUNKFILE%"
)

(
ENDLOCAL
set "%~4=%myError%"
)
goto:eof

:ZeroPad8
SETLOCAL
set string=%2
set string=0000000000%2
set string=%string:~-8%
(
ENDLOCAL
set "%~1=%string%"
)
goto:eof

:sizeAdd
SETLOCAL
set intSize=0
call:sizeGet %1 intSize
(
ENDLOCAL
set /a "%~2=%intSize%+%2"
)
goto:eof

:sizeGet
SETLOCAL
SET intSize=%~z1
(
ENDLOCAL
set "%~2=%intSize%"
)
goto:eof

:Len
SETLOCAL
set len=-1
echo %1 > "%temp%\st.txt"
for %%a in (%temp%\st.txt) do set /a len=%%~za & set /a len -=3 & del "%temp%\st.txt"
(
ENDLOCAL
set "%~2=%len%"
)
goto:eof

::==================
::goto
::==================
:Success
echo New release %Build% created and checked in
goto:finished

:ERROR_Copy
ECHO Sorry, could not copy file: "%BUILDFILE%"
goto:error

:ERROR_Rename
ECHO Sorry, could not rename file: "%BUILDFILE%"
goto:error

:WrongFormat
ECHO Sorry, the length (%BuildLen%) is wrong, should be 8 figures!
goto:error

:VersionExist
ECHO Sorry, this BUILDFILE is already compiled and shouldn't be re-created!
goto:error

:CannotCheckin
ECHO Sorry, I cannot check in files
goto:error

:error
echo Something went wrong, make sure nothing is checked out!
hg revert -a
goto:finished

:NothingToBuild
echo.There is no changes in current trunks. Skip database build
goto:finished

:finished
exit /b %myError%