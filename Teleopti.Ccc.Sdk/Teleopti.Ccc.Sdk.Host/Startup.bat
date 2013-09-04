@ECHO off

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%

SET DRIVELETTER=%~d0
SET LOGFILE=%ROOTDIR%\Startup.log

::init log file
ECHO Driveletter is: %DRIVELETTER%
ECHO Rootdir is: %ROOTDIR%

::Allow powershell
powershell Set-ExecutionPolicy RemoteSigned

::Declare/Set
SET BlobStorage=http://teleopticcc7.blob.core.windows.net/deployment

::================
::SDK
::================
Echo getting SDK site artifacts from Blob Storage ... 
SET SiteRoot=%DRIVELETTER%\sitesroot\0
SET FileName=SDK.zip

IF NOT EXIST "%SiteRoot%" MKDIR "%SiteRoot%"

ECHO :BlobStorageGet %FileName% %BlobStorage% %SiteRoot%
Call :BlobStorageGet %FileName% %BlobStorage% %SiteRoot%

ECHO :unzipFile "%SiteRoot%\%FileName%" "%SiteRoot%"
Call :unzipFile "%SiteRoot%\%FileName%" "%SiteRoot%"

Echo getting SDK site artifacts from Blob Storage. done 

::================
::Analytics
::================
Echo getting Analytics site artifacts from Blob Storage ...
SET SiteRoot=%DRIVELETTER%\sitesroot\1
SET FileName=Analytics.zip

IF NOT EXIST "%SiteRoot%" MKDIR "%SiteRoot%"

ECHO :BlobStorageGet %FileName% %BlobStorage% %SiteRoot%
Call :BlobStorageGet %FileName% %BlobStorage% %SiteRoot%

ECHO :unzipFile "%SiteRoot%\%FileName%" "%SiteRoot%"
Call :unzipFile "%SiteRoot%\%FileName%" "%SiteRoot%"

Echo getting Analytics web site and config Blob Storage. done 

::================
::Install Report Viewer
::================
SET SiteRoot=%DRIVELETTER%\sitesroot\1
ECHO installing Report Viewer ...  
ECHO log file is here: "%SiteRoot%\ReportViewer2010-installlog.htm" 
CALL "%SiteRoot%\ReportViewer2010.exe" /norestart /log "%SiteRoot%\ReportViewer2010-installlog.htm" /install /q
ECHO installing Report Viewer. Done  

::================
::Find and Replace config
::================
::SDK web config
Call :FindReplace "%DRIVELETTER%\sitesroot\0\web.config" SitePath_ReplaceByAzureDeploy %DRIVELETTER%\sitesroot\0\
Call :FindReplace "%DRIVELETTER%\sitesroot\0\web.config" MatrixWebSiteUrl_ReplaceByAzureDeploy http://teleopticcc7.cloudapp.net/Analytics

:: more to come ....

::================
::Done
::================
GOTO :eof

::================
::Sub routines
::================
:BlobStorageGet
ECHO powershell -File "%ROOTDIR%\BlobStorageGet.ps1" %1 %2 %3
powershell -File "%ROOTDIR%\BlobStorageGet.ps1" %1 %2 %3
exit /b

:UnzipFile
ECHO 7z.exe x -y %1 -o%2
7z.exe x -y %1 -o%2
exit /b

:FindReplace
ECHO powershell ". %ROOTDIR%\FindReplace.ps1; FindAndReplace -FileName \"%1\" -searchText \"%2\" -replaceText \"%3\""
powershell ". %ROOTDIR%\FindReplace.ps1; FindAndReplace -FileName \"%1\" -searchText \"%2\" -replaceText \"%3\""
exit /b

:eof
