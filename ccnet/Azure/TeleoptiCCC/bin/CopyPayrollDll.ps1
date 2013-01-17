Param(
  [string]$directory
  )
[Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime")



$directory
## Name of the job, name of source in Windows Event Log
$JOB = "Teleopti.Ccc.BlobStorageCopy"

## Local debug values
<#
$BlobPath = "http://teleopticcc7.blob.core.windows.net/"
$ContainerName="teleopticcc/Payroll/"
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
$FILEWATCH = ".\..\Services\ServiceBus\Payroll.DeployNew"

## Path to AzCopy logfile
$LOGFILE = "c:\temp\PayrollInbox"

## Log events from the script to this location
$SCRIPTLOG = "c:\temp\PayrollInbox\$JOB-scriptlog.log"

## Options to be added to AzCopy
$OPTIONS = @("/S","/XO","/Y","/sourceKey:$AccountKey")

## Options to be added to RoboCopy
$ROBOOPTIONS = @("/MIR")

## This will create a timestamp like yyyy-mm-yy
$TIMESTAMP = get-date -uformat "%Y-%m%-%d"

## This will get the time like HH:MM:SS
$TIME = get-date -uformat "%T"

## Wrap all above arguments
$cmdArgs = @("$BlobSource","$DESTINATION",$OPTIONS)

$AzCopyExe = ".\ccc7_azure\AzCopy\AzCopy.exe"

## Start the azcopy with above parameters and log errors in Windows Eventlog.
& $AzCopyExe @cmdArgs

## Wrap arguments for robocopy
$roboArgs = @("$DESTINATION","$FILEWATCH",$ROBOOPTIONS)

## Run robocopy from Inbox to FileWatch
& robocopy @roboArgs

## Get LastExitCode and store in variable
$ExitCode = $LastExitCode

$MSGType=@{
"7"="Error"
"2"="Error"
"0"="Information"
}

## Message descriptions for each ExitCode.
$MSG=@{
"7"="Server failed to authenticate the request. Make sure the value of Authorization header is formed correctly"
"2"="The syntax of the command is incorrect"
"0"="$BlobSource and $DESTINATION in sync."
}

## Function to see if running with administrator privileges
function Test-Administrator  
{  
    $user = [Security.Principal.WindowsIdentity]::GetCurrent();
    (New-Object Security.Principal.WindowsPrincipal $user).IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)  
}

## If running with administrator privileges
If (Test-Administrator -eq $True) {
	"Has administrator privileges"
	
	## Create EventLog Source if not already exists
	if ([System.Diagnostics.EventLog]::SourceExists("$JOB") -eq $false) {
	"Creating EventLog Source `"$JOB`""
    [System.Diagnostics.EventLog]::CreateEventSource("$JOB", "Application")
	}
	
	## Write known ExitCodes to EventLog
	if ($MSG."$ExitCode" -gt $null) {
		Write-EventLog -LogName Application -Source $JOB -EventID $ExitCode -EntryType $MSGType."$ExitCode" -Message $MSG."$ExitCode"
	}
	## Write unknown ExitCodes to EventLog
	else {
		Write-EventLog -LogName Application -Source $JOB -EventID $ExitCode -EntryType Warning -Message "Unknown ExitCode. EventID equals ExitCode"
	}
}
## If not running with administrator privileges
else {
	## Write to screen and logfile
	Add-content $SCRIPTLOG "$TIMESTAMP $TIME No administrator privileges" -PassThru
	Add-content $SCRIPTLOG "$TIMESTAMP $TIME Cannot write to EventLog" -PassThru
	
	## Write known ExitCodes to screen and logfile
	if ($MSG."$ExitCode" -gt $null) {
		Add-content $SCRIPTLOG "$TIMESTAMP $TIME Printing message to logfile:" -PassThru
		Add-content $SCRIPTLOG ($TIMESTAMP + ' ' + $TIME + ' ' + $MSG."$ExitCode") -PassThru
		Add-content $SCRIPTLOG "$TIMESTAMP $TIME ExitCode`=$ExitCode" -PassThru
	}
	## Write unknown ExitCodes to screen and logfile
	else {
		Add-content $SCRIPTLOG "$TIMESTAMP $TIME ExitCode`=$ExitCode (UNKNOWN)" -PassThru
	}
	Add-content $SCRIPTLOG ""
	Return
}