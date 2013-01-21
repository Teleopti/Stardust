@ECHO OFF

SET DIRECTORY=%~dp0
SET FILE=%~n0
SET TaskName=CopyPayrollDll

::try start the service before we contiune
net start "task scheduler" >> "%FILE%.log"

::Delete if exist
schtasks /Delete /F /TN "%TaskName%" >> "%FILE%.log"

::Schedule task every 20 minutes
schtasks /create /sc minute /mo 20 /tn "%TaskName%" /f /tr "%DIRECTORY%CopyPayrollDLL.cmd" >> "%FILE%.log"

::Run for the first time
schtasks /RUN /TN "CopyPayrollDll" >> "%FILE%.log"