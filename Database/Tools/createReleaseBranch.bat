:: Creates a new release branch in the source repository
@ECHO off

SET THISDIR=%~dp0

:: Call VersionBuild.bat
call "%THISDIR\versionbuild.bat"

if %myError% EQU 0 (
:: create new release branch and push it to server
set releaseBranchName="Release %SYSTEMVERSION%"
echo Creating branch %releaseBranchName%
hg pull
hg update default
hg branch "%releaseBranchName%"
hg ci -m "Creating release %releaseBranchName%"
hg push --new-branch

echo Finished!
)