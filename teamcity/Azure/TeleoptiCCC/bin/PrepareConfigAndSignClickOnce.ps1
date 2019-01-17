##===========
## Functions
##===========
function Update-ToggleByDelta {
    param(
    [string]$DeltaToggleFile,
    [string]$ToggleFile
    )

    $DeltaToggles = Get-Content -Path "$DeltaToggleFile" |
    Where-Object { !$_.StartsWith("#") } |
    ConvertFrom-Csv -Header ToggleName, Value -Delimiter "="

    $Toggles = Get-Content -Path "$ToggleFile" |
    Where-Object { !$_.StartsWith("#") } |
    ConvertFrom-Csv -Header ToggleName, Value -Delimiter "="


    foreach ($toggleDelta in $DeltaToggles){
        foreach ($toggleRunTime in $Toggles){
            if ($toggleDelta.ToggleName.Trim() -eq $toggleRunTime.ToggleName.Trim()) {
                if ($toggleDelta.Value.Trim() -ne $toggleRunTime.Value.Trim()) {
                    $newValue = $toggleDelta.ToggleName.trim() + " = " + $toggleDelta.Value.trim()
                    $regex = "^" + $toggleRunTime.ToggleName.trim() + ".+"
                    (Get-Content ($ToggleFile)) | Foreach-Object {$_ -replace $regex, $newValue} | Set-Content  ($ToggleFile)
                }
            }
        }
    }
}

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
	log-info "Creating event log..."
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
	log-info "Copying file from blob storage... $destinationFolder , $filename"
	
    $BlobPath = TeleoptiDriveMapProperty-get -name "BlobPath"
    $AccountKey = TeleoptiDriveMapProperty-get -name "AccountKey"
    $ContainerName = TeleoptiDriveMapProperty-get -name "ContainerName"
    $DataSourceName = TeleoptiDriveMapProperty-get -name "DataSourceName"

	## Options to be added to AzCopy
	$OPTIONS = @("/XO","/Y","/sourceKey:$AccountKey")

    #$BlobSource = $BlobPath + $ContainerName + "/" + $DataSourceName
	
	#Support AzCopy 6.3 version
	$BlobSourceArgs = "/Source:" + $BlobPath + $ContainerName + "/" + $DataSourceName
	$destinationFolderArgs = "/Dest:" + $destinationFolder
	$filenameArgs = "/Pattern:" + $filename

	## Wrap all above arguments
	$cmdArgs = @("$BlobSourceArgs","$destinationFolderArgs","$filenameArgs", $OPTIONS)

	$AzCopyExe = $directory + "\ccc7_azure\AzCopy\AzCopy.exe"
	$AzCopyExe

	## Start the azcopy with above parameters and log errors in Windows Eventlog.
	& $AzCopyExe @cmdArgs
    $AzExitCode = $LastExitCode
    
    if ($LastExitCode -ne 0) {
		log-error "AzCopy generated an error!"
		log-info "AzCopy generated an error!"
        throw "AzCopy generated an error!"
    }
}

