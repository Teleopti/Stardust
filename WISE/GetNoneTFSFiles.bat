::CCC_forecast
XCOPY "%DEPENDENCIESSRC%\ccc7_forecast\SQLEXPR.EXE" "%WISESOURCEFILE%\Wise\ccc7_forecast\" /D /Y
XCOPY "%DEPENDENCIESSRC%\ccc7_forecast\ForecastDatabase\TeleoptiCCC_Forecasts.BAK" "%WISESOURCEFILE%\ForecastDatabase\" /D /Y

::CCC_server
XCOPY "%DEPENDENCIESSRC%\ccc7_server\DemoDatabase\TeleoptiAnalytics_Demo.bak" "%WISESOURCEFILE%\DemoDatabase\" /D /Y
XCOPY "%DEPENDENCIESSRC%\ccc7_server\DemoDatabase\TeleoptiCCC7_Demo.bak" "%WISESOURCEFILE%\DemoDatabase\" /D /Y
XCOPY "%DEPENDENCIESSRC%\ccc7_server\DemoDatabase\TeleoptiCCC7Agg_Demo.BAK" "%WISESOURCEFILE%\DemoDatabase\" /D /Y
XCOPY "%DEPENDENCIESSRC%\ccc7_server\ReportViewer2010.exe" "%WISESOURCEFILE%\Wise\ccc7_server\" /D /Y
XCOPY "%DEPENDENCIESSRC%\ccc7_server\RegisterEventLogSource.exe" "%WISESOURCEFILE%\Wise\ccc7_server\Logs\" /D /Y
XCOPY "%DEPENDENCIESSRC%\ccc7_server\ntrights.exe" "%WISESOURCEFILE%\Wise\ccc7_server\Logs\" /D /Y
XCOPY "%DEPENDENCIESSRC%\ccc7_server\sqlio.exe" "%WISESOURCEFILE%\SupportTools\SQLServerPerformance\SQLIO\" /D /Y

::WISE
ROBOCOPY "%DEPENDENCIESSRC%\images" "%WISESOURCEFILE%\images" /MIR