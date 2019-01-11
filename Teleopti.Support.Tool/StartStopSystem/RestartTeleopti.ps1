#powershell v2 won't get $PSScriptroot, revert back to old style...
if (!$PSScriptroot)
{
    $PSScriptroot = split-path -parent $MyInvocation.MyCommand.Definition
}

. "$PSScriptroot\RestartHelper.ps1"

##===========
## Functions
##===========
function ServiceExist{
    param($ServiceName)
    if (Get-Service $ServiceName -ErrorAction SilentlyContinue)
    {
        return $true
    }
    else
    {
        return $false
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

  
# Not used ? /Henry
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

function RemoveAppOfflinePage {
    param([bool]$IsAzure)
    if ($IsAzure) {
        $webOffline = "..\..\..\..\sitesroot\3\app_offline.htm"
        $administrationOffline = "..\..\..\..\sitesroot\8\app_offline.htm"
		$rtaOffline = "..\..\..\..\sitesroot\5\app_offline.htm"
		if (Test-Path "$rtaOffline") {
			write-host "removing" $rtaOffline
			Remove-Item $rtaOffline
		}
    }else{
        $webOffline = "..\..\TeleoptiCCC\Web\app_offline.htm"
        $administrationOffline = "..\..\TeleoptiCCC\Administration\app_offline.htm"
    }
    if (Test-Path "$webOffline") {
        write-host "removing" $webOffline
        Remove-Item $webOffline
    }
    if (Test-Path "$administrationOffline") { 
        write-host "removing" $administrationOffline
        Remove-Item $administrationOffline
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
   $scriptpath = $myInvocation.MyCommand.Definition
   Start-Process -FilePath PowerShell.exe  -Verb runAs -ArgumentList "& '$scriptPath'"
   exit
   }
 
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.Web.Administration") | out-null
[Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime")
$JOB = "Teleopti.Ccc.RestartSystem"

Try
{
	##test if admin
	If (!(Test-Administrator($myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()))) {
		throw "User is not Admin!"
	}

    $isAzure = fnIsAzure
    if ($isAzure){
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    }
    EventlogSource-Create "$JOB"

    $iis = Get-WmiObject Win32_Service -Filter "Name = 'W3SVC'"
    $iisInstalled = $false;

    if ($iis.Name -eq 'W3SVC') {
        Write-host 'Found iis installed.'
        import-module WebAdministration
        $iisInstalled = $true;
        RemoveAppOfflinePage $isAzure
    }
	
	StopTeleoptiServer $iisInstalled
	StartTeleoptiServer $iisInstalled $isAzure

}

Catch [Exception]
{
	$ErrorMessage = $_.Exception.Message
	Write-EventLog -LogName Application -Source $JOB -EventID 1 -EntryType Error -Message "$ErrorMessage"
	Throw "Script failed, Check Windows event log for details"
}