function CopyFileToBlobStorage {
    Param(
      [string]$sourceFolder,
      [string]$pattern
      )
	log-info "Copying file to blob storage... $sourceFolder , $pattern"
	
    $BlobPath = TeleoptiDriveMapProperty-get -name "BlobPath"
    $AccountKey = TeleoptiDriveMapProperty-get -name "AccountKey"
    $ContainerName = TeleoptiDriveMapProperty-get -name "ContainerName"
    $DataSourceName = TeleoptiDriveMapProperty-get -name "DataSourceName"

	## Options to be added to AzCopy
	$OPTIONS = @("/XO","/Y","/destKey:$AccountKey")

    #$BlobDestination = $BlobPath + $ContainerName + "/" + $DataSourceName
	
	#Support AzCopy 6.3 version
    $BlobDestinationArgs = "/Dest:" + $BlobPath + $ContainerName + "/" + $DataSourceName
    $sourceFolderArgs = "/Source:" + $sourceFolder
	$patternArgs = "/Pattern:" + $pattern

	## Wrap all above arguments
	$cmdArgs = @("$sourceFolderArgs","$BlobDestinationArgs","$patternArgs", $OPTIONS)

	$AzCopyExe = $directory + "\ccc7_azure\AzCopy\AzCopy.exe"
	$AzCopyExe

	## Start the azcopy with above parameters and log errors in Windows Eventlog.
	& $AzCopyExe @cmdArgs
    $AzExitCode = $LastExitCode
    
    if ($LastExitCode -ne 0) {
		log-error "AzCopy generated an error!"
		log-info "AzCopy generated an error!" 
        throw "AzCopy generated an error!"
    }
}
function CopyReportsFromBlobStorage{
	log-info "Copying reports from blob storage..."

	$BlobPath = TeleoptiDriveMapProperty-get -name "BlobPath"
    $AccountKey = TeleoptiDriveMapProperty-get -name "AccountKey"
    $ContainerName = TeleoptiDriveMapProperty-get -name "ContainerName"
    $DataSourceName = TeleoptiDriveMapProperty-get -name "DataSourceName"
	$fullPathCustomReports =  "$directory\..\..\sitesroot\3\Areas\Reporting\Reports\Custom\" 
	
	If (!(Test-Path $fullPathCustomReports)) {
		New-Item -Path $fullPathCustomReports -ItemType Directory
 	}
	
	## Options to be added to AzCopy
	$OPTIONS = @("/S","/XO","/Y","/sourceKey:$AccountKey")

    #$BlobSource = $BlobPath + $ContainerName + "/" + $DataSourceName
	
	#Support AzCopy 6.3 version
	$BlobSourceArgs = "/Source:" + $BlobPath + $ContainerName + "/" + $DataSourceName
	$fullPathCustomReportsnewArgs = "/Dest:" + $fullPathCustomReports
	
	## Wrap all above arguments
	$cmdArgs = @("$BlobSourceArgs","$fullPathCustomReportsnewArgs",$OPTIONS)

	$AzCopyExe = $directory + "\ccc7_azure\AzCopy\AzCopy.exe"
	$AzCopyExe

	## Start the azcopy with above parameters and log errors in Windows Eventlog.
	& $AzCopyExe @cmdArgs
    $AzExitCode = $LastExitCode
    
    if ($LastExitCode -ne 0) {
		log-error "AzCopy generated an error!"
		log-info "AzCopy generated an error!"
        throw "AzCopy generated an error!"
    }
	Remove-Item $fullPathCustomReports\* -exclude *.rdlc
}

function AddIfNotExists{
	Param(
		[string]$fullPathsettingsFile,
		[string]$variableName,
		[string]$content
		)
	$notFound = @( Get-Content "$fullPathsettingsFile" | Where-Object { $_.Contains("$variableName") } ).Count -eq 0
	if ($notFound) {
		Add-Content "$fullPathsettingsFile" "$content"
	}
}


