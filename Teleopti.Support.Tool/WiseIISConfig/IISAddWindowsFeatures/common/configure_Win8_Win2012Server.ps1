function restartTeleoptiWFM {
	$batchfile = "$scriptPath\..\..\..\StartStopSystem\ResetSystem.bat"
	$restart = [diagnostics.process]::Start($batchfile)
	$restart.WaitForExit()
}

function IsSupportedOS {
	[CmdletBinding()]
	param($OSName)

	$File = "$scriptPath\SupportedOSList.txt"

	$OsList = Get-Content $File
    foreach ($Os in $OsList) {
        Write-Host $Os
        $temp = $Os.TrimEnd("*").ToLower()

        if ($OSName.Contains($temp)) {
            return $True
        }
	}
}

function InstallWin8Features {
	$File = "$scriptPath\Win8Features.txt"
	$featureList=Get-Content $File
	foreach ($feature in $featureList) {
		if ($feature.Contains("#") -Or [string]::IsNullOrWhiteSpace($feature)) {Continue}
		write-host $feature
		&"dism" /online /enable-feature /featurename:"$feature"
		}
	}
	
##===========
## Main
##===========
# Get the ID and security principal of the current user account
$myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()
$myWindowsPrincipal=new-object System.Security.Principal.WindowsPrincipal($myWindowsID)
 
# Get the security principal for the Administrator role
$adminRole=[System.Security.Principal.WindowsBuiltInRole]::Administrator
 
# Check to see if we are currently running "as Administrator"
if ($myWindowsPrincipal.IsInRole($adminRole))
   {
   # We are running EventlogSource-Create"as Administrator" - so change the title and background color to indicate this
   $Host.UI.RawUI.WindowTitle = $myInvocation.MyCommand.Definition + "(Elevated)"
   clear-host
   }
else
   {
   # We are not running "as Administrator" - so relaunch as administrator
   
   # Create a new process object that starts PowerShell
   $newProcess = new-object System.Diagnostics.ProcessStartInfo "PowerShell";
   
   # Specify the current script path and name as a parameter
   $newProcess.Arguments = $myInvocation.MyCommand.Definition;
   
   # Indicate that the process should be elevated
   $newProcess.Verb = "runas";
   
   # Start the new process, in elevated mode
   [System.Diagnostics.Process]::Start($newProcess);
   
   # Exit from the current, non elevated process
   exit
   }

 Try
{
	$source = "Teleopti.Wfm.ConfigureIIS8"
	$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition

    if ([System.Diagnostics.EventLog]::SourceExists($source) -eq $false) {
        [System.Diagnostics.EventLog]::CreateEventSource($source, "Application")
    }

    $OSName=(Get-WmiObject Win32_OperatingSystem).Caption.ToLower()
    if (IsSupportedOS($OSName)) {

        Write-EventLog -LogName Application -Source $source -EventID 1 -EntryType Information -Message "Add needed IIS features..."
		InstallWin8Features
        Write-EventLog -LogName Application -Source $source -EventID 1 -EntryType Information -Message "Add needed IIS features. Done!"
		
		restartTeleoptiWFM
    }
}

Catch [Exception]
{
	$ErrorMessage = $_.Exception.Message
	Write-Host "$ErrorMessage"
	Start-Sleep -s 3
	Write-EventLog -LogName Application -Source $source -EventID 1 -EntryType Error -Message "$ErrorMessage"
	Throw "Script failed, Check Windows event log for details"
}
Finally
{
	Write-Host "done"
}