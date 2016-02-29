@echo off
ping 127.0.0.1 -n 4 > nul
call CloseIISExpress.bat
ping 127.0.0.1 -n 4 > nul
call StartIISExpress.bat