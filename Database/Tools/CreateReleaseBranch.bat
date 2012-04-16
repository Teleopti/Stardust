:: Put this bat file elsewhere... Where?
:: Creates a new release branch in the source repository
@ECHO off

SET THISDIR=%~dp0
::Remove trailer slash
SET ROOTDIR=%THISDIR:~0,-1%
SET ROOTDIR=%ROOTDIR%\..\..

::Confirm
ECHO This will create release db scripts and
ECHO create a new named branch on server repository.
ECHO For safety reasons - you need to push manually. Named branches cannot easily be deleted.
ECHO When you push - you need the --new-branch flag to be set.
ECHO If you're using TortoiseHg, remember to remove the "allow --new-branch" thing after you've pushed!
CHOICE /M "Are you sure you want to continue"
IF ERRORLEVEL 2 exit
ECHO.

:: Call VersionBuild.bat
call "%THISDIR%\versionbuild.bat"

set releaseBranchName="Release %SYSTEMVERSION%"
echo using releasebranchname %releaseBranchName%

if %myError% NEQ 0 (
echo something wrong in versionbuild.bat. Ending...
exit
)

:: create new release branch
echo Creating branch %releaseBranchName%
hg pull
hg update default
hg branch %releaseBranchName%
hg ci -m "Creating release %SYSTEMVERSION%"
::hg push

:: Bump version on default
hg update default
SET /P ActiveBranchVersion= < "%ROOTDIR%\..\ActiveBranchVersion.txt"
SET /a NextBuild=%ActiveBranchVersion% + 1
echo.%NextBuild% > "%ROOTDIR%\..\ActiveBranchVersion.txt"

echo Finished!