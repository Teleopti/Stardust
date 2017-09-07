@echo off
@SET PATH=%PATH%;%~dp0
node "%~dp0..\..\..\packages\nodeenv.1.0.6\node_modules\grunt-cli\bin\grunt" %*
