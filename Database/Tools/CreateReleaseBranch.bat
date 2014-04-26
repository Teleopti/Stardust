:: Put this bat file elsewhere... Where?
:: Creates a new release branch in the source repository
@ECHO off

SET THISDIR=%~dp0
::Remove trailer slash
SET ROOTDIR=%THISDIR:~0,-1%
SET ROOTDIR=%ROOTDIR%\..\..

::Confirm
REM ECHO This will create release db scripts and
REM ECHO create a new named branch on server repository.
REM ECHO For safety reasons - you need to push manually. Named branches cannot easily be deleted.
REM ECHO When you push - you need the --new-branch flag to be set.
REM ECHO If you're using TortoiseHg, remember to remove the "allow --new-branch" thing after you've pushed!
REM CHOICE /M "Are you sure you want to continue"
REM IF ERRORLEVEL 2 exit
REM ECHO.

ECHO This will generate a new database build! Re-init trunks!
CHOICE /M "Are you sure you want to continue"
IF ERRORLEVEL 2 exit
ECHO.


:: Call VersionBuild.bat
call "%THISDIR%\versionbuild.bat"

REM set releaseBranchName="Release %SYSTEMVERSION%"
REM echo using releasebranchname %releaseBranchName%

if %myError% NEQ 0 (
echo something wrong in versionbuild.bat. Ending...
exit
)

:: create new release branch
REM echo Creating branch %releaseBranchName%
REM hg pull
REM hg update default
REM hg branch %releaseBranchName%
REM hg ci -m "Creating release %SYSTEMVERSION%"

:: Bump version on default
REM hg update default
REM SET /P ActiveBranchVersion= < "%ROOTDIR%\..\ActiveBranchVersion.txt"
REM SET /a NextBuild=%ActiveBranchVersion% + 1
REM echo.%NextBuild% > "%ROOTDIR%\..\ActiveBranchVersion.txt"
REM hg ci -m "Bumping version number to %NextBuild%"

echo Finished!