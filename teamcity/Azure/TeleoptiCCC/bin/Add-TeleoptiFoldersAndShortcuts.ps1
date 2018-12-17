Function Add-TeleoptiWebFolders {
    foreach ($appMapping in $appsArray) {
        $SymName=$appMapping[0]
        $Path=$appMapping[1]
        if (Test-Path $Path) {
            New-SymLink -Path "$Path" -SymName "$SymName" -Directory
        }
    }
}


Function Add-TeleoptiToolsAndServices {
    $physicalFolder = $approot + "\Services\ETL\Service"
    $symFolder      = $TeleoptiCCCPath + "\ETL\Service"
    if (Test-Path $physicalFolder) {
        New-SymLink -Path "$physicalFolder" -SymName $symFolder -Directory
    }

    $physicalFolder = $approot + "\Services\ETL\Tool"
    $symFolder      = $TeleoptiCCCPath + "\ETL\Tools"
    if (Test-Path $physicalFolder) {
        New-SymLink -Path "$physicalFolder" -SymName $symFolder -Directory
    }

    $physicalFolder = $approot + "\ServiceBus"
    $symFolder      = $TeleoptiCCCPath + "\Tool\service"
    if (Test-Path $physicalFolder) {
        New-SymLink -Path "$physicalFolder" -SymName $symFolder -Directory
    }
}


Function Add-InstallGoodies {
    $file="$desktopPublic\InstallGoodies.bat"

    $text = 'choco install google-chrome-x64 -y'
    $text | Set-Content $file

    $text = 'choco install baretail -y'
    $text | Add-Content $file

    $text = 'choco install notepadplusplus.install --x86 -y'
    $text | Add-Content $file
}

Function Add-DesktopShortcuts {

    # restart Teleopti
    $WshShell = New-Object -ComObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut("$desktopPublic\Restart.lnk")
    $Shortcut.TargetPath = $restartScript
    $Shortcut.Save()

    $Shortcut = $WshShell.CreateShortcut("$desktopPublic\StartPage.lnk")
    $Shortcut.TargetPath = "$StartUrl"
    $Shortcut.IconLocation = "$WfmIco"
    $Shortcut.Save()

    $Shortcut = $WshShell.CreateShortcut("$desktopPublic\TeleoptiFiles.lnk")
    $Shortcut.TargetPath = "$TeleoptiCCCPath"
    $Shortcut.Save()

    $Shortcut = $WshShell.CreateShortcut("$desktopPublic\EventViewer.lnk")
    $Shortcut.TargetPath = "%windir%\system32\eventvwr.msc"
    $Shortcut.Save()

    $Shortcut = $WshShell.CreateShortcut("$desktopPublic\Services.lnk")
    $Shortcut.TargetPath = "%windir%\system32\services.msc"
    $Shortcut.Save()

	$Shortcut = $WshShell.CreateShortcut("$desktopPublic\EtlTool.lnk")
	$Shortcut.TargetPath = "$EtlToolExe"
    $Shortcut.IconLocation = "$WfmIco"
    $Shortcut.Save()
	
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($WshShell)
    Remove-Variable WshShell

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

Function New-SymLink {
    <#
        .SYNOPSIS
            Creates a Symbolic link to a file or directory

        .DESCRIPTION
            Creates a Symbolic link to a file or directory as an alternative to mklink.exe

        .PARAMETER Path
            Name of the path that you will reference with a symbolic link.

        .PARAMETER SymName
            Name of the symbolic link to create. Can be a full path/unc or just the name.
            If only a name is given, the symbolic link will be created on the current directory that the
            function is being run on.

        .PARAMETER File
            Create a file symbolic link

        .PARAMETER Directory
            Create a directory symbolic link

        .NOTES
            Name: New-SymLink
            Author: Boe Prox
            Created: 15 Jul 2013


        .EXAMPLE
            New-SymLink -Path "C:\users\admin\downloads" -SymName "C:\users\admin\desktop\downloads" -Directory

            SymLink                          Target                   Type
            -------                          ------                   ----
            C:\Users\admin\Desktop\Downloads C:\Users\admin\Downloads Directory

            Description
            -----------
            Creates a symbolic link to downloads folder that resides on C:\users\admin\desktop.

        .EXAMPLE
            New-SymLink -Path "C:\users\admin\downloads\document.txt" -SymName "SomeDocument" -File

            SymLink                             Target                                Type
            -------                             ------                                ----
            C:\users\admin\desktop\SomeDocument C:\users\admin\downloads\document.txt File

            Description
            -----------
            Creates a symbolic link to document.txt file under the current directory called SomeDocument.
    #>
    [cmdletbinding(
        DefaultParameterSetName = 'Directory',
        SupportsShouldProcess=$True
    )]
    Param (
        [parameter(Position=0,ParameterSetName='Directory',ValueFromPipeline=$True,
            ValueFromPipelineByPropertyName=$True,Mandatory=$True)]
        [parameter(Position=0,ParameterSetName='File',ValueFromPipeline=$True,
            ValueFromPipelineByPropertyName=$True,Mandatory=$True)]
        [ValidateScript({
            If (Test-Path $_) {$True} Else {
                Throw "`'$_`' doesn't exist!"
            }
        })]
        [string]$Path,
        [parameter(Position=1,ParameterSetName='Directory')]
        [parameter(Position=1,ParameterSetName='File')]
        [string]$SymName,
        [parameter(Position=2,ParameterSetName='File')]
        [switch]$File,
        [parameter(Position=2,ParameterSetName='Directory')]
        [switch]$Directory
    )
    Begin {
        Try {
            $null = [mklink.symlink]
        } Catch {
            Add-Type @"
            using System;
            using System.Runtime.InteropServices;
 
            namespace mklink
            {
                public class symlink
                {
                    [DllImport("kernel32.dll")]
                    public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);
                }
            }
"@
        }
    }
    Process {
        #Assume target Symlink is on current directory if not giving full path or UNC
        If ($SymName -notmatch "^(?:[a-z]:\\)|(?:\\\\\w+\\[a-z]\$)") {
            $SymName = "{0}\{1}" -f $pwd,$SymName
        }
        $Flag = @{
            File = 0
            Directory = 1
        }
        If ($PScmdlet.ShouldProcess($Path,'Create Symbolic Link')) {
            Try {
                $return = [mklink.symlink]::CreateSymbolicLink($SymName,$Path,$Flag[$PScmdlet.ParameterSetName])
                If ($return) {
                    $object = New-Object PSObject -Property @{
                        SymLink = $SymName
                        Target = $Path
                        Type = $PScmdlet.ParameterSetName
                    }
                    $object.pstypenames.insert(0,'System.File.SymbolicLink')
                    $object
                } Else {
                    Throw "Unable to create symbolic link!"
                }
            } Catch {
                Write-warning ("{0}: {1}" -f $path,$_.Exception.Message)
            }
        }
    }
 }

