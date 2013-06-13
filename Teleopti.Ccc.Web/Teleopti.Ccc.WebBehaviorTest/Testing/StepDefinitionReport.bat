@echo off

set specflowFolder=..\..\..\packages\SpecFlow.1.9.0\tools\
set specflow=%specflowFolder%specflow.exe
set project=..\Teleopti.Ccc.WebBehaviorTest.csproj
set action=stepdefinitionreport

set command=%specflow% %action% %project%

copy specflow.exe.config %specflowFolder%

echo %command%

%command%

cmd /c "start StepDefinitionReport.html"
