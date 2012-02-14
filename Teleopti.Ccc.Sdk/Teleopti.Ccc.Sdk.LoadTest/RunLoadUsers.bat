@echo off

rem http://teleopti625/TeleoptiCCC/SDK/TeleoptiCCCSDKservice.svc
rem http://vmraptortest/TeleoptiCCC/SDK/TeleoptiCCCSDKservice.svc
rem http://raptortest/TeleoptiCCC/SDK/TeleoptiCCCSDKservice.svc
rem http://localhost:1335/TeleoptiCCCSDKservice.svc

Teleopti.Ccc.Sdk.LoadTest.exe http://vmraptortest/TeleoptiCCC/SDK/TeleoptiCCCSDKservice.svc 100 2 60 1
pause
