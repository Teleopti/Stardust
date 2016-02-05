taskkill /F /IM iisexpress.exe

ping 127.0.0.1 -n 11 > nul

set folder="C:\temp\AdminClient"
IF EXIST "%folder%" (
cd /d %folder%
for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q || del "%%i" /s/q)
)