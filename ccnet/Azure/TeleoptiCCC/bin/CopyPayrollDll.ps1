##===========
## Functions
##===========
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

function Roby-Copy
{
    Param(
      [string]$scrFolder,
      [string]$destFolder
      )
      
	## Options to be added to RoboCopy
	$ROBOOPTIONS = @("/E")

	## Wrap arguments for robocopy
	$roboArgs = @("$scrFolder","$destFolder",$ROBOOPTIONS)

	## Run robocopy from Inbox to FileWatch
	& robocopy @roboArgs  | out-null
    $RoboExitCode = $LastExitCode
    
    if ($RoboExitCode -ge 8) {
        throw "RoboCopy generated an error!"
    }
    return $RoboExitCode
}

function ServiceCheckAndStart{
    param($ServiceName)
    $arrService = Get-Service -Name $ServiceName
    if ($arrService.Status -ne "Running"){
        Start-Service $ServiceName
     }
}

function Blobsource-get {
    $computer = gc env:computername
    
	## Local debug values
    if ($computer.ToUpper().StartsWith("TELEOPTI")) {
	$BlobPath = "http://teleopticcc7.blob.core.windows.net/"
	$ContainerName="teleopticcc/Payroll"
	$AccountKey = "IqugZC5poDWLu9wwWocT42TAy5pael77JtbcZtnPcm37QRThCkdrnzOh3HEu8rDD1S8E6dU5D0aqS4sJA1BTxQ=="
	$DataSourceName = "teleopticcc-dev"
    }
    ## Get environment varaibles
    else {
    [Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime")
	$BlobPath = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.BlobPath")
	$ContainerName = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.ContainerName")
	$AccountKey = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.AccountKey")
	$DataSourceName = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.DataSourceName")
    }

	[string]$returnValue = $BlobPath + $ContainerName + "/" + $DataSourceName
	return $returnValue
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
Param(
  [string]$directory
  )
$TeleoptiServiceBus = "Teleopti Service Bus"
$computer = gc env:computername

## Name of the job, name of source in Windows Event Log
$JOB = "Teleopti.Ccc.BlobStorageCopy"

Try
{
	##test if admin
	If (!(Test-Administrator($myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()))) {
		throw "User is not Admin!"
	}

    #create event log source
    EventlogSource-Create "$JOB"
   
	## Local debug values
    if ($computer.ToUpper().StartsWith("TELEOPTI")) {
	$BlobPath = "http://teleopticcc7.blob.core.windows.net/"
	$ContainerName="teleopticcc/Payroll"
	$AccountKey = "IqugZC5poDWLu9wwWocT42TAy5pael77JtbcZtnPcm37QRThCkdrnzOh3HEu8rDD1S8E6dU5D0aqS4sJA1BTxQ=="
	$DataSourceName = "teleopticcc-dev"
    }
    ## Get environment varaibles
    else {
    [Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime")
	$BlobPath = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.BlobPath")
	$ContainerName = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.ContainerName")
	$AccountKey = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.AccountKey")
	$DataSourceName = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.DataSourceName")
    }
   
    $BlobSource = $BlobPath + $ContainerName + "/" + $DataSourceName

	## Destination directory. Files in this directory will mirror the source directory. Extra files will be deleted!
	$DESTINATION = "c:\temp\PayrollInbox"
	if (-not(test-path -path $DESTINATION))
	{
		mkdir $DESTINATION
	}

	## FileWatch destination directory
	$FILEWATCH = $directory + "\..\Services\ServiceBus\Payroll.DeployNew"

	## Options to be added to AzCopy
	$OPTIONS = @("/S","/XO","/Y","/sourceKey:$AccountKey")

	## Wrap all above arguments
	$cmdArgs = @("$BlobSource","$DESTINATION",$OPTIONS)

	$AzCopyExe = $directory + "\ccc7_azure\AzCopy\AzCopy.exe"
	$AzCopyExe

	## Start the azcopy with above parameters and log errors in Windows Eventlog.
	& $AzCopyExe @cmdArgs
    $AzExitCode = $LastExitCode
    
    if ($LastExitCode -ne 0) {
        throw "AsCopy generated an error!"
    }
    
    $RoboExitCode = Roby-Copy $DESTINATION $FILEWATCH
    
	##one or more files are new, log info to Eventlog and restart serviceBus
	If ($RoboExitCode -ge 1) {
        Write-EventLog -LogName Application -Source $JOB -EventID 0 -EntryType Information -Message "$BlobSource and $FILEWATCH in sync."
    	Stop-Service -name $TeleoptiServiceBus
    	write-host "-------------" -ForegroundColor blue
    	Start-Service -name $TeleoptiServiceBus
	}
    
    ##safty, if the service is down for some reason, restart it
    ServiceCheckAndStart $TeleoptiServiceBus;
}
Catch [Exception]
{
    $ErrorMessage = $_.Exception.Message
    Write-EventLog -LogName Application -Source $JOB -EventID 1 -EntryType Error -Message "$ErrorMessage"
	Throw "Script failed, Check Windows event log for detils"
}
Finally
{
    Write-Host "done"
}
}