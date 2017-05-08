copy \\a380\hangaren\#PROGRAM\Develop\Syncfusion\BuildServer\BuildServerSetup.reg .debug-Setup\BuildServerSetup.reg

regedit /s .debug-Setup\BuildServerSetup.reg

del .debug-Setup\BuildServerSetup.reg

".debug-Setup\Restore to Local.bat"