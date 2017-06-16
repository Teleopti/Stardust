
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
		Write-Host '.'
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
    Write-Host 'Check-HttpStatus: ' $url
	[net.httpWebResponse] $res = $req.getResponse()
	Write-Host 'Response Code:' $res.StatusCode
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
             Write-Host "Stopping " $ServiceName " service" 
             $bailOut = 100
             $sleep = 3
             $totalWait = $bailOut * $sleep

             do
             {
                $arrService = Get-Service -Name $ServiceName
                Write-Host "."
                Start-Sleep 3
                $bailOut--
                if ($bailOut -eq 0)
                {
                    #kill the process by pid
                    Write-Host "we have waited $totalWait secs. Force a kill on $ServiceName, pid: $ServicePID"
                    $ServicePID = (get-wmiobject win32_service | where { $_.name -eq $ServiceName}).processID
                    Stop-Process $ServicePID -Force
                }
             } while ($arrService.Status -ne "Stopped")
             Write-Host "`nService $ServiceName successfully stopped"
         }
         else
         {
            Write-Host "Warning! Service $ServiceName status is:" $arrService.Status
         }
    }
    else {
        Write-Host "Service" $ServiceName " is not installed on this host." 
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
             Write-Host "Starting " $ServiceName " service" 
             $bailOut = 100
             do
             {
                $arrService = Get-Service -Name $ServiceName
                Write-Host "."
                Start-Sleep 3
                $bailOut--
                if ($bailOut -eq 0)
                {
                    Throw "Could not start service $ServiceName, status remains " + $arrService.Status
                }
             } while ($arrService.Status -ne "Running")
             Write-Host "`nService $ServiceName successfully started"
         }
         else
         {
            Write-Host "Warning! Service $ServiceName status is:" $arrService.Status
         }
    }
    else {
        Write-Host "Service" $ServiceName " is not installed on this host." 
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

function AppPools-Start {
    param([bool]$IsAzure)
    if (!$IsAzure) {
		$appcmd="$env:windir\system32\inetsrv\AppCmd.exe"
        
		try
		{
			&$appcmd list apppool /state:Stopped /name:"$=*Teleopti*" /xml | &$appcmd Set apppool /autoStart:true /in
			&$appcmd list apppool /state:Stopped /name:"$=*Teleopti*" /xml | &$appcmd start apppool /in
		}
		Catch {
			$err=$Error[0]
			Write-host "$err"
		}
	}
}


function StopTeleoptiServer
{
	TeleoptiWindowsServices-Stop
	Invoke-Expression -Command:"iisreset /STOP"
	StopWindowsService -ServiceName "W3SVC"
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

function StartTeleoptiServer
{
	param
	(
		[bool] $isAzure = $false
	)

	StartWindowsService -ServiceName "W3SVC"
	Invoke-Expression -Command:"iisreset /START"
	AppPools-Start $isAzure
	TeleoptiWindowsServices-Start
	if ([int]$psversiontable.psversion.major -gt 2)
    {
        $BaseUrl = BaseUrl-get $isAzure
        Write-Host "Waiting for web services to start..."
        $Url = $BaseURL + "web/StardustDashboard/ping"
        $cred = GetCredentials
        WaitForUrl $Url $cred
    }
}

