@ECHO OFF
::Depends on:
::Version, ProductId, SRCDIR, OUTDIR, WISEPROJFILE, ccc7_server, ccc7_forecast,ccc7_client,ccc7_mytime,WISEEXE
::All set by outer batch file

::If  Version is nothing get it from user
IF "%Version%"=="" GOTO VersionIsNull
IF "%ProductId%"=="" GOTO ProductIdIsNull
IF "%SRCDIR%"=="" GOTO PathIsNull
IF "%OUTDIR%"=="" GOTO PathIsNull
IF "%WISESOURCEFILE%"=="" GOTO PathIsNull

::Set Product
IF %ProductId%==1 SET Product=%ccc7_server%
IF %ProductId%==2 SET Product=%ccc7_forecast%
IF %ProductId%==3 SET Product=%ccc7_client%
IF %ProductId%==4 SET Product=%ccc7_mytime%

::set Outfile
IF %ProductId%==1 SET OUTFILE=Teleopti WFM %version%.msi
IF %ProductId%==2 SET OUTFILE=Teleopti WFM Forecasts %version%.exe
IF %ProductId%==3 SET OUTFILE=Teleopti WFM Client %version%.msi
IF %ProductId%==4 SET OUTFILE=Teleopti WFM MyTime %version%.msi

::Exist?
IF EXIST "%OUTDIR%\%OUTFILE%" (
ECHO.
ECHO --------------------------------
ECHO Warning: This product and version already exist as a compiled exe/msi-file!
ECHO %OUTDIR%\%OUTFILE%
ECHO --------------------------------
ECHO Would you like to re-compile the file?
IF %Silent%==1 ECHO Sorry I'm in silent mode. Abort!&GOTO EOF
SET /P ReCompile=Re-Compile? [Y/N]
IF "%ReCompile%"=="N" GOTO DontReCompile
IF "%ReCompile%"=="n" GOTO DontReCompile
ECHO This will overwrite current file for this version! Sure?
ECHO Cancel via [Ctrl-C] or&PAUSE
)

::create empty stub-file
TYPE NUL>"%WISEPROJFILE%\%Product%\%Product%.stub"

::compile all .wse files
if exist "%WISEPROJFILE%\%Product%\*.wse" (
FOR /F %%I IN ('dir /b "%WISEPROJFILE%\%Product%\*.wse"') DO "C:\Program Files (x86)\Altiris\Wise\WiseScript Package Editor\Wise32.exe" /c "%WISEPROJFILE%\%Product%\%%I"
IF %ERRORLEVEL% NEQ 0 GOTO Error
ECHO Compiling WISE project, Done!
)

::Start Wise compile
ECHO "%WISEEXE%" "%WISEPROJFILE%\%Product%\%Product%.wsi" /c
"%WISEEXE%" "%WISEPROJFILE%\%Product%\%Product%.wsi" /c
IF %ERRORLEVEL% NEQ 0 GOTO Error
ECHO Compiling WISE project, Done!

ECHO Copy file to destination folder  ...
::move final exe to target location
ECHO MOVE /Y "%WISEPROJFILE%\%Product%\%Product%.exe" "%OUTDIR%\%OUTFILE%"
IF %ProductId%==1 MOVE /Y "%WISEPROJFILE%\%Product%\%Product%.msi" "%OUTDIR%\%OUTFILE%"
IF %ProductId%==2 MOVE /Y "%WISEPROJFILE%\%Product%\%Product%.exe" "%OUTDIR%\%OUTFILE%"
IF %ProductId%==3 MOVE /Y "%WISEPROJFILE%\%Product%\%Product%.msi" "%OUTDIR%\%OUTFILE%"
IF %ProductId%==4 MOVE /Y "%WISEPROJFILE%\%Product%\%Product%.msi" "%OUTDIR%\%OUTFILE%"
::IF %ProductId%==5 MOVE /Y "%WISEPROJFILE%\%Product%\%Product%.msi" "%OUTDIR%\%OUTFILE%"
GOTO Done

:DontReCompile
ECHO Skipping WISE compile ....
GOTO EOF

:PathIsNull
ECHO Some of the needed paths are blank. Abort!
GOTO EOF

:VersionIsNull
ECHO No version number provided. Abort!
GOTO EOF

:ProductIdIsNull
ECHO No productId provided. Abort!
GOTO EOF

:Error
ECHO Error from Wise. File didn't compile!
ECHO Product: %Product%, Version: %Version%
SET ERRORLEVEL=%ProductId%
IF %Silent%==0 PAUSE
GOTO EOF

:Done
ECHO Done!
GOTO EOF

:EOF
