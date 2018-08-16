@echo off
call %~dp0..\..\..\packages\NodeEnv.1.0.9\nodevars.bat
cd ..\WFM
npm run dev:watch