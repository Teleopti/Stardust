@echo off
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
COLOR A
cls

::init
set productId=D8B146E5-428F-42BD-9509-6585ABE76818
SET "isInstalled="
SET /A AMD64=0
SET /A x86=0

::64-bit
reg query HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{%productId%} /v DisplayName /reg:64 > NUL
SET /A AMD64=%errorlevel%

::32-bit
reg query HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{%productId%} /v DisplayName /reg:32 > NUL
SET /A x86=%errorlevel%

::hide reg query error
cls

::uninstall
if %x86% equ 0 SET /A isInstalled=1
if %AMD64% equ 0 SET /A isInstalled=1
if defined isInstalled (
	echo uninstall ...
	MsiExec.exe /X{%productId%} /qb /L "uninstall-server.log"
	echo uninstall. Done!
)

GOTO :EOF

:EOF