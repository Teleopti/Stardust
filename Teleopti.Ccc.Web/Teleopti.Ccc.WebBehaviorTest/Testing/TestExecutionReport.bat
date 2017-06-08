@echo off

:: goto start

echo Please configure this file by editing it before running
pause
exit

:start

:: set nunit=..\..\..\packages\NUnit.Runners.2.6.2\tools\nunit-console.exe
set nunit="C:\Program Files (x86)\NUnit 2.6\bin\nunit-console.exe"
set specflow=..\..\..\packages\SpecFlow.2.1.0\tools\specflow.exe
set assembly=..\bin\Debug\Teleopti.Ccc.WebBehaviorTest.dll
set project=..\Teleopti.Ccc.WebBehaviorTest.csproj
:: set tests=/run=Teleopti.Ccc.WebBehaviorTest.PreferencesFeature.AddStandardPreference
set action=nunitexecutionreport

set command=%nunit% %assembly% %tests% /out=TestResult.txt /xml=TestResult.xml 
echo %command%
%command%

set command=%specflow% %action% %project% /out:TestExecutionReport.html
echo %command%
%command%

cmd /c "start TestExecutionReport.html"
