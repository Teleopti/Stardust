How to deploy Local Wiki

1. The Build process will archives the local wiki file and the required deployment batch files into \\A380\T-Files\Product\Teleopti CCC\v8\LocalWiki\Latest folder.
2. Copy those folder/files locally
3. Run UninstallWiki.bat manually
4. Run InstallWiki.bat manually
5. InstallWiki.bat file will read installation settings from the registry and get the installed directory path.
6. If it does not exists then it will ask for install path. (eg C:\Program Files(x86)\Teleopti\)

The batch file will automatically unzip file using 7za.exe in the install directory.
Create Virtual directory (/path:/TeleoptiCCC/LocalWiki) automatically. 

Test:
http://localhost/TeleoptiWFM/LocalWiki

