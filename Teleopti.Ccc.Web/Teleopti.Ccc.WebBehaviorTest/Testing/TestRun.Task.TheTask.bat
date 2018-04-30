@echo off

set work=C:\Code\teleoptiwfm.git\
set nunit=%work%packages\NUnit.ConsoleRunner.3.6.1\tools\nunit3-console.exe
set assembly=%work%Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\bin\Debug\Teleopti.Ccc.WebBehaviorTest.dll
set output=/out:%work%Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\Testing\TestResult.txt
set result=/result:%work%Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\Testing\TestResult.xml
set tests=--test=Teleopti.Ccc.WebBehaviorTest.Wfm.RealTimeAdherence.SolidProofFeature.SeeRuleChanges

set command=%nunit% %assembly% %tests% %output% %result%

%command%
