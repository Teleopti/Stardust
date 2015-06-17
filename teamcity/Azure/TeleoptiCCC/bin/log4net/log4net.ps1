function configure-logging($configFile, $serviceName)
{
	write-host "Load log4net ..."
    Set-log4netConfig($configFile)
    Unblock-File -Path "$log4netPath\log4net.dll"
	Add-Type -Path "$log4netPath\log4net.dll"
	$LogManager = [log4net.LogManager]
	$global:logger = $LogManager::GetLogger("PowerShell");
	if ( (test-path $configFile) -eq $false)
	{
		$message = "WARNING: logging config file not found: log4net.config"
		write-host
		write-host $message -foregroundcolor yellow
		write-host
	}
	else
	{
	    $xmlConfigurator = [log4net.Config.XmlConfigurator]::ConfigureAndWatch($configFile);
    }
	write-host "Load log4net. Done!"
}

function log-info ([string] $message)
{
	write-host $message
	$logger.Info($message);
}

function log-warn ([string] $message)
{
	write-host "WARNING: $message" -foregroundcolor yellow
	$logger.Warn($message);
} 

function log-error ([string] $message)
{
	write-host "ERROR: $message" -foregroundcolor red
	$logger.Error($message);
} 

function Set-log4netConfig($configFile)
{
$NewConfigFileText = @"
<?xml version="1.0"?>
<configuration>
    <log4net>
        <appender name="PowerShellRollingFileAppender" type="log4net.Appender.RollingFileAppender" >
            <param name="File" value="$scriptPath\$ScriptFileName.log" />
            <param name="AppendToFile" value="true" />
            <param name="RollingStyle" value="Size" />
            <param name="MaxSizeRollBackups" value="100" />
            <param name="MaximumFileSize" value="1024KB" />
            <param name="StaticLogFileName" value="true" />
            <lockingModel type="log4net.Appender.FileAppender+MinimalLock" />
            <layout type="log4net.Layout.PatternLayout">
            <param name="ConversionPattern" value="%utcdate %-5p %c %m%n" />
            </layout>
        </appender>
        <root>
            <level value="info" />
        </root>
        <logger name="PowerShell" additivity="false">
            <level value="info" />
            <appender-ref ref="PowerShellRollingFileAppender" />
        </logger>
    </log4net>
</configuration>
"@
    New-Item $configFile -type file -force
    Add-Content $configFile $NewConfigFileText
}




