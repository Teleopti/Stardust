$TargetServer = "Proteus"

$Username = "toptinet\tfsintegration"
$Password = "m8kemew0rk"
$pw = convertto-securestring -AsPlainText -Force -String $Password
$credentials = new-object -typename System.Management.Automation.PSCredential -argumentlist $Username,$pw

#Path to InstallUtil for service installations
$InstallUtil = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe"

#Path to service exe on Targetserver
$ScheduleExePath = "C:\Temp\ServiceInstall\AirproFi.ScheduleChangeListener\ScheduleChangeListener.exe"
$ShiftExePath = "C:\Temp\ServiceInstall\AirproFi.ShiftAndPersAccHandler\ShiftAndPersAccHandler.exe"

#Schedule
$ScheduleSource = "$PSScriptroot\..\Teleopti.Ccc.Payroll.Customers.AirproFi.ScheduleChangeListener\bin\Release\*"
$ScheduleDestination = "\\$TargetServer\c$\temp\ServiceInstall\AirproFi.ScheduleChangeListener"

#Shift
$ShiftSource = "$PSScriptroot\..\Teleopti.Ccc.Payroll.Customers.AirproFi.ShiftAndPersAccHandler\bin\Release\*"
$ShiftDestination = "\\$TargetServer\c$\temp\ServiceInstall\AirproFi.ShiftAndPersAccHandler"

#Service names
$ServiceScheduleChangeListener = "TeleoptiTimeBankScheduleChangeListener"
$ServiceTimeBankScheduleValidator = "TeleoptiTimeBankScheduleValidator"


function CopyPayrollDlls
{
    param (
        $Source,
        $Destination
    )
        if (!(Test-path $destination)) 
        {

            New-Item -ItemType directory -Path $destination
        }

            Copy-Item $Source $Destination -recurse -force -verbose

}

function InstallPayrollService
{
    param (

        $Computername,
        $ServiceExePath,
        $remove = $false
    )
    
    $install = "/i"
    
    if ($remove -eq $true) 
    {
        $install = "/u" 
    }
  
    
    Invoke-Command -ComputerName $Computername -Credential $credentials -ScriptBlock {

    &'C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe' $using:ServiceExePath $using:install
    
    }

}

function fnStartRemoteService
{
    param (

        $Computername,
        $Servicename
    )
        Write-Host "Starting"
        Get-Service -Name $Servicename -ComputerName $Computername | Start-Service
}

function Main
{
	
    net use "\\$TargetServer\c$" $Password /USER:$Username /persistent:no

    CopyPayrollDlls $ScheduleSource $ScheduleDestination
    CopyPayrollDlls $ShiftSource $ShiftDestination

    InstallPayrollService $TargetServer $ScheduleExePath #-Remove $true
    InstallPayrollService $TargetServer $ShiftExePath #-Remove $true
	
	fnStartRemoteService $TargetServer $ServiceScheduleChangeListener
	fnStartRemoteService $TargetServer $ServiceTimeBankScheduleValidator
	
}

try
{
	Main
}
catch
{
	Log $error[0]
	echo $_.Exception | format-list -force
	exit 1
}