@echo off
@SET PATH=%PATH%;%~dp0
"%~dp0..\..\..\packages\nodeenv.1.0.9\flatten-packages.cmd" %*
