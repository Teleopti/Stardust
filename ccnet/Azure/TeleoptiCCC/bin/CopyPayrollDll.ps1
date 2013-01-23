Param(
  [string]$directory
  )
[Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime")

##===========
## Functions
##===========
function Test-Administrator  
{  
    $user = [Security.Principal.WindowsIdentity]::GetCurrent();
    (New-Object Security.Principal.WindowsPrincipal $user).IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)  
}

function ServiceCheckAndStart{
    param($ServiceName)
    $arrService = Get-Service -Name $ServiceName
    if ($arrService.Status -ne "Running"){
        Start-Service $ServiceName
     }
}

##===========
## Main
##===========
$TeleoptiServiceBus = "Teleopti Service Bus"

##test if admin
$isAdmin = Test-Administrator;
If ($isAdmin -ne $True) {
    throw "User is not Admin!"
}

## Name of the job, name of source in Windows Event Log
$JOB = "Teleopti.Ccc.BlobStorageCopy"

## Local debug values
<#
$BlobPath = "http://teleopticcc7.blob.core.windows.net/"
$ContainerName="teleopticcc/Payroll"
$AccountKey = "IqugZC5poDWLu9wwWocT42TAy5pael77JtbcZtnPcm37QRThCkdrnzOh3HEu8rDD1S8E6dU5D0aqS4sJA1BTxQ=="
$DataSourceName = "teleopticcc-dev"
#>

## Get environment varaibles
$BlobPath = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.BlobPath")
$ContainerName = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.ContainerName")
$AccountKey = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.AccountKey")
$DataSourceName = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.DataSourceName")

$BlobSource = $BlobPath + $ContainerName + "/" + $DataSourceName
$BlobSource


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

## Options to be added to RoboCopy
$ROBOOPTIONS = @("/MIR")

## Wrap all above arguments
$cmdArgs = @("$BlobSource","$DESTINATION",$OPTIONS)

$AzCopyExe = $directory + "\ccc7_azure\AzCopy\AzCopy.exe"
$AzCopyExe

Try
{
        
	## Create EventLog Source if not already exists
	if ([System.Diagnostics.EventLog]::SourceExists("$JOB") -eq $false) {
	"Creating EventLog Source `"$JOB`""
	[System.Diagnostics.EventLog]::CreateEventSource("$JOB", "Application")
	}
    
	## Start the azcopy with above parameters and log errors in Windows Eventlog.
	& $AzCopyExe @cmdArgs
    $AzExitCode = $LastExitCode
    
    if ($LastExitCode -ne 0) {
        throw "AsCopy generated an error!"
    }
    
	## Wrap arguments for robocopy
	$roboArgs = @("$DESTINATION","$FILEWATCH",$ROBOOPTIONS)

	## Run robocopy from Inbox to FileWatch
	& robocopy @roboArgs
    $RoboExitCode = $LastExitCode
    
    if ($RoboExitCode -ge 8) {
        throw "RoboCopy generated an error!"
    }

	##one or more files are new, log info to Eventlog and restart serviceBus
	If ($RoboExitCode -ge 1) {
        Write-EventLog -LogName Application -Source $JOB -EventID 0 -EntryType Information -Message "$SOURCE and $DESTINATION in sync."
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
    Break
}
Finally
{
    Write-Host "done"
}