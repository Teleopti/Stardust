##===========
## Functions
##===========
function TeleoptiDriveMapProperty-get {
    Param(
      [string]$name
      )
    $computer = gc env:computername

	switch ($name){
	BlobPath		{$TeleoptiDriveMapProperty = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.BlobPath"); break}
    ContainerName	{$TeleoptiDriveMapProperty="teleopticcc/Settings"; break}        
	AccountKey		{$TeleoptiDriveMapProperty = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.AccountKey"); break}
	DataSourceName	{$TeleoptiDriveMapProperty = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.DataSourceName"); break}
	default			{$TeleoptiDriveMapProperty="Unknown Value"; break}
           
    }
	return $TeleoptiDriveMapProperty
}

function Hostsfile-Add-Cname {
    param(
    [string]$CName
    )
	
	$ipV4 = Test-Connection -ComputerName (hostname) -Count 1  | Select IPV4Address
	$hostentry = "$($ipV4.IPV4Address.IPAddressToString) $CName"
    $hostsFile = "$($env:windir)\system32\Drivers\etc\hosts"
    If ((Get-Content "$hostsFile" ) -notcontains "$hostentry") {
        Add-Content -Encoding UTF8  "$hostsFile" "`r`n"
        Add-Content -Encoding UTF8  "$hostsFile" "$hostentry"
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

function EventlogSource-Create {
    Param([string]$EventSourceName)
	log-info "Creating event log..."
    $type = "Application"
    #create event log source
    if ([System.Diagnostics.EventLog]::SourceExists("$EventSourceName") -eq $false) {
        [System.Diagnostics.EventLog]::CreateEventSource("$EventSourceName", $type)
        }
}


##===========
## Main
##===========
$directory = Get-ScriptDirectory
$computer = gc env:computername


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
	
	#74478, #76734, #78787
    $DataSourceName = TeleoptiDriveMapProperty-get -name "DataSourceName"
    $Cname = "$DataSourceName.teleopticloud.com"
    Hostsfile-Add-Cname -Cname $Cname

	#Setting $Env:TeleoptiIsAzure = $true
	[Environment]::SetEnvironmentVariable("TeleoptiIsAzure", "true", "Machine")
	log "Environment variable 'TeleoptiIsAzure' is set to '$Env:TeleoptiIsAzure'"

	#Set environment variables for RoleInstanceID & Rolename
	[Environment]::SetEnvironmentVariable("RoleName", [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::CurrentRoleInstance.Role.Name, "Machine") 
	[Environment]::SetEnvironmentVariable("RoleInstanceID", [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::CurrentRoleInstance.Id, "Machine")
	
	#Set Datasource customer name as env variables
	$Env:CustomerName = $DataSourceName

    
}
Catch [Exception]
{
    $ErrorMessage = $_.Exception.Message
	log-error "$ErrorMessage"
	log-info "$ErrorMessage"
    Throw "Script failed, Check Windows event log for details"
}
Finally
{
    log-info "End of Script."
}