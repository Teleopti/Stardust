@ECHO OFF
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
 
::Get user input
SET /P SQLServerInstance=name of SQL Server instance: 
SET /P ASServerInstance=name of AS Server instance: 
SET /P DatabaseName=name of SQL and AS database: 
SET /P WinAuth=Use WinAuth connection between AS and SQL? [Y/N]: 

::Check input
IF /I "%WinAuth%"=="Y" GOTO WinAuth
IF /I "%WinAuth%"=="N" (
GOTO SQLAuth) ELSE (
GOTO errorAuth)

::Set Auth string
:WinAuth
SET Conn=-EE
GOTO Execute

:SQLAuth
SET /P SQLLogin=SQL Login: 
SET /P SQLPwd=SQL password: 
SET Conn=-SU%SQLLogin% -SP%SQLPwd%
GOTO Execute

:Execute
"%ROOTDIR%\AnalysisServicesManager.exe" -AS%ASServerInstance% -AD%DatabaseName% -SS%SQLServerInstance% -SD%DatabaseName% %Conn% -FPDropDatabase.xmla
IF %ERRORLEVEL% NEQ 0 GOTO errorExucute
ECHO.

"%ROOTDIR%\AnalysisServicesManager.exe" -AS%ASServerInstance% -AD%DatabaseName% -SS%SQLServerInstance% -SD%DatabaseName% %Conn% -FPCreateDatabase.xmla
IF %ERRORLEVEL% NEQ 0 GOTO errorExucute
ECHO.

::Wait for Cube to wake up
ping 127.0.0.1 -n 3 -w 1000 > nul
"%ROOTDIR%\AnalysisServicesManager.exe" -AS%ASServerInstance% -AD%DatabaseName% -SS%SQLServerInstance% -SD%DatabaseName% %Conn% -FPProcessDatabase.xmla
IF %ERRORLEVEL% NEQ 0 GOTO errorExucute
ECHO.
GOTO finished

:errorAuth
ECHO.
ECHO Neither Y or N given as connection method. Abort!
GOTO :EOF

:errorExucute
ECHO.
ECHO ------------WARNING!!!---------------
ECHO Something when wrong when connecting or executing the XMLA script in AS!
ECHO Please check log file
NOTEPAD %~dp0ErrorLog.txt
ECHO -------------------------------------
ECHO.
GOTO :EOF

:finished
ECHO Drop, create and Process. All Done!
GOTO :EOF

