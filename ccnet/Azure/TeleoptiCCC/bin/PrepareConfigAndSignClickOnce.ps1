##===========
## Functions
##===========
function FindAndReplace {
    Param(
      [string]$findString,
      [string]$replaceString,
      [string]$fullPath
      )
(Get-Content "$fullPath") | 
Foreach-Object {$_ -replace "$findString", "$replaceString"} | 
Set-Content "$fullPath"
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
    $computer = gc env:computername
	## Local debug values
    if ($computer.ToUpper().StartsWith("TELEOPTI")) {
	$DataSourceName = "teleopticcc-dev"
    }
    else {
	$DataSourceName = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.DataSourceName")
    }
	return $DataSourceName
}

function BlobPath-get {
    $computer = gc env:computername
    ## Local debug values
    if ($computer.ToUpper().StartsWith("TELEOPTI")) {
	$BlobPath = "teleopticcc-dev"
    }
    else {
	$BlobPath = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.BlobPath")
    }
	return $BlobPath
}

function BlobPath-get {
    $computer = gc env:computername
    ## Local debug values
    if ($computer.ToUpper().StartsWith("TELEOPTI")) {
	$BlobPath = "teleopticcc-dev"
    }
    else {
	$BlobPath = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.BlobPath")
    }
	return $BlobPath
}

function TeleoptiDriveMapProperty-get {
    Param(
      [string]$name
      )
    $computer = gc env:computername
    ## Local debug values
    if ($computer.ToUpper().StartsWith("TELEOPTI")) {
		switch ($name){
		BlobPath		{$TeleoptiDriveMapProperty="http://teleopticcc7.blob.core.windows.net/"; break}
		ContainerName	{$TeleoptiDriveMapProperty="teleopticcc/Settings"; break}
		AccountKey		{$TeleoptiDriveMapProperty="IqugZC5poDWLu9wwWocT42TAy5pael77JtbcZtnPcm37QRThCkdrnzOh3HEu8rDD1S8E6dU5D0aqS4sJA1BTxQ=="; break}
		DataSourceName	{$TeleoptiDriveMapProperty="teleopticcc-dev"; break}
		default			{$TeleoptiDriveMapProperty="Unknown Value"; break}
        }
     }
    else {
	$TeleoptiDriveMapProperty = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap."+$name)
    }
	return $TeleoptiDriveMapProperty
}

function EventlogSource-Create {
    param([string]$EventSourceName)
    $type = "Application"
    #create event log source
    if ([System.Diagnostics.EventLog]::SourceExists("$EventSourceName") -eq $false) {
        [System.Diagnostics.EventLog]::CreateEventSource("$EventSourceName", $type)
        }
}

function CopyFileFromBlobStorage {
    Param(
      [string]$destinationFolder,
      [string]$filename
      )
    $BlobPath = TeleoptiDriveMapProperty-get -name "BlobPath"
    $AccountKey = TeleoptiDriveMapProperty-get -name "AccountKey"
    $ContainerName = TeleoptiDriveMapProperty-get -name "ContainerName"
    $DataSourceName = TeleoptiDriveMapProperty-get -name "DataSourceName"

	## Options to be added to AzCopy
	$OPTIONS = @("/XO","/Y","/sourceKey:$AccountKey")

    $BlobSource = $BlobPath + $ContainerName + "/" + $DataSourceName

	## Wrap all above arguments
	$cmdArgs = @("$BlobSource","$destinationFolder","$filename", $OPTIONS)

	$AzCopyExe = $directory + "\ccc7_azure\AzCopy\AzCopy.exe"
	$AzCopyExe

	## Start the azcopy with above parameters and log errors in Windows Eventlog.
	& $AzCopyExe @cmdArgs
    $AzExitCode = $LastExitCode
    
    if ($LastExitCode -ne 0) {
        throw "AsCopy generated an error!"
    }
}

