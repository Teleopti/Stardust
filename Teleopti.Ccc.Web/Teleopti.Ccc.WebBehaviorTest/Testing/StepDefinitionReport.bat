::@echo off

set specflowFolder=..\..\..\packages\SpecFlow.2.1.0\tools\
set specflow=%specflowFolder%specflow.exe
set project=..\Teleopti.Ccc.WebBehaviorTest.csproj
set action=stepdefinitionreport

set command=%specflow% %action% %project%

copy specflow.exe.config %specflowFolder%

echo %command%

%command%

cmd /c "start StepDefinitionReport.html"
