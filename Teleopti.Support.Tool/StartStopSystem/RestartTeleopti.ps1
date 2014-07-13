##===========
## Functions
##===========

function ServiceCheckAndStart{
    param($ServiceName)
    $arrService = Get-Service -Name $ServiceName
    if ($arrService.Status -ne "Running"){
        Start-Service $ServiceName
     }
}
function Test-Administrator
{
	[CmdletBinding()]
	param($currentUser)
	$myWindowsPrincipal=new-object System.Security.Principal.WindowsPrincipal($myWindowsID)

	# Get the security principal for the Administrator role
	$adminRole=[System.Security.Principal.WindowsBuiltInRole]::Administrator

	# Check to see if we are currently running "as Administrator"
	return ($myWindowsPrincipal.IsInRole($adminRole))
}

function BaseUrl-get {
    param([bool]$IsAzure)
    if ($IsAzure) {
        $DataSourceName = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.DataSourceName")
        $BaseUrl = "https://" + $DataSourceName +".teleopticloud.com/"
    }
    else
    {
         $BaseUrl = fnDnsAlias-Get
         $BaseUrl = $BaseUrl + "TeleoptiWFM/"
    }
	return $BaseUrl
}

function fnDnsAlias-Get {
     if ("${Env:ProgramFiles(x86)}") {
         $DNS_ALIAS = (Get-Item HKLM:\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC\InstallationSettings).GetValue("DNS_ALIAS")
         }
     else {
        $DNS_ALIAS = (Get-Item HKLM:\SOFTWARE\Teleopti\TeleoptiCCC\InstallationSettings).GetValue("DNS_ALIAS")
        }
    Return $DNS_ALIAS
}
function fnIsAzure {
    try
    {
    	$DataSourceName = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.DataSourceName")
        Return $true
    }
    catch
    {
        Return $false
    }
}

function TeleoptiWindowsServices-Stop {
param([bool]$IsAzure)
    if (!$isAzure) {
        $EtlService = "TeleoptiEtlService"
        $ServiceBus = "TeleoptiServiceBus"
    } else {
        $EtlService = "AnalyticsEtlService"
        $ServiceBus = "TeleoptiServiceBus"
    }
	fnServiceStop -ServiceName $ServiceBus
	fnServiceStop -ServiceName $EtlService
}

function TeleoptiWindowsServices-Start {
param([bool]$IsAzure)
    if (!$isAzure) {
        $EtlService = "TeleoptiEtlService"
        $ServiceBus = "TeleoptiServiceBus"
    } else {
        $EtlService = "AnalyticsEtlService"
        $ServiceBus = "TeleoptiServiceBus"
    }
	fnServiceStart -ServiceName $ServiceBus
	fnServiceStart -ServiceName $EtlService
}

function fnServiceStart{
     param($ServiceName)
     $arrService = Get-Service -Name $ServiceName
     if ($arrService.Status -ne "Running"){
         Start-Service $ServiceName
         Write-Host "Starting " $ServiceName " service" 
         " ---------------------- " 
         " Service is now started"
         }
     if ($arrService.Status -eq "running"){ 
        Write-Host "$ServiceName service is already started"
     }
 }
 
function fnServiceStop{
     param($ServiceName)
     $arrService = Get-Service -Name $ServiceName
     if ($arrService.Status -ne "Stopped"){
         Stop-Service $ServiceName
         Write-Host "Stopping " $ServiceName " service" 
         " ---------------------- " 
         " Service is now stopped"
         }
     if ($arrService.Status -eq "Stopped"){ 
        Write-Host "$ServiceName service is already stopped"
     }
 }
  
function IIS-Restart {
    Invoke-Expression -Command:"iisreset /STOP"
    Invoke-Expression -Command:"iisreset /START"
    fnServiceStart -ServiceName "W3SVC"
}

function AppPools-Start {
    param([bool]$IsAzure)
    if (!$IsAzure) {

        $AppPoolArray = @(
                "Teleopti ASP.NET v4.0",
                "Teleopti ASP.NET v4.0 Web",
                "Teleopti ASP.NET v4.0 Broker",
                "Teleopti ASP.NET v4.0 SDK",
                "Teleopti ASP.NET v4.0 RTA"
                )

        for ($i=0; $i -lt $AppPoolArray.length; $i++) {
            try
            {
				$AppPoolName = $AppPoolArray[$i]
				&"$env:windir\system32\inetsrv\AppCmd.exe" Start AppPool "$AppPoolName"
				&"$env:windir\system32\inetsrv\AppCmd.exe" Set Apppool "$AppPoolName" /autoStart:true
            }
            Catch {
                $err=$Error[0]
                Write-host "$err"
            }
        }
	}
}

