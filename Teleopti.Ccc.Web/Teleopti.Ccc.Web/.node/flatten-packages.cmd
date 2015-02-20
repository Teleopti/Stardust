@echo off
@SET PATH=%PATH%;%~dp0
"%~dp0..\..\..\packages\nodeenv.1.0.3\flatten-packages.cmd" %*
