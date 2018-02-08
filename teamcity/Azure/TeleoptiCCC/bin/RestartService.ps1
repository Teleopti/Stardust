
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


function WaitForUrl
{
	param
	(
		$Url,
		$cred
 	)

	$isOk = Check-HttpStatus -url $Url -credentials $cred
	$bailOut = 200

	while ($isOk -eq $false)
	{ 
		sleep 3
		$isOk = Check-HttpStatus -url $Url -credentials $cred
		log-info '.'
		$bailOut--
		if ($bailOut -eq 0)	{ break }
	}
}

function Check-HttpStatus {     
	param(
	[string] $url,
    [System.Net.NetworkCredential]$credentials = $null
	)

	[net.httpWebRequest] $req = [net.webRequest]::create($url)
    $req.Credentials = $credentials;
	$req.Method = "GET"
    log-info 'Check-HttpStatus: ' $url
	[net.httpWebResponse] $res = $req.getResponse()
	log-info 'Response Code:' $res.StatusCode
    $ret = $res.StatusCode -eq "200"
    $res.Close()
    return $ret
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

function StopWindowsService
{
     param($ServiceName)

    if (ServiceExist($ServiceName))
    {
         $arrService = Get-Service -Name $ServiceName
         
         if ($arrService.Status -ne "Stopped")
         {
             $arrService.Stop()
             log-info "Stopping " $ServiceName " service"
			 
             $bailOut = 100
             $sleep = 3
             $totalWait = $bailOut * $sleep

             do
             {
                $arrService = Get-Service -Name $ServiceName
                log-info "."
                Start-Sleep 3
                $bailOut--
                if ($bailOut -eq 0)
                {
                    #kill the process by pid
                    log-info "we have waited $totalWait secs. Force a kill on $ServiceName, pid: $ServicePID"
                    $ServicePID = (get-wmiobject win32_service | where { $_.name -eq $ServiceName}).processID
                    Stop-Process $ServicePID -Force
                }
             } while ($arrService.Status -ne "Stopped")
             log-info "`nService $ServiceName successfully stopped"
         }
         else
         {
            log-info "Warning! Service $ServiceName status is:" $arrService.Status
         }
    }
    else {
        log-info "Service" $ServiceName " is not installed on this host." 
    }
 }

 function StartWindowsService
{
    param($ServiceName)

    if (ServiceExist($ServiceName))
    {
         $arrService = Get-Service -Name $ServiceName
         
         if ($arrService.Status -ne "Running")
         {
             $arrService.Start()
             log-info "Starting " $ServiceName " service" 
             $bailOut = 100
             do
             {
                $arrService = Get-Service -Name $ServiceName
                log-info $arrservice.status
                Start-Sleep 3
                $bailOut--
                if ($bailOut -eq 0)
                {
                    Throw "Could not start service $ServiceName, status remains " + $arrService.Status
                }
             } while ($arrService.Status -ne "Running")
             log-info "`nService $ServiceName successfully started"
         }
         else
         {
            log-info "Warning! Service $ServiceName status is:" $arrService.Status
         }
    }
    else {
        log-info "Service" $ServiceName " is not installed on this host." 
    }
 }

 function TeleoptiWindowsServices-Stop {
    $EtlService = "TeleoptiEtlService"
    $ServiceBus = "TeleoptiServiceBus"

	StopWindowsService -ServiceName $ServiceBus
	StopWindowsService -ServiceName $EtlService
}

function TeleoptiWindowsServices-Start {
    $EtlService = "TeleoptiEtlService"
    $ServiceBus = "TeleoptiServiceBus"

	StartWindowsService -ServiceName $ServiceBus
	StartWindowsService -ServiceName $EtlService

    if (ServiceExist($ServiceBus)) {
        & sc.exe failure $ServiceBus reset= 0 actions= restart/60000/restart/60000/restart/60000
    }
}

function StopTeleoptiServer
{
    param
	(
        [bool] $iisInstalled
	)
	TeleoptiWindowsServices-Stop
	if ($iisInstalled) {
		Invoke-Expression -Command:"iisreset /STOP"
		StopWindowsService -ServiceName "W3SVC"
	}
}

function GetCredentials
{
	$username = "tfsintegration"
	$domain = "toptinet"
	$password = "m8kemew0rk"
	$secstr = New-Object -TypeName System.Security.SecureString
	$password.ToCharArray() | ForEach-Object {$secstr.AppendChar($_)}
	$AdminCredentials = new-object -typename System.Management.Automation.PSCredential -argumentlist $domain\$username, $secstr
	return $AdminCredentials
}

function CheckThisInstanceWeb
{
	param 
	(
		$SubSite
	)
	
	$localip = test-connection $env:computername -count 1 | select Ipv4Address
	$localip = $localip.IPV4Address.IPAddressToString

	$BaselUrl = "https://$localip/$SubSite"

	add-type @"
		using System.Net;
		using System.Security.Cryptography.X509Certificates;
		public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint srvPoint, X509Certificate certificate,
            WebRequest request, int certificateProblem) {
            return true;
			}
		}