function SetDefaultSettings{
	Param(
		[string]$fullPathsettingsFile,
		[string]$DataSourceName
		)
    
    log-info "Before fetching hostname"
    $theHost = [System.Net.Dns]::GetHostName()
    log-info "After fetching hostname: $theHost"

	Add-Content "$fullPathsettingsFile" ""
	
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(DEFAULT_IDENTITY_PROVIDER)" -content "`$(DEFAULT_IDENTITY_PROVIDER)|Teleopti"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(WindowsClaimProvider)" -content "`$(WindowsClaimProvider)|"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(TeleoptiClaimProvider)" -content "`$(TeleoptiClaimProvider)|<add identifier=`"urn:Teleopti`" displayName=`"Teleopti`" url=`"https://$DataSourceName.teleopticloud.com/Web/sso/`" protocolHandler=`"RelativeOpenIdHandler`" />"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(SIGNALR_BACKPLANE_TYPE)" -content "`$(SIGNALR_BACKPLANE_TYPE)|azureservicebus"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(SIGNALR_BACKPLANE_PREFIX)" -content "`$(SIGNALR_BACKPLANE_PREFIX)|$DataSourceName"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(SDK_CRED_PROT)" -content "`$(SDK_CRED_PROT)|None"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(MATRIX_WEB_SITE_URL)" -content "`$(MATRIX_WEB_SITE_URL)|https://$DataSourceName.teleopticloud.com/Analytics"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(SDK_SSL_SECURITY_MODE)" -content "`$(SDK_SSL_SECURITY_MODE)|Transport"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(AGENT_SERVICE)" -content "`$(AGENT_SERVICE)|https://$DataSourceName.teleopticloud.com/SDK/TeleoptiCCCSdkService.svc"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(PM_INSTALL)" -content "`$(PM_INSTALL)|False"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(PM_AUTH_MODE)" -content "`$(PM_AUTH_MODE)|Windows"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(PM_ANONYMOUS_DOMAINUSER)" -content "`$(PM_ANONYMOUS_DOMAINUSER)|"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(PM_ANONYMOUS_PWD)" -content "`$(PM_ANONYMOUS_PWD)|"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(IIS_AUTH)" -content "`$(IIS_AUTH)|Forms"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(HTTPGETENABLED)" -content "`$(HTTPGETENABLED)|false"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(HTTPSGETENABLED)" -content "`$(HTTPSGETENABLED)|true"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(SDK_SSL_MEX_BINDING)" -content "`$(SDK_SSL_MEX_BINDING)|mexHttpsBinding"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(RTA_SERVICE)" -content "`$(RTA_SERVICE)|https://$DataSourceName.teleopticloud.com/RTA/TeleoptiRtaService.svc"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(CONFIGURATION_FILES_PATH)" -content "`$(CONFIGURATION_FILES_PATH)|$DatasourcesPath"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(RTA_STATE_CODE)" -content "`$(RTA_STATE_CODE)|ACW,ADMIN,EMAIL,IDLE,InCall,LOGGED ON,OFF,Ready,WEB"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(RTA_QUEUE_ID)" -content "`$(RTA_QUEUE_ID)|2001,2002,0063,2000,0019,0068,0085,0202,0238,2003"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(WEB_BROKER_FOR_WEB)" -content "`$(WEB_BROKER_FOR_WEB)|https://$DataSourceName.teleopticloud.com/Web"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(WEB_BROKER)" -content "`$(WEB_BROKER)|https://$DataSourceName.teleopticloud.com/Web/"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(PM_ASMX)" -content "`$(PM_ASMX)|NotImplemented"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(PM_SERVICE)" -content "`$(PM_SERVICE|NotImplemented"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(AS_DATABASE)" -content "`$(AS_DATABASE)|NotImplemented"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(AS_SERVER_NAME)" -content "`$(AS_SERVER_NAME)|NotImplemented"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(LOCAL_WIKI)" -content "`$(LOCAL_WIKI)|https://wiki.teleopti.com/TeleoptiCCC/Special:MyLanguage/"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(ETLPM_BINDING_NAME)" -content "`$(ETLPM_BINDING_NAME)|Etl_Pm_Https_Binding"	
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(URL)" -content "`$(URL)|https://$DataSourceName.teleopticloud.com/Web/"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(STARDUST)" -content "`$(STARDUST)|http://$theHost"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(UrlAuthenticationBridge)" -content "`$(UrlAuthenticationBridge)|https://$DataSourceName.teleopticloud.com/AuthenticationBridge/"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(WEB_DEPLOY)" -content "`$(WEB_DEPLOY)|true"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(DNS_ALIAS)" -content "`$(DNS_ALIAS)|https://$DataSourceName.teleopticloud.com/"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(HSTS_HEADER)" -content "`$(HSTS_HEADER)|<add name=`"Strict-Transport-Security`" value=`"max-age=31536000`"/>"
	AddIfNotExists -fullPathsettingsFile "$fullPathsettingsFile" -variableName "`$(CSP_PARENT_URL)" -content "`$(CSP_PARENT_URL)|"
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
	
	##test if admin
	If (!(Test-Administrator($myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()))) {
		log-error "User is not Admin!"
		throw "User is not Admin!"
	}

    #create event log source
    EventlogSource-Create "$JOB"

    $SupportToolFolder = $directory + "\..\Tools\SupportTools"
    $settingsFile = "settings.txt"
    $fullPathsettingsFile =  $SupportToolFolder + "\" + $settingsFile
	
	$togglesFile = "toggles.txt"
    $deltaToggleFile = "DeltaToggle.txt"

	$fullPathTogglesFileForWeb =  "$directory\..\..\sitesroot\3\bin\FeatureFlags\" + $togglesFile
	$fullPathTogglesFileForRta =  "$directory\..\..\sitesroot\5\bin\FeatureFlags\" + $togglesFile
	
    $customStartupScript = "CustomStartup.ps1"

    $DataSourceName = TeleoptiDriveMapProperty-get -name "DataSourceName"

	
	if (Test-Path "$fullPathsettingsFile") {
		Remove-Item "$fullPathsettingsFile"
	}
	
	$DatasourcesPath="$directory\..\Services\ETL\Service"
	
    #Get customer specific config from BlobStorage
    CopyFileFromBlobStorage -destinationFolder "$SupportToolFolder" -filename "$settingsFile"
	#copy the settings for sms (and email) notifications, if any
    CopyFileFromBlobStorage -destinationFolder "$DatasourcesPath" -filename "NotificationConfig.xml"
	#copy the password policy, if any
    CopyFileFromBlobStorage -destinationFolder "$DatasourcesPath" -filename "PasswordPolicy.xml"
    
	CopyReportsFromBlobStorage
	
	CopyFileFromBlobStorage -destinationFolder "$directory" -filename "$togglesFile"
    CopyFileFromBlobStorage -destinationFolder "$directory" -filename "$customStartupScript"
    CopyFileFromBlobStorage -destinationFolder "$directory" -filename "$deltaToggleFile"

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

	SetDefaultSettings -fullPathsettingsFile "$fullPathsettingsFile" -DataSourceName "$DataSourceName"
	
	#Try to get machine key from BlogStorage, if cannot, support tool will generate one, and save back to BlogStorage at a later step, hope no other instance comes to this step before we save the first one.
	CopyFileFromBlobStorage -destinationFolder "$SupportToolFolder" -filename "decryption.key"
    CopyFileFromBlobStorage -destinationFolder "$SupportToolFolder" -filename "validation.key"

    $SupportTool = $SupportToolFolder + "\Teleopti.Support.Tool.exe"
    Set-Location $SupportToolFolder

    $cmdArgs = @("-MOAzure")
	log-info "Running Teleopti.Support.Tool.exe..."
	$p = Start-Process $SupportTool -ArgumentList "-MOAzure" -wait -NoNewWindow -PassThru
    $p.HasExited
    $LastExitCode = $p.ExitCode

    
    if ($LastExitCode -ne 0) {
		log-error "SupportTool generated an error!"
        throw "SupportTool generated an error!"
    }

	#Save machine key files for later usage on other instances
	CopyFileToBlobStorage -sourceFolder "$SupportToolFolder" -pattern "*.key"
    
       
    #Sign ClickOnce, create bat files for later execution in Scheduled task
    $ClickOnceSignPath="$directory\..\Tools\ClickOnceSign"
    CD e:
    Set-Location $ClickOnceSignPath
    $ClickOnceTool = $ClickOnceSignPath + "\ClickOnceSign.exe"

    $ClientPath="$directory\..\..\sitesroot\4"
    $pwd ="T3l30pt1"
	log-info "Running ClickOnceSign.exe..."
    $cmdArgs = @("-s","-a Teleopti.Ccc.SmartClientPortal.Shell.application", "-m Teleopti.Ccc.SmartClientPortal.Shell.exe.manifest","-u https://$DataSourceName.teleopticloud.com/Client/","-c $scriptPath\Teleopti.pfx","-dir $ClientPath","-p $pwd")
	
	if (Test-Path "$ClickOnceSignPath\SignAdminClient.bat") {
		Remove-Item "$ClickOnceSignPath\SignAdminClient.bat"
	}
    
	Add-Content "$ClickOnceSignPath\SignAdminClient.bat" "E:"
	Add-Content "$ClickOnceSignPath\SignAdminClient.bat" "CD $ClickOnceSignPath"
    Add-Content "$ClickOnceSignPath\SignAdminClient.bat" "$ClickOnceTool $cmdArgs"
    &"$ClickOnceSignPath\SignAdminClient.bat"
 
}
Catch [Exception]
{
    $ErrorMessage = $_.Exception.Message
	log-error "$ErrorMessage"
	log-info "$ErrorMessage"
    Write-EventLog -LogName Application -Source $JOB -EventID 1 -EntryType Error -Message "$ErrorMessage"
	Throw "Script failed, Check Windows event log for details"
}
Finally
{
    log-info "End of Script."
}