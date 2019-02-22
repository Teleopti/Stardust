REM Increases the number of available IIS threads for high performance applications
REM Uses the recommended values from http://msdn.microsoft.com/en-us/library/ms998549.aspx#scalenetchapt06_topic8
REM Values may be subject to change depending on your needs
REM Assumes you're running on two cores (medium instance on Azure)
 
%windir%\system32\inetsrv\appcmd set config /commit:MACHINE -section:processModel -maxWorkerThreads:600 /clr:4 >> log.txt 2>> err.txt
%windir%\system32\inetsrv\appcmd set config /commit:MACHINE -section:processModel -minWorkerThreads:100 /clr:4 >> log.txt 2>> err.txt
%windir%\system32\inetsrv\appcmd set config /commit:MACHINE -section:processModel -minIoThreads:100 /clr:4 >> log.txt 2>> err.txt
%windir%\system32\inetsrv\appcmd set config /commit:MACHINE -section:processModel -maxIoThreads:600 /clr:4 >> log.txt 2>> err.txt


%windir%\SysWOW64\inetsrv\appcmd set config /commit:MACHINE -section:processModel -maxWorkerThreads:600 /clr:4 >> log.txt 2>> err.txt
%windir%\SysWOW64\inetsrv\appcmd set config /commit:MACHINE -section:processModel -minWorkerThreads:100 /clr:4 >> log.txt 2>> err.txt
%windir%\SysWOW64\inetsrv\appcmd set config /commit:MACHINE -section:processModel -minIoThreads:100 /clr:4 >> log.txt 2>> err.txt
%windir%\SysWOW64\inetsrv\appcmd set config /commit:MACHINE -section:processModel -maxIoThreads:600 /clr:4 >> log.txt 2>> err.txt