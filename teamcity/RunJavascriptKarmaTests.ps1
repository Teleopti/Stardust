@ECHO off

:: Set nodevars so node/npm/npx is available via path
call ..\..\..\packages\NodeEnv.1.1.0\nodevars.bat


SET /A ERRORLEV=0
:: Runs from [repo]\Teleopti.Ccc.Web\Teleopti.Ccc.Web\WFM
call npm run test:teamcity
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=1 & GOTO :error
echo %errorlevel%

GOTO :EOF

:Error
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!

ECHO.
ECHO --------
Exit /B %ERRORLEV%
GOTO :EOF