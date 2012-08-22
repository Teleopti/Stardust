@echo off

:: goto start

echo Please configure this file by editing it before running
pause
exit

:start

set nunit=..\..\..\packages\NUnit.Runners.2.6.0.12051\tools\nunit-console.exe
:: set tests=/run=Teleopti.Ccc.WebBehaviorTest.PreferencesFeature.AddStandardPreference

set testoriginal=..\
set testfolder1=..\..\Teleopti.Ccc.WebBehaviorTest.Test1
set testfolder2=..\..\Teleopti.Ccc.WebBehaviorTest.Test2
set server=localhost
:: set siteurl1=http://%server%/Test1/
:: set siteurl2=http://%server%/Test2/
set siteurl1=http://test1.local.teleopti.com/
set siteurl2=http://test2.local.teleopti.com/
set siteoriginal=..\..\Teleopti.Ccc.Web\
set sitepath1=..\..\Teleopti.Ccc.Web.Test1\
set sitepath2=..\..\Teleopti.Ccc.Web.Test2\




if not exist %testfolder1% md %testfolder1%
if not exist %testfolder2% md %testfolder2%

xcopy %testoriginal%*.* %testfolder1% /s /e /q /y
xcopy %testoriginal%*.* %testfolder2% /s /e /q /y

copy TestRunParallel.appconfig %testfolder1%\App.config
copy TestRunParallel.appconfig %testfolder1%\bin\Debug\Teleopti.Ccc.WebBehaviorTest.dll.config
copy TestRunParallel.appconfig %testfolder2%\App.config
copy TestRunParallel.appconfig %testfolder2%\bin\Debug\Teleopti.Ccc.WebBehaviorTest.dll.config



if not exist %sitepath1% md %sitepath1%
if not exist %sitepath2% md %sitepath2%

xcopy %siteoriginal%*.* %sitepath1% /s /e /q /y
xcopy %siteoriginal%*.* %sitepath2% /s /e /q /y

set infratest=%testfolder1%\bin\Debug\infratest.ini
set dbname=Test1
echo [TestDatabase] >%infratest%
echo dbname="%dbname%" >>%infratest%
echo servername="%server%" >>%infratest%
echo user="sa" >>%infratest%
echo password="cadadi" >>%infratest%
echo matrix="Data Source=%server%;Initial Catalog=%dbname%_analytics;User Id=sa;Password=cadadi" >>%infratest%
echo create="true" >>%infratest%
echo url="%siteurl1%" >>%infratest%
echo sitepath="..\%sitepath1%" >>%infratest%

set infratest=%testfolder2%\bin\Debug\infratest.ini
set dbname=Test2
echo [TestDatabase] >%infratest%
echo dbname="%dbname%" >>%infratest%
echo servername="%server%" >>%infratest%
echo user="sa" >>%infratest%
echo password="cadadi" >>%infratest%
echo matrix="Data Source=%server%;Initial Catalog=%dbname%_analytics;User Id=sa;Password=cadadi" >>%infratest%
echo create="true" >>%infratest%
echo url="%siteurl2%" >>%infratest%
echo sitepath="..\%sitepath2%" >>%infratest%

set assembly1=%testfolder1%\bin\Debug\Teleopti.Ccc.WebBehaviorTest.dll
set assembly2=%testfolder2%\bin\Debug\Teleopti.Ccc.WebBehaviorTest.dll

start %nunit% %assembly1% %tests% /result=Test1.xml
start %nunit% %assembly2% %tests% /result=Test2.xml

pause
