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

function DataSourceName-get {
    if ($debug) {
	$DataSourceName = "teleopticcc-dev"
    }
    else {
	$DataSourceName = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.DataSourceName")
    }
	return $DataSourceName
}

function TeleoptiWindowsServices-Stop()
{
    if ($debug) {
        $EtlService = "TeleoptiEtlService"
        $ServiceBus = "TeleoptiServiceBus"
    } else {
        $EtlService = "AnalyticsEtlService"
        $ServiceBus = "TeleoptiServiceBus"
    }
	fnServiceStop -ServiceName $ServiceBus
	fnServiceStop -ServiceName $EtlService
}

function TeleoptiWindowsServices-Start()
{
    if ($debug) {
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

function TeleoptiWebSites-HttpGet([string]$DataSourceName)
{
    $backplane = "Broker.backplane/backplane/signalr/negotiate"
    $broker    = "broker/signalr/negotiate"
    $sdk       = "SDK/TeleoptiCCCSdkService.svc"
    $rta       = "RTA/TeleoptiRtaService.svc"
    $web       = "Web/MyTime/Authentication"

    if ($debug) {
    $BaseUrl = "http://localhost/TeleoptiCCC/"
    } else {
    $BaseUrl = "https://" + $DataSourceName +".teleopticloud.com/"
    }

    $backplane = $BaseUrl + $backplane
    $broker    = $BaseUrl + $broker
    $sdk       = $BaseUrl + $sdk
    $rta       = $BaseUrl + $rta
    $web       = $BaseUrl + $web

    $UrlArray = @($backplane,$broker,$web,$sdk,$rta)


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
    $req = [system.Net.WebRequest]::Create($url)
    try {
        Get-Webclient $url #try wake her up!
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
function main {
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.Web.Administration")
$JOB = "Teleopti.Ccc.RestartSystem"

    Try
    {
    	import-module WebAdministration

        #create event log source
        EventlogSource-Create "$JOB"

        $computer = gc env:computername
        if ($computer.ToUpper().StartsWith("TELEOPTI")) {
        $debug = $true
        }

        [Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime")

	    ##test if admin
	    If (!(Test-Administrator($myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()))) {
		    throw "User is not Admin!"
	    }

        $DataSourceName = DataSourceName-get
	    TeleoptiWindowsServices-Stop
        IIS-Restart
        write-host "sleep 5 seconds for IIS to restart ..."
        Start-Sleep -Seconds 5       
        TeleoptiWebSites-HttpGet $DataSourceName
        TeleoptiWindowsServices-Start
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
}