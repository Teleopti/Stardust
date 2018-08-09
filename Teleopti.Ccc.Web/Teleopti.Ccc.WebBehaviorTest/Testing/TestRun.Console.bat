@echo off

set nunit=..\..\..\packages\NUnit.ConsoleRunner.3.6.1\tools\nunit3-console.exe
set assembly=..\bin\Debug\Teleopti.Ccc.WebBehaviorTest.dll
set output=/out:TestResult.txt
set result=/result:TestResult.xml
rem set tests=--test=Teleopti.Ccc.WebBehaviorTest.Wfm.RealTimeAdherence.SeeHistoricalAdherenceFor7DaysBackFeature
rem set tests=--test=Teleopti.Ccc.WebBehaviorTest.Wfm.RealTimeAdherence.SolidProofFeature
set tests=--test=Teleopti.Ccc.WebBehaviorTest.Wfm.ResourcePlanner.ManageScheduleFeature.RunArchivingForOneAgent

set command=%nunit% %assembly% %tests% %output% %result%

echo %command%

%command%

pause
