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

##===========
## Main
##===========
$directory = Get-ScriptDirectory
$computer = gc env:computername

Try
{
    #[Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime")
	
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
	
		## Destination directory. Files in this directory will mirror the source directory. Extra files will be deleted!
		
    
    $BlobPath = "https://cloudinstancestartupscri.blob.core.windows.net/"
	$ContainerName="startupscripts"
	$AccountKey = "s/7wYuHtwzhykg5hkkzEX9dEotpn4LDm6VNRKeRZBt74zr/8+DRcHPE1Zc8soKtMr9lrzCpMyJtjjKlt5SzvWA=="
	#$DataSourceName = "teleopticcc-dev"

	## FileWatch destination directory
	#$FILEWATCH = $directory + "\..\Services\ServiceBus\Payroll.DeployNew"
	#$FILEWATCHNEWDIR = $directory + "\..\Services\ServiceBus\Payroll"

	## Options to be added to AzCopy
	$OPTIONS = @("/S","/XO","/Y","/sourceKey:$AccountKey")
	
	#Support AzCopy 6.3 version
	$BlobSourceArgs = "/Source:" + $BlobPath + $ContainerName #+ "/" + $DataSourceName
	$DESTINATIONArgs = "/Dest:" + $directory

	## Wrap all above arguments
	$cmdArgs = @("$BlobSourceArgs","$DESTINATIONArgs",$OPTIONS)

	$AzCopyExe = $directory + "\ccc7_azure\AzCopy\AzCopy.exe"
	$AzCopyExe

	log-info "Copying Startup tasks from blob storage..."
	## Start the azcopy with above parameters and log errors in Windows Eventlog.
	& $AzCopyExe @cmdArgs
    $AzExitCode = $LastExitCode
    
    if ($LastExitCode -ne 0) {
		log-error "AzCopy generated an error!"
		log-info "AzCopy generated an error!"
        throw "AzCopy generated an error!"
    }

Log-Info "Running InitHost.cmd..."
& InitHost.cmd
Log-Info "Running PatchDatabases.cmd..."
& PatchDatabases.cmd
Log-Info "Running PrepareConfigAndSignClickOnce.cmd..."
& PrepareConfigAndSignClickOnce.cmd
Log-Info "Running InstallServiceV2.cmd..."
& InstallServiceV2.cmd
Log-Info "Running InitializeCopyPayroll.cmd..."
& InitializeCopyPayroll.cmd
Log-Info "Running TLSHardening.cmd..."
& TLSHardening.cmd
Log-Info "Running CustomStartup.cmd..."
& CustomStartup.cmd

       
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