# Check id and sec principal of current user
$myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()
$myWindowsPrincipal=new-object System.Security.Principal.WindowsPrincipal($myWindowsID)
 
# Get sec principal for the Administrator
$adminRole=[System.Security.Principal.WindowsBuiltInRole]::Administrator
 
# Check if we are currently running "as Administrator"
if ($myWindowsPrincipal.IsInRole($adminRole))
   {
   # If we are running as admin, change background colour to indicate this
   $Host.UI.RawUI.WindowTitle = $myInvocation.MyCommand.Definition + '(Elevated)'
   $Host.UI.RawUI.BackgroundColor = 'DarkGreen'
   clear-host
   }
else
   {
   # Relaunch as admin if needed
   # Create a new process and start Powershell
   $newProcess = new-object System.Diagnostics.ProcessStartInfo "PowerShell";
   
   # Specify the current script path and name as a parameter
   $newProcess.Arguments = $myInvocation.MyCommand.Definition;
   
   # Indicate that the process should be elevated
   $newProcess.Verb = "runas";
   
   # Start the new process
   [System.Diagnostics.Process]::Start($newProcess);
   
   # Exit from the current, unelevated, process
   exit
   }

$global:value = (netsh http show urlacl url=http://+:14000/)

if(!($global:value  -like '*http://+:14000/*'))
{
    $global:value = (netsh http add urlacl url=http://+:14000/ user=Everyone listen=yes)

    if($global:value  -like '*Error: 5*')
    {
        Write-host 'You must run this as administrator to add permissions to listen on ports'
        start-sleep -seconds 5
        Exit   
    }
    netsh http add urlacl url=http://+:14100/ user=Everyone listen=yes
}



