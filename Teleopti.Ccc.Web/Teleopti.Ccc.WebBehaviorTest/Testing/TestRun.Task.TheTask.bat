@echo off

set work=D:\RaptorHg\teleopticcc\
set nunit=%work%packages\NUnit.Runners.2.6.2\tools\nunit-console.exe
set assembly=%work%Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\bin\Debug\Teleopti.Ccc.WebBehaviorTest.dll
set output=/out:%work%Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\Testing\TestResult.txt
set result=/result:%work%Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\Testing\TestResult.xml
set tests=/run=Teleopti.Ccc.WebBehaviorTest.MyTime.PreferencesFeature.AddStandardPreference

set command=%nunit% %assembly% %tests% %output% %result%

echo %command%

%command%

pause
