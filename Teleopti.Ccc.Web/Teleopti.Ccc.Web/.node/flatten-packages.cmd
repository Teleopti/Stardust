@echo off
@SET PATH=%PATH%;%~dp0
"%~dp0..\..\..\packages\nodeenv.1.1.0\flatten-packages.cmd" %*
