@ECHO off
ECHO List available drives:
fsutil fsinfo drives
ECHO.
SET /P DriveLetter=please provid drive letter to test:
SET DriveLetter=%DriveLetter:~0,1%
ECHO %DriveLetter%

powershell set-executionpolicy -scope CurrentUser unrestricted
SET resultFile=%~dp0%DriveLetter%_result.txt
ECHO. >"%resultFile%"

::working dir
SET TestFilepath=%DriveLetter%:\Temp\SQLIO
IF NOT EXIST %TestFilepath% MKDIR %TestFilepath%
SET FastMode=True
SET TestFileSizeInGB=5

ECHO This test will take approx 5 min per drive letter
time /t
ping 127.0.0.1 -n 3 >nul
powershell -File "%~dp0scripts\SQLIO.ps1" %TestFileSizeInGB% "%TestFilepath%" SequentialReads64kb %FastMode% "Format-Table" >>"%resultFile%"
powershell -File "%~dp0scripts\SQLIO.ps1" %TestFileSizeInGB% "%TestFilepath%" RandomReads64kb %FastMode% "Format-Table" >>"%resultFile%"
powershell -File "%~dp0scripts\SQLIO.ps1" %TestFileSizeInGB% "%TestFilepath%" RandomWrite8kb %FastMode% "Format-Table" >>"%resultFile%"
powershell -File "%~dp0scripts\SQLIO.ps1" %TestFileSizeInGB% "%TestFilepath%" SequentialWrite2kb %FastMode% "Format-Table" >>"%resultFile%"
powershell -File "%~dp0scripts\SQLIO.ps1" %TestFileSizeInGB% "%TestFilepath%" SequentialWrite6kb %FastMode% "Format-Table" >>"%resultFile%"

::clean up
IF EXIST %TestFilepath% RMDIR %TestFilepath% /Q /S
PAUSE
