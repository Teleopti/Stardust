

$restartHelperPath = $PSScriptRoot + "..\Tools\SupportTools\StartStopSystem\RestartHelper.ps1"
. $restartHelperPath



function ReinstallService
{
		param
		(
			$nombreDeServicio,
			$exePath
		)
		
	$etlService = Get-Service | Where-Object {$_.name -eq $nombreDeServicio}
	if ($etlService -ne $null)
	{
		log-info "Service $nombreDeServicio already exists, uninstalling..."
		StopWindowsService $nombreDeServicio
		SC.EXE DELETE $nombreDeServicio
		$etlService = Get-Service | Where-Object {$_.name -eq $nombreDeServicio}
		if ($etlService -ne $null)
		{
			log-error "Could not delete TeleoptiEtlService"
			# Felhantering
		}
	}
	log-info "Installing $nombreDeServicio ..."
	SC.EXE CREATE $nombreDeServicio binPath=$exePath DisplayName="$nombreDeServicio"
	log-info "Setting $nombreDeServicio to autostart..."
	SC.EXE CONFIG $nombreDeServicio start=Auto
}

##===========
## Main
##===========
Try
{
    [Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime")
	
	#Get local path
    [string]$global:scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
    [string]$global:ScriptFileName = $MyInvocation.MyCommand.Name
    Set-Location $scriptPath
 
	#start log4net
	$log4netPath = $scriptPath + "\log4net"
    Unblock-File -Path "$log4netPath\log4net.ps1"
    . "$log4netPath\log4net.ps1";
    $configFile = new-object System.IO.FileInfo($log4netPath + "\log4net.config");
    configure-logging -configFile "$configFile"
	
	log-info "running: $ScriptFileName"
	
	ReinstallService "TeleoptiEtlService" "E:\approot\Services\ETL\Service\Teleopti.Analytics.Etl.ServiceHost.exe"
	ReinstallService "TeleoptiServiceBus" "E:\approot\Services\ServiceBus\Teleopti.CCC.Sdk.ServiceBus.Host.exe"

	#log-info "Installing Report Viewer"
	#CALL "ReportViewer2010.exe" /norestart /log "%ROOTDIR%\ReportViewer2010-installlog.htm" /install /q
	
	log-info "Install Powershell ISE..."
	. "$PSScriptRoot\InstallPowershell-ISE.ps1"
	log-info "Install Powershell ISE. Done!"

	log-info "Register all service and application names in Event Log..."
	& $PSScriptRoot\RegisterEventLogSource.exe "TeleoptiAnalyticsWebPortal"
	& $PSScriptRoot\RegisterEventLogSource.exe "TeleoptiETLService"
	& $PSScriptRoot\RegisterEventLogSource.exe "TeleoptiETLTool"
	& $PSScriptRoot\RegisterEventLogSource.exe "TeleoptiRtaWebService"
	& $PSScriptRoot\RegisterEventLogSource.exe "TeleoptiSdkWebService"
	& $PSScriptRoot\RegisterEventLogSource.exe "TeleoptiServiceBus"
	& $PSScriptRoot\RegisterEventLogSource.exe "TeleoptiWebApps"
	& $PSScriptRoot\RegisterEventLogSource.exe "TeleoptiWebBroker"
	log-info "Register all service and application names in Event Log. Done!"
	
}
Catch [Exception]
{
    $ErrorMessage = $_.Exception.Message
    Write-EventLog -LogName Application -Source $JOB -EventID 1 -EntryType Error -Message "$ErrorMessage"
	log-error "Script failed, Check Windows event log for details!"
	Throw "Script failed, Check Windows event log for details"
}
Finally
{
	log-info "Done."
	Write-EventLog -LogName Application -Source $JOB -EventID 0 -EntryType Information -Message "$newFiles files synced from: $BlobSource to: $FILEWATCH"

}
