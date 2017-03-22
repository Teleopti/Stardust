@ECHO OFF

SET DIRECTORY=%~dp0
SET FILE=%~n0
SET admin=ScheduleTaskAdmin
SET taskName=CopyPayrollDll
SET taskName2=ClickOnceSign

::generate password
net user administrator /random > %temp%\pwd.txt
set /P pwd=< %temp%\pwd.txt
set pwd=%pwd:~31,39%
echo Temporary user is %admin% with password %pwd% >> "%FILE%.log"
del %temp%\pwd.txt

::Add new local user to administrators group
net user %admin% /del
net user %admin% %pwd% /add
net localgroup Administrators %admin% /add

::try start the service before we contiune
net start "task scheduler" >> "%FILE%.log"

::Delete if exist
schtasks /Delete /F /TN "%taskName%" >> "%FILE%.log"
schtasks /Delete /F /TN "%taskName2%" >> "%FILE%.log"

::Get current date and time
FOR /F "TOKENS=1* DELIMS= " %%A IN ('DATE/T') DO SET CDATE=%%B
FOR /F "TOKENS=1,2 eol=/ DELIMS=/ " %%A IN ('DATE/T') DO SET mm=%%B
FOR /F "TOKENS=1,2 DELIMS=/ eol=/" %%A IN ('echo %CDATE%') DO SET dd=%%B
FOR /F "TOKENS=2,3 DELIMS=/ " %%A IN ('echo %CDATE%') DO SET yyyy=%%B
SET date=%mm%/%dd%/%yyyy%
echo %date%


FOR /F "tokens=1-3 delims=:" %%A IN ('echo %time%') DO SET HOUR=%%A& SET MINUTES=%%B
FOR /F "tokens=1,2 delims= " %%A IN ('time /t') DO SET AM_PM=%%B  

:: --- Convert the HOUR block [H] to "24 hour" format [HH]
IF %AM_PM%==PM (
IF %HOUR%==01 (SET HOUR=13)
IF %HOUR%==02 (SET HOUR=14)
IF %HOUR%==03 (SET HOUR=15)
IF %HOUR%==04 (SET HOUR=16)
IF %HOUR%==05 (SET HOUR=17)
IF %HOUR%==06 (SET HOUR=18)
IF %HOUR%==07 (SET HOUR=19)
IF %HOUR%==08 (SET HOUR=20)
IF %HOUR%==09 (SET HOUR=21)
IF %HOUR%==10 (SET HOUR=22)
IF %HOUR%==11 (SET HOUR=23)
) ELSE (
IF %HOUR%==12 (SET HOUR=00)
IF %HOUR%==1 (SET HOUR=01)
IF %HOUR%==2 (SET HOUR=02)
IF %HOUR%==3 (SET HOUR=03)
IF %HOUR%==4 (SET HOUR=04)
IF %HOUR%==5 (SET HOUR=05)
IF %HOUR%==6 (SET HOUR=06) 
IF %HOUR%==7 (SET HOUR=07)
IF %HOUR%==8 (SET HOUR=08)
IF %HOUR%==9 (SET HOUR=09)
)  

SET /a MIN=%MINUTES%+3
SET TIMESTAMP=%HOUR%:%MIN%

::Schedule task every 20 minutes
schtasks /create /sc minute /mo 20 /tn "%taskName%" /f /ru "%admin%" /rp "%pwd%" /tr "%DIRECTORY%CopyPayrollDll.cmd" >> "%FILE%.log"

::Run for the first time
schtasks /RUN /TN "%taskName%" >> "%FILE%.log"

ECHO %ERRORLEVEL%
exit /b 0