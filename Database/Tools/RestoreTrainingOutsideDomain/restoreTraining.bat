SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
SET RARFOLDER=%ROOTDIR%\rarFiles
SET DATAFOLDER=%ROOTDIR%\Data
SET Zip7Folder=%ROOTDIR%\7zip
SET BRANCH=%COMPUTERNAME%
SET INSTANCE=.
SET CUSTOMER=Training
SET AppRar=%Customer%App.rar
SET StatRar=%Customer%Stat.rar
SET UNRAR="%Zip7Folder%\7z.exe" x -y -o"%RarFolder%"
SET DBMANAGER=C:\Program Files (x86)\Teleopti\DatabaseInstaller\DBManager.exe
SET DATABASEPATH=C:\Program Files (x86)\Teleopti\DatabaseInstaller
SET TRUNK=-T -R -Ltraining:training
SET SUPPORTOOLDIR=C:\Program Files (x86)\Teleopti\SupportTools
SET GIGANTES=\\gigantes\Customer Databases\CCC\RestoreToLocal
SET INSTALLDIR=C:\Program Files (x86)\Teleopti
NET USE "%GIGANTES%"
ROBOCOPY "%GIGANTES%\Baselines\7zip" "%Zip7Folder%" /MIR

IF NOT EXIST "%RARFOLDER%" MKDIR "%RARFOLDER%"
IF NOT EXIST "%DATAFOLDER%" MKDIR "%DATAFOLDER%"
XCOPY "%GIGANTES%\tsql\Restore.sql" "%ROOTDIR%" /Y
XCOPY "%GIGANTES%\Baselines\%AppRar%" "%RARFOLDER%" /Y
XCOPY "%GIGANTES%\Baselines\%StatRar%" "%RARFOLDER%" /Y

%UNRAR% "%RarFolder%\%AppRar%"
%UNRAR% "%RarFolder%\%StatRar%"

::Restore Customer databases
ECHO.
ECHO ------
ECHO Restoring baselines databases from backup. This will take a few minutes...
SQLCMD -S%INSTANCE% -E -dmaster -i"%ROOTDIR%\Restore.sql" -v DATAFOLDER="%DATAFOLDER%" -v RARFOLDER="%RARFOLDER%" -v CUSTOMER=%Customer% -v LOADSTAT=1 -v BRANCH="%BRANCH%"

ECHO Restoring baselines. Done!
ECHO ------
ECHO.

::patcha etc.
"%DBMANAGER%" -S%INSTANCE% -D"%Customer%_TeleoptiAnalytics" -E -OTeleoptiAnalytics %TRUNK% %CreateAnalytics% -F"%DATABASEPATH%" 
"%DBMANAGER%" -S%INSTANCE% -D"%Customer%_TeleoptiCCCAgg" -E -OTeleoptiCCCAgg %TRUNK% %CreateAgg% -F"%DATABASEPATH%"
"%DBMANAGER%" -S%INSTANCE% -D"%Customer%_TeleoptiCCC7" -E -OTeleoptiCCC7 %TRUNK% -F"%DATABASEPATH%"

::konfa om
COPY "%ROOTDIR%\settings.txt.template" "%ROOTDIR%\settings.txt" /Y
cscript "%ROOTDIR%\vbs\replace.vbs" "REPLACEME" "%COMPUTERNAME%" "%ROOTDIR%\settings.txt"
COPY "%ROOTDIR%\settings.txt" "%SUPPORTOOLDIR%" /Y
"%SUPPORTOOLDIR%\Teleopti.Support.Tool.exe" -MODeploy

"%INSTALLDIR%\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe" -DS%INSTANCE% -DD"%Customer%_TeleoptiCCC7" -EE
"%INSTALLDIR%\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe" -DS%INSTANCE% -DD"%Customer%_TeleoptiAnalytics" -CD"%Customer%_TeleoptiCCCAgg" -EE

::restart
"%SUPPORTOOLDIR%\StartStopSystem\ResetSystem.bat" y
NET USE "%GIGANTES%" /DEL
PAUSE