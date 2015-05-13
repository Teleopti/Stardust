:: Runs from [repo]\Teleopti.Ccc.Web\Teleopti.Ccc.Web\WFM

call ..\.node\npm uninstall karma
call ..\.node\npm uninstall karma-chrome-launcher
call ..\.node\npm uninstall karma-jasmine
call ..\.node\npm uninstall karma-cli

call ..\.node\npm install -g karma
call ..\.node\npm install -g karma-teamcity-reporter
call ..\.node\npm install -g karma-chrome-launcher@0.1.10
call ..\.node\npm install -g karma-jasmine
call ..\.node\npm install -g karma-cli
call ..\.node\npm install -g flatten-packages

for /f "tokens=1" %%i in ('..\.node\npm bin -g') do set output=%%i
echo "%output%"

call "%output%\flatten-packages.cmd" "%output%"
call "%output%\karma" start --reporters teamcity --single-run
