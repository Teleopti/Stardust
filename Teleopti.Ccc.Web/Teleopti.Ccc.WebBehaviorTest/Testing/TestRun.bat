@echo off

:: goto start


:start

set nunit=..\..\..\packages\NUnit.Runners.2.6.2\tools\nunit-console.exe
set assembly=..\bin\Debug\Teleopti.Ccc.WebBehaviorTest.dll
set output=/out:TestResult.txt
set tests=/run=Teleopti.Ccc.WebBehaviorTest.PreferencesFeature.AddStandardPreference

set command=%nunit% %assembly% %tests% %output%

echo %command%

%command%

pause
