@echo off
tasklist /fi "imagename eq iisexpress.exe" | find ":" > nul
if errorlevel 1 taskkill /F /IM iisexpress.exe"
