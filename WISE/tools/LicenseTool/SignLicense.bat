@echo off

SET KeyFile=\\integration\Installation\Dependencies\Teleopti\LicenseTool\Temporary.tmp
SET LicenceToolPath=\\integration\Installation\Dependencies\Teleopti\LicenseTool
SET Template=%Temp%\Template.xml
SET OutFile=%TEMP%\Out.xml

ECHO ^<^?xml version="1.0" encoding="utf-8"^?^> > %Template%
ECHO ^<License^> >> %Template%
ECHO   ^<CustomerName^>Demo License. For Teleopti internal usage only!^</CustomerName^> >> %Template%
ECHO   ^<ExpirationDate^>2009-03-02T12:00:00^</ExpirationDate^> >> %Template%
ECHO   ^<ExpirationGracePeriod^>P30D^</ExpirationGracePeriod^> >> %Template%
ECHO   ^<MaxActiveAgents^>10000^</MaxActiveAgents^> >> %Template%
ECHO   ^<MaxActiveAgentsGrace^>10^</MaxActiveAgentsGrace^> >> %Template%
ECHO   ^<TeleoptiCCC^> >> %Template%
ECHO     ^<Base^>true^</Base^> >> %Template%
ECHO     ^<AgentSelfService^>true^</AgentSelfService^> >> %Template%
ECHO     ^<ShiftTrades^>true^</ShiftTrades^> >> %Template%
ECHO     ^<Developer^>true^</Developer^> >> %Template%
ECHO     ^<MyTimeWeb^>true^</MyTimeWeb^> >> %Template%
ECHO     ^<AgentScheduleMessenger^>true^</AgentScheduleMessenger^> >> %Template%
ECHO     ^<HolidayPlanner^>true^</HolidayPlanner^> >> %Template%
ECHO     ^<RealtimeAdherence^>true^</RealtimeAdherence^> >> %Template%
ECHO     ^<PerformanceManager^>true^</PerformanceManager^> >> %Template%
ECHO     ^<PayrollIntegration^>true^</PayrollIntegration^> >> %Template%
ECHO   ^</TeleoptiCCC^> >> %Template%
ECHO ^</License^> >> %Template%

::Edit license template
ECHO.
ECHO I will now pop up a template xml-file in Notepad.
ECHO The file will be used as template for the license to be created.
ECHO Please edit and save it to it's current location.
ECHO.
PAUSE
NOTEPAD "%Template%"

::Create local KeyPair
ECHO.
ECHO Creating local KeyPair from keyfile ...
CALL "%LicenceToolPath%\vcWrapper.bat" Teleopti "%KeyFile%"

::Create License
ECHO.
ECHO Creating license ...
%LicenceToolPath%\Teleopti.LicenseTool.exe -Sign="%Template%" -Signed="%OutFile%"

::Show License
ECHO.
ECHO I will now pop up the acctual license file in notepad.
ECHO Please save this file as an .xml-file to another location
ECHO Handle with care!
PAUSE
NOTEPAD "%OutFile%"

::Cleane up
DEL "%OutFile%"
DEL "%Template%"