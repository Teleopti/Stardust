@echo off
powershell -NoProfile "%~dp0\UseNodeEnv.ps1;cd ..\WFM;npm run dev:watch"
