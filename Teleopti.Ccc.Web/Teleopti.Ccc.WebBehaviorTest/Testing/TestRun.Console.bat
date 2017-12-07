@echo off

set nunit=..\..\..\packages\NUnit.ConsoleRunner.3.6.1\tools\nunit3-console.exe
set assembly=..\bin\Release\Teleopti.Ccc.WebBehaviorTest.dll
set output=/out:TestResult.txt
set result=/result:TestResult.xml
set tests=--test=Teleopti.Ccc.WebBehaviorTest.Wfm.RealTimeAdherence

set command=%nunit% %assembly% %tests% %output% %result%

echo %command%

%command%

pause
