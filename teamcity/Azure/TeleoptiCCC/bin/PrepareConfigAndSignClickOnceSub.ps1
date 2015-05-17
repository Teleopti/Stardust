##===========
## Functions
##===========
function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value;
    if($Invocation.PSScriptRoot)
    {
        $Invocation.PSScriptRoot;
    }
    Elseif($Invocation.MyCommand.Path)
    {
        Split-Path $Invocation.MyCommand.Path
    }
    else
    {
        $Invocation.InvocationName.Substring(0,$Invocation.InvocationName.LastIndexOf("\"));
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
		switch ($name){
		BlobPath		{$TeleoptiDriveMapProperty = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.BlobPath"); break}
        ContainerName	{$TeleoptiDriveMapProperty="teleopticcc/Settings"; break}        
		AccountKey		{$TeleoptiDriveMapProperty = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.AccountKey"); break}
		DataSourceName	{$TeleoptiDriveMapProperty = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.DataSourceName"); break}
		default			{$TeleoptiDriveMapProperty="Unknown Value"; break}
        }
           
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

function CopyFileToBlobStorage {
    Param(
      [string]$sourceFolder,
      [string]$pattern
      )
    $BlobPath = TeleoptiDriveMapProperty-get -name "BlobPath"
    $AccountKey = TeleoptiDriveMapProperty-get -name "AccountKey"
    $ContainerName = TeleoptiDriveMapProperty-get -name "ContainerName"
    $DataSourceName = TeleoptiDriveMapProperty-get -name "DataSourceName"

	## Options to be added to AzCopy
	$OPTIONS = @("/XO","/Y","/destKey:$AccountKey")

    $BlobDestination = $BlobPath + $ContainerName + "/" + $DataSourceName

	## Wrap all above arguments
	$cmdArgs = @("$sourceFolder","$BlobDestination","$pattern", $OPTIONS)

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
$directory = Get-ScriptDirectory
write-host $directory
$computer = gc env:computername

## Name of the job, name of source in Windows Event Log
$JOB = "Teleopti.Ccc.BlobStorageCopy"

Try
{
    [Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime")

	##test if admin
	If (!(Test-Administrator($myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()))) {
		throw "User is not Admin!"
	}

    #create event log source
    EventlogSource-Create "$JOB"

    $SupportToolFolder = $directory + "\..\Tools\SupportTools"
    $settingsFile = "settings.txt"
    $fullPathsettingsFile =  $SupportToolFolder + "\" + $settingsFile
	
	$togglesFile = "toggles.txt"
	$fullPathTogglesFileForWeb =  "$directory\..\..\sitesroot\3\bin\FeatureFlags\" + $togglesFile
	$fullPathTogglesFileForRta =  "$directory\..\..\sitesroot\5\bin\FeatureFlags\" + $togglesFile

    $DataSourceName = TeleoptiDriveMapProperty-get -name "DataSourceName"
    
	#Remove *.nhib.xml files that still resides in public facing directories
	$WebRootFolder="$directory\..\..\sitesroot\*"
	get-childitem $WebRootFolder -include *.nhib.xml -recurse | foreach ($_) {remove-item $_.fullname}
	
	Remove-Item "$fullPathsettingsFile"
	if (Test-Path "$fullPathsettingsFile") {
		Remove-Item "$fullPathsettingsFile"
	}

    #Get customer specific config from BlobStorage
    CopyFileFromBlobStorage -destinationFolder "$SupportToolFolder" -filename "$settingsFile"
    CopyFileFromBlobStorage -destinationFolder "$SupportToolFolder" -filename "decryption.key"
    CopyFileFromBlobStorage -destinationFolder "$SupportToolFolder" -filename "validation.key"
	
	CopyFileFromBlobStorage -destinationFolder "$directory" -filename "$togglesFile"
	$tempTogglesFile = "$directory\" + $togglesFile
	if (Test-Path "$tempTogglesFile") {
		Remove-Item "$fullPathTogglesFileForWeb"
		if (Test-Path "$fullPathTogglesFileForWeb") {
			Remove-Item "$fullPathTogglesFileForWeb"
		}
		Remove-Item "$fullPathTogglesFileForRta"
		if (Test-Path "$fullPathTogglesFileForRta") {
			Remove-Item "$fullPathTogglesFileForRta"
		}
		CopyFileFromBlobStorage -destinationFolder "$directory\..\..\sitesroot\3\bin\FeatureFlags" -filename "$togglesFile"
		CopyFileFromBlobStorage -destinationFolder "$directory\..\..\sitesroot\5\bin\FeatureFlags" -filename "$togglesFile"
		Remove-Item "$tempTogglesFile"
	}

	$DatasourcesPath="$directory\..\Services\ETL\Service"

    #Set static config for all other params
    Add-Content "$fullPathsettingsFile" ""
    Add-Content "$fullPathsettingsFile" "`$(SDK_CRED_PROT)|None"
    Add-Content "$fullPathsettingsFile" "`$(MATRIX_WEB_SITE_URL)|https://$DataSourceName.teleopticloud.com/Analytics"
    Add-Content "$fullPathsettingsFile" "`$(SDK_SSL_SECURITY_MODE)|Transport"
    Add-Content "$fullPathsettingsFile" "`$(AGENT_SERVICE)|https://$DataSourceName.teleopticloud.com/SDK/TeleoptiCCCSdkService.svc"
    Add-Content "$fullPathsettingsFile" "`$(PM_INSTALL)|False"
    Add-Content "$fullPathsettingsFile" "`$(PM_AUTH_MODE)|Windows"
    Add-Content "$fullPathsettingsFile" "`$(PM_ANONYMOUS_DOMAINUSER)|"
    Add-Content "$fullPathsettingsFile" "`$(PM_ANONYMOUS_PWD)|"
    Add-Content "$fullPathsettingsFile" "`$(IIS_AUTH)|Forms"
    Add-Content "$fullPathsettingsFile" "`$(HTTPGETENABLED)|false"
    Add-Content "$fullPathsettingsFile" "`$(HTTPSGETENABLED)|true"
    Add-Content "$fullPathsettingsFile" "`$(SDK_SSL_MEX_BINDING)|mexHttpsBinding"
    Add-Content "$fullPathsettingsFile" "`$(RTA_SERVICE)|https://$DataSourceName.teleopticloud.com/RTA/TeleoptiRtaService.svc"
    Add-Content "$fullPathsettingsFile" "`$(ETL_SERVICE_nhibConfPath)|$DatasourcesPath"
    Add-Content "$fullPathsettingsFile" "`$(ETL_TOOL_nhibConfPath)|$DatasourcesPath"
    Add-Content "$fullPathsettingsFile" "`$(SDK_nhibConfPath)|$DatasourcesPath"
    Add-Content "$fullPathsettingsFile" "`$(AGENTPORTALWEB_nhibConfPath)|$DatasourcesPath"
    Add-Content "$fullPathsettingsFile" "`$(RTA_STATE_CODE)|ACW,ADMIN,EMAIL,IDLE,InCall,LOGGED ON,OFF,Ready,WEB"
    Add-Content "$fullPathsettingsFile" "`$(RTA_QUEUE_ID)|2001,2002,0063,2000,0019,0068,0085,0202,0238,2003"
    Add-Content "$fullPathsettingsFile" "`$(WEB_BROKER_FOR_WEB)|https://$DataSourceName.teleopticloud.com/Web"
    Add-Content "$fullPathsettingsFile" "`$(WEB_BROKER_BACKPLANE)|https://$DataSourceName.teleopticloud.com/Broker.backplane/backplane"
	Add-Content "$fullPathsettingsFile" "`$(WEB_BROKER)|https://$DataSourceName.teleopticloud.com/Broker/"
    Add-Content "$fullPathsettingsFile" "`$(PM_ASMX)|NotImplemented"
    Add-Content "$fullPathsettingsFile" "`$(PM_SERVICE|NotImplemented"
    Add-Content "$fullPathsettingsFile" "`$(AS_DATABASE)|NotImplemented"
    Add-Content "$fullPathsettingsFile" "`$(AS_SERVER_NAME)|NotImplemented"
    Add-Content "$fullPathsettingsFile" "`$(LOCAL_WIKI)|http://wiki.teleopti.com/TeleoptiCCC/Special:MyLanguage/"
    Add-Content "$fullPathsettingsFile" "`$(ETLPM_BINDING_NAME)|Etl_Pm_Https_Binding"	
	Add-Content "$fullPathsettingsFile" "`$(URL)|https://$DataSourceName.teleopticloud.com/Web/"
    Add-Content "$fullPathsettingsFile" "`$(DEFAULT_IDENTITY_PROVIDER)|Teleopti"
    Add-Content "$fullPathsettingsFile" "`$(WindowsClaimProvider)|"
    Add-Content "$fullPathsettingsFile" "`$(TeleoptiClaimProvider)|<add identifier=`"urn:Teleopti`" displayName=`"Teleopti`" url=`"https://$DataSourceName.teleopticloud.com/Web/sso/`" protocolHandler=`"RelativeOpenIdHandler`" />"
    Add-Content "$fullPathsettingsFile" "`$(UrlAuthenticationBridge)|https://$DataSourceName.teleopticloud.com/AuthenticationBridge/"
    Add-Content "$fullPathsettingsFile" "`$(WEB_DEPLOY)|true"
	Add-Content "$fullPathsettingsFile" "`$(DNS_ALIAS)|https://$DataSourceName.teleopticloud.com/"

    $SupportTool = $SupportToolFolder + "\Teleopti.Support.Tool.exe"
    Set-Location $SupportToolFolder

    $cmdArgs = @("-MOAzure")
	$p = Start-Process $SupportTool -ArgumentList "-MOAzure" -wait -NoNewWindow -PassThru
    $p.HasExited
    $LastExitCode = $p.ExitCode

    
    if ($LastExitCode -ne 0) {
        throw "SupportTool generated an error!"
    }

	#Save machine key files for later usage on other instances
	CopyFileToBlobStorage -sourceFolder "$SupportToolFolder" -pattern "*.key"
    
    #more - show content of ETL + Service bus config
    $AzureConfigFiles = $SupportToolFolder + "\ConfigFiles\AzureConfigFiles.txt"
    get-content $AzureConfigFiles | ForEach-Object {
        $temp = ([string] $SupportToolFolder + "\" + $_).split(",")
        if (!$temp[0].EndsWith(".txt")) {
            write-host "--------------------"
            write-host $temp[0]
            write-host "--------------------"
            get-content $temp[0]
            }
     }
    
    #Sign ClickOnce, create bat files for later execution in Scheduled task
    $ClickOnceSignPath="$directory\..\Tools\ClickOnceSign"
    CD e:
    Set-Location $ClickOnceSignPath
    $ClickOnceTool = $ClickOnceSignPath + "\ClickOnceSign.exe"

    $ClientPath="$directory\..\..\sitesroot\4"
    $pwd ="`"`""
	
    $cmdArgs = @("-s","-a Teleopti.Ccc.SmartClientPortal.Shell.application", "-m Teleopti.Ccc.SmartClientPortal.Shell.exe.manifest","-u https://$DataSourceName.teleopticloud.com/Client/","-c $ClickOnceSignPath\TemporaryKey.pfx","-dir $ClientPath","-p $pwd")
    Remove-Item "$ClickOnceSignPath\SignAdminClient.bat"
	Add-Content "$ClickOnceSignPath\SignAdminClient.bat" "E:"
	Add-Content "$ClickOnceSignPath\SignAdminClient.bat" "CD $ClickOnceSignPath"
    Add-Content "$ClickOnceSignPath\SignAdminClient.bat" "$ClickOnceTool $cmdArgs"
    &"$ClickOnceSignPath\SignAdminClient.bat"
 
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