@echo off

set work=C:\Code\TeleoptiWFM\
set nunit=%work%packages\NUnit.ConsoleRunner.3.6.1\tools\nunit3-console.exe
set assembly=%work%Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\bin\Debug\Teleopti.Ccc.WebBehaviorTest.dll
set output=/out:%work%Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\Testing\TestResult.txt
set result=/result:%work%Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\Testing\TestResult.xml
rem set tests=--test=Teleopti.Ccc.WebBehaviorTest.Wfm.RealTimeAdherence.SeeHistoricalAdherenceFor7DaysBackFeature
rem set tests=--test=Teleopti.Ccc.WebBehaviorTest.Wfm.RealTimeAdherence.SolidProofFeature
set tests=--test=Teleopti.Ccc.WebBehaviorTest.Wfm.ResourcePlanner.ManageScheduleFeature.RunArchivingForOneAgent

set command=%nunit% %assembly% %tests% %output% %result%

%command%
