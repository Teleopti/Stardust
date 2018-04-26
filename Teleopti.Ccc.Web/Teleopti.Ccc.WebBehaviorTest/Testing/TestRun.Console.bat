@echo off

set nunit=..\..\..\packages\NUnit.ConsoleRunner.3.6.0\tools\nunit3-console.exe
set assembly=..\bin\Release\Teleopti.Ccc.WebBehaviorTest.dll
set output=/out:TestResult.txt
set result=/result:TestResult.xml
set tests=--test=Teleopti.Ccc.WebBehaviorTest.Wfm.RealTimeAdherence.SeeHistoricalAdherenceFor7DaysBackFeature.SeeHistoricalAdherenceFor7DaysBack

set command=%nunit% %assembly% %tests% %output% %result%

echo %command%

%command%

pause
