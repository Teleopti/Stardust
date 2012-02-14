::Get path to this batchfile
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%

echo open ftp.teleopti.com> "%temp%\tfp.txt"
echo China>> "%temp%\tfp.txt"
echo 1qaz!QAZ>> "%temp%\tfp.txt"
echo binary>> "%temp%\tfp.txt"
echo prompt n>> "%temp%\tfp.txt"
echo cd msi>> "%temp%\tfp.txt"
echo mkdir %1>> "%temp%\tfp.txt"
echo cd %1>> "%temp%\tfp.txt"
echo lcd %2>> "%temp%\tfp.txt"
echo put "Teleopti CCC 7.1.*.zip">> "%temp%\tfp.txt"
echo put "ContextHelpEN.exe">> "%temp%\tfp.txt"

echo bye>> "%temp%\tfp.txt"
ftp -i -s:"%temp%\tfp.txt"

del "%temp%\tfp.txt"