##===========
## Main
##===========
function main {
Param(
  [string]$directory
  )
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

    $destFolder = $directory + "\SupportTools"
    $settingsFile = "settings.txt"
    $fullPath =  "$destFolder" + "\" + "$settingsFile"

    $DataSourceName = TeleoptiDriveMapProperty-get -name "DataSourceName"

    Remove-Item "$fullPath"
    #Get customer specific config from BlobStorage
    CopyFileFromBlobStorage -destinationFolder "$destFolder" -filename "$settingsFile"

    #Set static config for all other params
    Add-Content "$fullPath" "`$(SDK_CRED_PROT)|None"
    Add-Content "$fullPath" "`$(MATRIX_WEB_SITE_URL)|https://$DataSourceName.teleopticloud.com/Analytics"
    Add-Content "$fullPath" "`$(SDK_SSL_SECURITY_MODE)|Transport"
    Add-Content "$fullPath" "`$(AGENT_SERVICE)|https://$DataSourceName.teleopticloud.com/SDK/TeleoptiCCCSdkService.svc"
    Add-Content "$fullPath" "`$(PM_INSTALL)|False"
    Add-Content "$fullPath" "`$(PM_AUTH_MODE)|Windows"
    Add-Content "$fullPath" "`$(PM_ANONYMOUS_DOMAINUSER)|"
    Add-Content "$fullPath" "`$(PM_ANONYMOUS_PWD)|"
    Add-Content "$fullPath" "`$(IIS_AUTH)|Forms"
    Add-Content "$fullPath" "`$(HTTPGETENABLED)|false"
    Add-Content "$fullPath" "`$(HTTPSGETENABLED)|true"
    Add-Content "$fullPath" "`$(SDK_SSL_MEX_BINDING)|mexHttpsBinding"
    Add-Content "$fullPath" "`$(WEB_BROKER)|https://$DataSourceName.teleopticloud.com/Broker/"
    Add-Content "$fullPath" "`$(RTA_SERVICE)|https://$DataSourceName.teleopticloud.com/RTA/TeleoptiRtaService.svc"
    Add-Content "$fullPath" "`$(ETL_SERVICE_nhibConfPath)|"
    Add-Content "$fullPath" "`$(ETL_TOOL_nhibConfPath)|"
    Add-Content "$fullPath" "`$(SDK_nhibConfPath)| "
    Add-Content "$fullPath" "`$(AGENTPORTALWEB_nhibConfPath)| "
    Add-Content "$fullPath" "`$(RTA_STATE_CODE)|ACW,ADMIN,EMAIL,IDLE,InCall,LOGGED ON,OFF,Ready,WEB"
    Add-Content "$fullPath" "`$(RTA_QUEUE_ID)|2001,2002,0063,2000,0019,0068,0085,0202,0238,2003"
    Add-Content "$fullPath" "`$(WEB_BROKER_FOR_WEB)|https://$DataSourceName.teleopticloud.com/Web"
    Add-Content "$fullPath" "`$(WEB_BROKER_BACKPLANE)|"
    Add-Content "$fullPath" "`$(PM_ASMX)|NotImplemented"
    Add-Content "$fullPath" "`$(PM_SERVICE|NotImplemented"
    Add-Content "$fullPath" "`$(AS_DATABASE)|NotImplemented"
    Add-Content "$fullPath" "`$(AS_SERVER_NAME)|NotImplemented"
    

    $SupportToolFolder = $directory + "\SupportTools"
    $SupportTool = $SupportToolFolder + "\Teleopti.Support.Tool.exe"
    Set-Location $SupportToolFolder

    $cmdArgs = @("-MOAzure")
	& $SupportTool @cmdArgs
    $SupportTool = $LastExitCode
    
    if ($LastExitCode -ne 0) {
        throw "SupportTool generated an error!"
    }

    #replace Data Factory Name
    FindAndReplace -findString "Teleopti CCC" -replaceString "$DataSourceName" -fullPath "$directory\..\..\sitesroot\1\TeleoptiCCC7.nhib.xml"
    FindAndReplace -findString "Teleopti CCC" -replaceString "$DataSourceName" -fullPath "$directory\..\..\sitesroot\3\TeleoptiCCC7.nhib.xml"
    FindAndReplace -findString "Teleopti CCC" -replaceString "$DataSourceName" -fullPath "$directory\..\Services\ETL\Service\TeleoptiCCC7.nhib.xml"
    FindAndReplace -findString "Teleopti CCC" -replaceString "$DataSourceName" -fullPath "$directory\..\Services\ETL\Tool\TeleoptiCCC7.nhib.xml"

    #Sign ClickOnce
    $ClickOnceSignPath="$directory\..\Tools\ClickOnceSign"
    Set-Location $ClickOnceSignPath
    $ClickOnceTool = $ClickOnceSignPath + "\ClickOnceSign.exe"

    $ClientPath="$directory\..\..\sitesroot\5"
    $MyTimePath="$directory\..\..\sitesroot\4"

    $cmdArgs = @("-s","-a Teleopti.Ccc.SmartClientPortal.Shell.application","-m Teleopti.Ccc.SmartClientPortal.Shell.exe.manifest","-u https://$DataSourceName.teleopticloud.com/Client/","-c $ClickOnceSignPath\TemporaryKey.pfx","-p ","-dir $ClientPath")
	& $ClickOnceTool @cmdArgs

    $cmdArgs = @("-s","-a Teleopti.Ccc.AgentPortal.application","-m Teleopti.Ccc.AgentPortal.exe.manifest","-u https://$DataSourceName.teleopticloud.com/MyTime/","-c $ClickOnceSignPath\TemporaryKey.pfx","-p ","-dir $MyTimePath")
	& $ClickOnceTool @cmdArgs
    
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