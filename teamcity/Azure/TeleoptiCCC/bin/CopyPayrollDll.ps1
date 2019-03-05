#powershell v2 won't get $PSScriptroot, revert back to old style...
if (!$PSScriptroot)
{
    $PSScriptroot = split-path -parent $MyInvocation.MyCommand.Definition
}

. "$PSScriptroot\..\Tools\SupportTools\StartStopSystem\RestartHelper.ps1"

##===========
## Functions
##===========
function FilesAreEqual
{
   param([System.IO.FileInfo] $first, [System.IO.FileInfo] $second) 
   $BYTES_TO_READ = 8;

   if ($first.Length -ne $second.Length)
   {
        return $false;
   }

   $iterations = [Math]::Ceiling($first.Length / $BYTES_TO_READ);
   $fs1 = $first.OpenRead();
   $fs2 = $second.OpenRead();

   $one = New-Object byte[] $BYTES_TO_READ;
   $two = New-Object byte[] $BYTES_TO_READ;

   for ($i = 0; $i -lt $iterations; $i = $i + 1)
   {
       $fs1.Read($one, 0, $BYTES_TO_READ) | out-null;
       $fs2.Read($two, 0, $BYTES_TO_READ) | out-null;

       if ([BitConverter]::ToInt64($one, 0) -ne 
           [BitConverter]::ToInt64($two, 0))
       {
           $fs1.Close();
           $fs2.Close();
           return $false;
       }
   }

   $fs1.Close();
   $fs2.Close();

   return $true;
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

function Sync-NewFilesOnly
{
    param(
    $sourceDir,
    $targetDir    
    )
    
[int]$counter = 0
    Get-ChildItem "$sourceDir" -recurse | Foreach-Object {
        $sourceFile = Get-Item $_.FullName
        
        $currDir = ($sourceFile).Directory.Name
        if (!($currDir -eq "PayrollInbox"))
        {
            $targetFile = $targetDir + "\" + $currDir + "\" + $Sourcefile.Name      
        }
        else
        {
            $targetFile = $targetDir + "\" + $sourceFile.Name  
        }

        if ($sourceFile.PsIscontainer) 
        {
            if (!(Test-Path $targetFile)) 
            {
                log-info "Folder:'$targetFile' doesn´t exist, will be created..."
                New-Item -ItemType Directory -Force -Path $targetFile | Out-Null
            }
        }
        else
        {
            if (!(FilesAreEqual $sourceFile $targetFile)) 
            { 
                
                $currDir = ($sourceFile).directory.name
                if (!($currDir -eq "PayrollInbox"))
                {
                    $destfile = $targetDir + "\" + $currDir + "\" + $Sourcefile.Name
                }
                else
                {
                    $destfile = $targetDir + "\" + $Sourcefile.Name
                } 
                
                log-info ""
                log-info "File:'$SourceFile' seems to be new..."
                log-info "Will be copied to: '$destfile'..."
                log-info ""
                Copy-Item $sourceFile  $destfile -Force
               
                $counter = $counter + 1
            }
        }
    }
    return $counter
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
Try
{
	#Get local path
    [string]$global:directory = split-path -parent $MyInvocation.MyCommand.Definition
    [string]$global:ScriptFileName = $MyInvocation.MyCommand.Name
    Set-Location $directory

 	#start log4net
	$log4netPath = $directory + "\log4net"
    Unblock-File -Path "$log4netPath\log4net.ps1"
    . "$log4netPath\log4net.ps1";
    $configFile = new-object System.IO.FileInfo($log4netPath + "\log4net.config");
    configure-logging -configFile "$configFile" -serviceName "$serviceName"
	
	log-info "running: $ScriptFileName"
	
    $TeleoptiServiceBus = "TeleoptiServiceBus"
    $computer = $env:computername

    ## Name of the job, name of source in Windows Event Log
    $JOB = "Teleopti.Ccc.BlobStorageCopy"

	##test if admin
	If (!(Test-Administrator($myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()))) {
		log-error "User is not Admin!"
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
   
    #$BlobSource = $BlobPath + $ContainerName + "/" + $DataSourceName

	## Destination directory. Files in this directory will mirror the source directory. Extra files will be deleted!
	$DESTINATION = "c:\temp\PayrollInbox"
	if (!(test-path -path $DESTINATION))
	{
		mkdir $DESTINATION
	}

	## FileWatch destination directory
	$FILEWATCH = $directory + "\..\Services\ServiceBus\Payroll.DeployNew"
	$FILEWATCHNEWDIR = $directory + "\..\Services\ServiceBus\Payroll"

	## Options to be added to AzCopy
	$OPTIONS = @("/S","/XO","/Y","/sourceKey:$AccountKey")
	
	#Support AzCopy 6.3 version
	$BlobSourceArgs = "/Source:" + $BlobPath + $ContainerName + "/" + $DataSourceName
	$DESTINATIONArgs = "/Dest:" + $DESTINATION

	## Wrap all above arguments
	$cmdArgs = @("$BlobSourceArgs","$DESTINATIONArgs",$OPTIONS)

	$AzCopyExe = $directory + "\ccc7_azure\AzCopy\AzCopy.exe"
	$AzCopyExe

	log-info "Copying Payroll from blob storage..."
	## Start the azcopy with above parameters and log errors in Windows Eventlog.
	& $AzCopyExe @cmdArgs
    $AzExitCode = $LastExitCode
    
    if ($LastExitCode -ne 0) {
		log-error "AzCopy generated an error!"
		log-info "AzCopy generated an error!"
        throw "AzCopy generated an error!"
    }

    $newFiles = Sync-NewFilesOnly $DESTINATION $FILEWATCH
	$newFiles1 = Sync-NewFilesOnly $DESTINATION $FILEWATCHNEWDIR
	##one or more files are new, log info to Eventlog and restart serviceBus
	If ($newFiles -ge "1" -or $newFiles1 -ge "1" ) {
		#log-info "Stopping service bus..."
        #StopWindowsService -ServiceName $TeleoptiServiceBus
    	#write-host "-------------" -ForegroundColor blue
		#log-info "Starting service bus..."
        #StartWindowsService -ServiceName $TeleoptiServiceBus
		log-info "$newFiles files synced from: $BlobSource to: '$FILEWATCH' and '$FILEWATCHNEWDIR'"
	}
	Write-EventLog -LogName Application -Source $JOB -EventID 0 -EntryType Information -Message "$newFiles files synced from: $BlobSource to: $FILEWATCH"
	Write-EventLog -LogName Application -Source $JOB -EventID 0 -EntryType Information -Message "$newFiles files synced from: $BlobSource to: $FILEWATCHNEWDIR"
}
Catch [Exception]
{
    $ErrorMessage = $_.Exception.Message
    Write-EventLog -LogName Application -Source $JOB -EventID 1 -EntryType Error -Message "$ErrorMessage"
	log-error "Script failed, Check Windows event log for details!"
	log-info "Script failed, Check Windows event log for details!"
	Throw "Script failed, Check Windows event log for details"
}
Finally
{
	log-info "End of Script."

}