#===============
#main
#===============
# Are we elevated? else restart script
# Get the ID and security principal of the current user account
$myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()
$myWindowsPrincipal=new-object System.Security.Principal.WindowsPrincipal($myWindowsID)

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 
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

    #paths
    log-info "running: $ScriptFileName"
    $debug = 0
    if ($debug -eq 1)
    {
        $sitesroot = "C:\temp\eDrive\siteroot"
        $approot  = "C:\temp\eDrive\approot"
        $desktopPublic = "C:\Users\Public\Desktop"
    } else {
        $sitesroot = "E:\sitesroot"
        $approot  = "E:\approot"
        $desktopPublic = "D:\Users\Public\Desktop"
        }
    $TeleoptiCCCPath = "C:\Program Files (x86)\Teleopti\TeleoptiCCC"
    $WfmIco = "$approot\Tools\SupportTools\BuildArtifacts\ccc_Menu.ico"
    $restartScript = "$approot\bin\RestartTeleopti.cmd"
    $DataSourceName = TeleoptiDriveMapProperty-get -name "DataSourceName"
    $StartUrl = "https://$DataSourceName.teleopticloud.com/"
	$EtlToolExe = "$approot\Services\ETL\Tool\Teleopti.Analytics.Etl.ConfigTool.exe"
    
    #Array describing "SymLink,Pysicalpath"
    $appsArray = @(
                ("$TeleoptiCCCPath\SDK","$sitesroot\1"),
                ("$TeleoptiCCCPath\Web","$sitesroot\3"),
                ("$TeleoptiCCCPath\Client","$sitesroot\4"),
                ("$TeleoptiCCCPath\RTA","$sitesroot\5"),
                ("$TeleoptiCCCPath\AuthenticationBridge","$sitesroot\6"),
                ("$TeleoptiCCCPath\WindowsIdentityProvider","$sitesroot\7"),
                ("$TeleoptiCCCPath\Administration","$sitesroot\8"),
                ("$TeleoptiCCCPath\API","$sitesroot\9")
            )


        if (!(Test-Path $TeleoptiCCCPath)) {
            New-Item "$TeleoptiCCCPath" -type directory
            }
        if (!(Test-Path "$TeleoptiCCCPath\ETL")) {
            New-Item "$TeleoptiCCCPath\ETL" -type directory
            }
        if (!(Test-Path "$TeleoptiCCCPath\ServiceBus")) {
            New-Item "$TeleoptiCCCPath\ServiceBus" -type directory
            }
        
        log-info "Add-TeleoptiWebFolders ..."
        Add-TeleoptiWebFolders;
        log-info "Add-TeleoptiToolsAndServices ..."
        Add-TeleoptiToolsAndServices;
        log-info "Add-DesktopShortcuts ..."
        Add-DesktopShortcuts;

        #add chocolatey to the instance
        log-info "Add Chocolatey ..."
        iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))
        log-info "Add Goodies batch ..."
        Add-InstallGoodies;
}

Catch [Exception]
{
	$ErrorMessage = $_.Exception.Message
    log-error "$ErrorMessage"
	Throw "Script failed, Check log for details";
}
Finally
{
	log-info "Done!"
}