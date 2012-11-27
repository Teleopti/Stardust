@echo off

set specflow=..\..\..\packages\SpecFlow.1.9.0\tools\specflow.exe
set project=..\Teleopti.Ccc.WebBehaviorTest.csproj
set action=stepdefinitionreport

set command=%specflow% %action% %project%

echo %command%

%command%

cmd /c "start StepDefinitionReport.html"
