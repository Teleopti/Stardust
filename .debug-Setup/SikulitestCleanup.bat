taskkill /F /IM iisexpress.exe

set folder="C:\temp\AdminClient"
IF EXIST "%folder%" (
cd /d %folder%
for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q || del "%%i" /s/q)
)