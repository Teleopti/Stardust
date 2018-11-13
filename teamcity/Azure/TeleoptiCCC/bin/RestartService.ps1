Param 
(
	$StopOnly = $false
)

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

function BaseUrl-get {
    
    $DataSourceName = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.DataSourceName")
    $BaseUrl = "https://" + $DataSourceName +".teleopticloud.com/"
    
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
         
		 $bailOut = 100

         while ($arrService.Name -ne $ServiceName)
         {
            log-info "Waiting for '$ServiceName' to be accessible..."
            $arrService = Get-Service -Name $ServiceName
            Start-sleep 3
			$bailOut--
         }
		 
		 if ($bailOut -eq 0) 
		 {
			log-info "Could not get access to service: '$ServiceName'"
		 }
		 
		 
         if ($arrService.Status -ne "Stopped")
         {
             $arrService.Stop()
             log-info "Stopping '$ServiceName' service"
			 
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
             log-info "Service '$ServiceName' successfully stopped"
         }
         else
         {
            $status = $arrService.Status
			log-info "Warning! Service $ServiceName status is: $status"
         }
    }
    else {
        log-info "Service '$ServiceName' is not installed on this host." 
    }
 }

 function StartWindowsService
{
    param($ServiceName)

    if (ServiceExist($ServiceName))
    {
         $arrService = Get-Service -Name $ServiceName
         
		 $bailOut = 100
		 while ($arrService.Status -ne "Stopped")
		 {
			$arrService = Get-Service -Name $ServiceName
			
			if ($arrService.Status -eq "Running")
			{
				$status = $arrService.status
				log-info "Warning! Service $ServiceName status is: $Status" 
				log-error "Warning! Service $ServiceName status is: $Status"
				Return
			}
			
			$status = $arrService.status
			log-info "Waiting for service: '$ServiceName' status to be Stopped. Current status is: $status"
			$bailOut --
			Start-Sleep 5
			
			if ($bailOut -eq 0)
                {
                    $status = $arrService.status
                    log-info "'$ServiceName' never entered stopped state. Status remains: $status"
					log-error "'$ServiceName' never entered stopped state. Status remains: $status"
					Throw "'$ServiceName' never entered stopped state. Status remains: $status"
                }
		 }
		 
		 $status = $arrService.status
		 log-info "'$ServiceName' is: '$status'"
		 
		          
		 if ($arrService.Status -eq "Stopped")
         {
             $arrService.Start()
             log-info "Starting '$ServiceName'..." 
             $bailOut = 100
             do
             {
                $arrService = Get-Service -Name $ServiceName
				$status = $arrService.status
                log-info "Current status of '$ServiceName' is: $status"
                Start-Sleep 5
                $bailOut--
                if ($bailOut -eq 0)
                {
                    $status = $arrService.status
					log-info "Could not start service $ServiceName, status remains: $Status" 
					log-error "Could not start service $ServiceName, status remains: $Status" 
					Throw "Could not start service $ServiceName, status remains: $Status" 
                }
             } while ($arrService.Status -ne "Running")
             log-info "`nService $ServiceName successfully started"
         }
         
    }
    else {
        log-info "Service" $ServiceName " is not installed on this host."
		log-error "Service" $ServiceName " is not installed on this host."
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
		#Invoke-Expression -Command:"iisreset /STOP"
		StopWindowsService -ServiceName "W3SVC"
	}
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

        #C# class to create callback
        $code = @"
        public class SSLHandler
        {
             public static System.Net.Security.RemoteCertificateValidationCallback GetSSLHandler()
            {

             return new System.Net.Security.RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
                }

        }
"@

#compile the class
Add-Type -TypeDefinition $code

#disable checks using new class
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = [SSLHandler]::GetSSLHandler()
[Net.ServicePointManager]::SecurityProtocol = 'Tls12'

try
{
    $statusCode = wget $BaselUrl -UseBasicParsing -ErrorAction SilentlyContinue

}catch [System.Net.WebException] {
    $StatusCode = $_.Exception.Response
    log-info $_.Exception
	return $null
} finally {
   #enable checks again
   [System.Net.ServicePointManager]::ServerCertificateValidationCallback = $null
}
	
	$StatusCode = $statusCode.StatusCode
	log-info "Wget return: '$statusCode'"
	return $statusCode

}

