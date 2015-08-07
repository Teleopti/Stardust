@echo off

set nunit=..\..\..\packages\NUnit.Runners.2.6.2\tools\nunit-console.exe
set assembly=..\bin\Debug\Teleopti.Ccc.WebBehaviorTest.dll
set output=/out:TestResult.txt
set result=/result:TestResult.xml
set tests=/run=Teleopti.Ccc.WebBehaviorTest.MyTime.PreferencesFeature.AddStandardPreference

set command=%nunit% %assembly% %tests% %output% %result%

echo %command%

%command%

pause