"@

	[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy

	$statusCode = wget $BaselUrl | % {$_.StatusCode} 
	
	return $statusCode

}

function StartTeleoptiServer
{
	param
	(
        [bool] $iisInstalled
	)
	
	if ($iisInstalled) {
		StartWindowsService -ServiceName "W3SVC"
		Invoke-Expression -Command:"iisreset /START"
       
		
	}

    #Make sure W3SVC is started before starting ServiceBus and ETL
    do
	{ 
		log-info "." 
	}
	until (($WaitforLocalWeb = CheckThisInstanceWeb -SubSite Web) -eq "200")
	
	log-info "Local Web Service on this Instance is up..."
	
	
    #Starting ServiceBus and ETL 
    TeleoptiWindowsServices-Start
	
	do
	{ 
		log-info "." 
	}
	until (($WaitforLocalTeleoptiServices = CheckThisInstanceWeb -SubSite "web/StardustDashboard/ping") -eq "200")
	
	log-info "Teleopti Services on this instance is started..."
   
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
    
    
    $webOffline = "..\..\..\..\sitesroot\3\app_offline.htm"
    $administrationOffline = "..\..\..\..\sitesroot\8\app_offline.htm"
	$rtaOffline = "..\..\..\..\sitesroot\5\app_offline.htm"
		
		if (Test-Path "$rtaOffline") {
			log-info "removing" $rtaOffline
			Remove-Item $rtaOffline
		}
    
	    if (Test-Path "$webOffline") {
			log-info "removing" $webOffline
			Remove-Item $webOffline
		}
		
		if (Test-Path "$administrationOffline") { 
        
			log-info "removing" $administrationOffline
			Remove-Item $administrationOffline
		}
}

##Main

[Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime")
$JOB = "Teleopti.Ccc.RestartService"
	
	#Get local path
    [string]$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
    [string]$ScriptFileName = $MyInvocation.MyCommand.Name
    Set-Location $scriptPath

 	#start log4net
	$log4netPath = $scriptPath + "\log4net"
    Unblock-File -Path "$log4netPath\log4net.ps1"
    . "$log4netPath\log4net.ps1";
    $configFile = new-object System.IO.FileInfo($log4netPath + "\log4net.config");
    configure-logging -configFile "$configFile" -serviceName "$serviceName"
	
	log-info "running: $ScriptFileName"

Try
{
	    
    EventlogSource-Create "$JOB"

    $iis = Get-WmiObject Win32_Service -Filter "Name = 'W3SVC'"
    $iisInstalled = $false;

    if ($iis.Name -eq 'W3SVC') {
        log-info 'Found iis installed.'
        import-module WebAdministration
        $iisInstalled = $true;
        RemoveAppOfflinePage 
    }
	
	StopTeleoptiServer $iisInstalled
	StartTeleoptiServer $iisInstalled

}

Catch [Exception]
{
	$ErrorMessage = $_.Exception.Message
	#$_.Exception | format-list -force
	Write-EventLog -LogName Application -Source $JOB -EventID 1 -EntryType Error -Message "$ErrorMessage"
	Throw "Script failed, Check Windows event log for details"
}




