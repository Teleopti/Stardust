:: Creates a new release branch in the source repository
@ECHO off

SET THISDIR=%~dp0

::Confirm
ECHO This will create release db scripts and
ECHO create a new named branch on server repository.
ECHO This operation cant be rolled back.
CHOICE /M "Are you sure you want to continue"
IF ERRORLEVEL 2 exit
ECHO.


:: Call VersionBuild.bat
call "%THISDIR%\versionbuild.bat"

set releaseBranchName="Release %SYSTEMVERSION%"
echo using releasebranchname %releaseBranchName%

if %myError% EQU 0 (
:: create new release branch and push it to server
echo Creating branch %releaseBranchName%
hg pull
hg update default
hg branch %releaseBranchName%
hg tip "Starting %SYSTEMVERSION%"
hg ci -m "Creating release %SYSTEMVERSION%"
hg push --new-branch
hg update default

echo Finished!
)