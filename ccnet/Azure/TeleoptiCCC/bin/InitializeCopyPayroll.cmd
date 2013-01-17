@ECHO OFF

SET DIRECTORY=%~dp0
SET FILE=%~n0

::Delete if exist
schtasks /Delete /F /TN "CopyPayrollDll" >> "%FILE%.log"

::Schedule task every 20 minutes
schtasks /create /sc minute /mo 20 /tn "CopyPayrollDll" /tr "%DIRECTORY%CopyPayrollDLL.cmd" >> "%FILE%.log"

::Run for the first time
"%DIRECTORY%CopyPayrollDll.cmd"  >> "%FILE%.log"