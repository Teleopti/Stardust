@echo off
@SET PATH=%PATH%;%~dp0
node "%~dp0..\..\packages\nodeenv.1.0.7\npm\bin\npm-cli.js" %*
