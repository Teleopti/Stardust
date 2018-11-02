

$restartHelperPath = $PSScriptRoot + "..\Tools\SupportTools\StartStopSystem\RestartHelper.ps1"
. $restartHelperPath



function ReinstallService
{
		param
		(
			$nombreDeServicio,
			$exePath
		)
		
	$Servicio = Get-Service | Where-Object {$_.name -eq $nombreDeServicio}
	if ($Servicio -ne $null)
	{
		log-info "Service $nombreDeServicio already exists, uninstalling..."
		StopWindowsService $nombreDeServicio
		taskkill /F /IM "mmc.exe"
		SC.EXE DELETE $nombreDeServicio
		$Servicio = Get-Service | Where-Object {$_.name -eq $nombreDeServicio}
		if ($Servicio -ne $null)
		{
			log-error "Could not delete $nombreDeServicio"
			log-info "Could not delete $nombreDeServicio"
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
	
	$ServiceBus = "TeleoptiServiceBus"
	& sc.exe failure $ServiceBus reset= 0 actions= restart/60000/restart/60000/restart/60000

	#log-info "Installing Report Viewer"
	#CALL "ReportViewer2010.exe" /norestart /log "%ROOTDIR%\ReportViewer2010-installlog.htm" /install /q
	
	log-info "Install IIS Application Initialization..."
	. "$PSScriptRoot\InstallIisApplicationInitialization.ps1"
	log-info "Install IIS Application Initialization. Done!"
	
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
	& $PSScriptRoot\RegisterEventLogSource.exe "TeleoptiWebAuthApps"
	log-info "Register all service and application names in Event Log. Done!"
	
}
Catch [Exception]
{
    $ErrorMessage = $_.Exception.Message
    #Write-EventLog -LogName Application -Source $JOB -EventID 1 -EntryType Error -Message "$ErrorMessage"
	log-error "Script failed, Check Windows event log for details!"
	log-info "Script failed, Check Windows event log for details!"
	Throw "Script failed, Check Windows event log for details"
}
Finally
{
	log-info "End of Script."
	
}