function TeleoptiWebSites-HttpGet([string]$BaseUrl)
{
    $AuthenticationBridge = "AuthenticationBridge"
	$WindowsIdentityProvider = "WindowsIdentityProvider"
	$backplane = "Broker.backplane/backplane/signalr/negotiate"
    $broker    = "broker/signalr/negotiate"
    $sdk       = "SDK/TeleoptiCCCSdkService.svc"
    $rta       = "RTA/TeleoptiRtaService.svc"
    $web       = "Web/MyTime/Authentication"

    $AuthenticationBridge = $BaseUrl + $AuthenticationBridge
	$WindowsIdentityProvider = $BaseUrl + $WindowsIdentityProvider
	$backplane = $BaseUrl + $backplane
    $broker    = $BaseUrl + $broker
    $sdk       = $BaseUrl + $sdk
    $rta       = $BaseUrl + $rta
    $web       = $BaseUrl + $web

    $UrlArray = @($AuthenticationBridge,$WindowsIdentityProvider,$backplane,$broker,$web,$sdk,$rta)


    for ($i=0; $i -lt $UrlArray.length; $i++) {
        Write-Host $UrlArray[$i]
        try
        {
            fnBrowseUrl $UrlArray[$i]
        }
        Catch {
            $err=$Error[0]
            Write-host "$err"
        }
    }
}

function fnBrowseUrl ([string]$url)
{
	#Get-Webclient $url #skip this for now, doesn't make any difference
    $req = [system.Net.WebRequest]::Create($url)
    try {
        $res = $req.GetResponse()
    }
    catch [System.Net.WebException] {
       $res = $_.Exception.Response
     }
     
    $intStatusCode = [int]$res.StatusCode
    Write-host "http status is: " $intStatusCode
}

Function Get-Webclient ([string]$url) {

    $WebClient = New-Object System.Net.WebCLient
    if ($url.Contains("Web/MyTime")) {
        $webClient.Headers.Add("user-agent", 'Mozilla/5.0 (iPhone; U; CPU iPhone OS 4_3_2 like Mac OS X; en-us) AppleWebKit/533.17.9 (KHTML, like Gecko) Version/5.0.2 Mobile/8H7 Safari/6533.18.5')
    }
    $Results = $WebClient.DownloadString($url)
}

function EventlogSource-Create {
    param([string]$EventSourceName)
    $type = "Application"
    #create event log source
    if ([System.Diagnostics.EventLog]::SourceExists("$EventSourceName") -eq $false) {
        [System.Diagnostics.EventLog]::CreateEventSource("$EventSourceName", $type)
        }
}

##===========
## Main
##===========
# Get the ID and security principal of the current user account
$myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()
$myWindowsPrincipal=new-object System.Security.Principal.WindowsPrincipal($myWindowsID)
 
# Get the security principal for the Administrator role
$adminRole=[System.Security.Principal.WindowsBuiltInRole]::Administrator
 
# Check to see if we are currently running "as Administrator"
if ($myWindowsPrincipal.IsInRole($adminRole))
   {
   # We are running "as Administrator" - so change the title and background color to indicate this
   $Host.UI.RawUI.WindowTitle = $myInvocation.MyCommand.Definition + "(Elevated)"
   clear-host
   }
else
   {
   # We are not running "as Administrator" - so relaunch as administrator
   
   # Create a new process object that starts PowerShell
   $newProcess = new-object System.Diagnostics.ProcessStartInfo "PowerShell";
   
   # Specify the current script path and name as a parameter
   $newProcess.Arguments = $myInvocation.MyCommand.Definition;
   
   # Indicate that the process should be elevated
   $newProcess.Verb = "runas";
   
   # Start the new process, in elevated mode
   [System.Diagnostics.Process]::Start($newProcess);
   
   # Exit from the current, non elevated process
   exit
   }
 
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.Web.Administration")
[Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime")
$JOB = "Teleopti.Ccc.RestartSystem"

Try
{
	##test if admin
	If (!(Test-Administrator($myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()))) {
		throw "User is not Admin!"
	}

	import-module WebAdministration

	EventlogSource-Create "$JOB"

	$isAzure = fnIsAzure
	$BaseUrl = BaseUrl-get $isAzure
	TeleoptiWindowsServices-Stop $isAzure
	IIS-Restart
	write-host "sleep 5 seconds for IIS to restart ..."
	Start-Sleep -Seconds 5       
	AppPools-Start $isAzure
	TeleoptiWebSites-HttpGet $BaseUrl
	TeleoptiWindowsServices-Start $isAzure
}

Catch [Exception]
{
	$ErrorMessage = $_.Exception.Message
	Write-EventLog -LogName Application -Source $JOB -EventID 1 -EntryType Error -Message "$ErrorMessage"
	Throw "Script failed, Check Windows event log for details"
}
Finally
{
	Write-Host "done"
}