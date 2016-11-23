

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
             do
             {
                $arrService = Get-Service -Name $ServiceName
                Write-Host "."
                Start-Sleep 3
                $bailOut--
                if ($bailOut -eq 0)
                {
                    Throw "Could not stop service $ServiceName, status remains " + $arrService.Status
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

    #if (ServiceExist($ServiceBus)) {
    #    & sc.exe failure $ServiceBus reset= 0 actions= restart/60000/restart/60000/restart/60000
    #}
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
}