function CheckPublicWeb
{
	param 
	(
		$PublicUrl
	)
	
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
	[Net.ServicePointManager]::SecurityProtocol = 'Tls12'

	log-info "Url to check: '$PublicUrl'"
	$statusCode = $null
	
	try {
		$statusCode = wget $PublicUrl -UseBasicParsing -ErrorAction SilentlyContinue
	}
	catch [System.Net.WebException] {
		$StatusCode = $_.Exception.Response
	}
	catch {
		log-info $_.Exception
		return $null
	}
	$StatusCode = $statusCode.StatusCode
	log-info "Wget return: '$statusCode'"
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
		#Invoke-Expression -Command:"iisreset /START"
    }

    #Make sure local W3SVC on this instance is started before starting ServiceBus and ETL
    log-info ""
	do
	{ 
		log-info "Waiting for local web site '/Web' on this instance to become responsive..."
		Start-Sleep 5
	}
	until (($WaitforLocalWeb = CheckThisInstanceWeb -SubSite "Web") -eq "200")
	
	log-info "Local 'Web' Service on this Instance is up..."
	
	do
	{ 
		log-info "Waiting for local web site 'Web/ToggleHandler/AllToggles' on this instance to become responsive..."
		Start-Sleep 5
	}
	until (($WaitforLocalWeb = CheckThisInstanceWeb -SubSite "Web/ToggleHandler/AllToggles") -eq "200")
	
	log-info "local url: 'Web/ToggleHandler/AllToggles' on this instance is accessible..."

	
	#Checking public URL is responding:
	$BaseUrl = BaseUrl-get
    log-info "Waiting for web services to start..."
    $Url = $BaseURL + "web/"
    do
	{ 
		log-info "Waiting for public web site '$Url' to become responsive..."
		Start-Sleep 5		
	}
	until (($WaitforLocalWeb = CheckPublicWeb -PublicUrl $Url) -eq "200")
	
	log-info "Public url: '$Url' is accessible..."
		
	
	#Starting ServiceBus and ETL 
    TeleoptiWindowsServices-Start
	
	do
	{ 
		log-info "Waiting for local web site 'web/StardustDashboard/ping' on this instance to become responsive..."
		Start-Sleep 5
	}
	until (($WaitforLocalTeleoptiServices = CheckThisInstanceWeb -SubSite "web/StardustDashboard/ping") -eq "200")
	
	log-info "Teleopti Services on this instance is started..."
	
	#Checking public URL is responding:
	$BaseUrl = BaseUrl-get
    log-info "Waiting for Teleopti Services to start..."
    $Url = $BaseURL + "web/StardustDashboard/ping"
	do
	{ 
		log-info "Waiting for public web site '$Url' on this instance to become responsive..." 
		Start-Sleep 5
	}
	until (($WaitforLocalWeb = CheckPublicWeb -PublicUrl $Url) -eq "200")
	
	log-info "Public url: '$Url' is accessible..."
 
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
    [string]$global:scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
    [string]$global:ScriptFileName = $MyInvocation.MyCommand.Name
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
	
	if ($StopOnly -eq $True) 
	{
		log-info "StopOnly param is intentionally set to 'True', will stop services and start them from a scheduled task!"
	}
	
	if ($StopOnly -eq $false) 
	{
		StartTeleoptiServer $iisInstalled
		log-info "IIS and Teleopti services started..."
	}

}

Catch [Exception]
{
	$ErrorMessage = $_.Exception.Message
	#$_.Exception | format-list -force
	Write-EventLog -LogName Application -Source $JOB -EventID 1 -EntryType Error -Message "$ErrorMessage"
	log-info "Script failed: $ErrorMessage"
	log-error "Script failed: $ErrorMessage"
	Throw "Script failed, Check Windows event log for details"
}




