@echo off
call %~dp0..\..\..\packages\NodeEnv.1.1.0\nodevars.bat
cd ..\WFM
npm run dev:watch