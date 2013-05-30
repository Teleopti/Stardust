function IISAdmin {
	$ModuleName = "WebAdministration"
	$ModuleLoaded = $false
	$LoadAsSnapin = $false

	if ($PSVersionTable.PSVersion.Major -ge 2)
	{
		if ((Get-Module -ListAvailable | ForEach-Object {$_.Name}) -contains $ModuleName)
		{
			Import-Module $ModuleName
			if ((Get-Module | ForEach-Object {$_.Name}) -contains $ModuleName)
			{
				$ModuleLoaded = $true
			}
			else
			{
				$LoadAsSnapin = $true
			}
		}
		elseif ((Get-Module | ForEach-Object {$_.Name}) -contains $ModuleName)
		{
			$ModuleLoaded = $true
		}
		else
		{
			$LoadAsSnapin = $true
		}
	}
	else
	{
		$LoadAsSnapin = $true
	}

	if ($LoadAsSnapin)
	{
		if ((Get-PSSnapin -Registered | ForEach-Object {$_.Name}) -contains $ModuleName)
		{
			Add-PSSnapin $ModuleName
			if ((Get-PSSnapin | ForEach-Object {$_.Name}) -contains $ModuleName)
			{
				$ModuleLoaded = $true
			}
		}
		elseif ((Get-PSSnapin | ForEach-Object {$_.Name}) -contains $ModuleName)
		{
			$ModuleLoaded = $true
		}
	}
}

function Get-Authentication {
    param(
        $path, #TeleoptiCCC/SDK
		$type #windowsAuthentication
    )
	$pspath = "MACHINE/WEBROOT/APPHOST/Default Web Site/" + $path
	
	$temp = Get-WebConfigurationProperty  -pspath $pspath -filter /system.webServer/security/authentication/$type -name enabled
	return $temp.Value
}

function Get-defaultDocument{
	Get-WebConfigurationProperty -Filter //defaultDocument/files/add -PSPath 'IIS:\Sites\Default Web Site' -Name value
}


function Test-RegistryKeyValue
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)]
        [string]
        # The path to the registry key
        $Path,

        [Parameter(Mandatory=$true)]
        [string]
        # The name of the value
        $Name
    )

    if( -not (Test-Path -Path $Path -PathType Container) )
    {
        return $false
    }

    $properties = Get-ItemProperty -Path $Path 
    if( -not $properties )
    {
        return $false
    }

    $member = Get-Member -InputObject $properties -Name $Name
    if( $member )
    {
        return $true
    }
    else
    {
        return $false
    }

}

function Get-UninstallRegPath {
    param(
        $MsiKey
    )

    # paths: x86 and x64 registry keys are different
    if ([IntPtr]::Size -eq 4) {
        $paths = 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\' + $MsiKey
    }
    else {
        $paths = @(
            'HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\' + $MsiKey
            'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\' + $MsiKey)
    }
    
    foreach ($path in $paths.GetEnumerator()) {
		if (Test-path $path) {
			return $path    
		}
		else {
			return null
		}
	}
}

Function Uninstall-ByRegPath(){
    [CmdletBinding()]
    Param (
    [Parameter(Position=1,Mandatory=$true, ValueFromPipelineByPropertyName=$true,HelpMessage="Regkey to the Application to be uninstalled")] [Alias("a")] [string]$path
    )
    Process {
		$Error.Clear()
		try {

			$appKey = Get-Item -Path $path

			#Write-Host $appKey.gettype()
			if ($appKey -is [Microsoft.Win32.RegistryKey]) # If it is a system.object or something else this will fail.
			{
				$UninstallString = $appKey.GetValue("UninstallString")
			}
			else
			{ #The application was not found.
				Write-Host "The application $appName was not found so no uninstallation was performed."
			}
			
			#check to make sure this is an MSI based application so we don't get unexpected behavior
			if ($UninstallString.StartsWith("msiexec.exe","CurrentCultureIgnoreCase"))
			{
				$Arguments = @()
				$Arguments += "/Q"
				$Arguments += "/X"
				$Arguments += $UninstallString.substring($UninstallString.indexof("{"),$UninstallString.Length - $UninstallString.indexof("{"))
				$ExitCode = (Start-Process -FilePath "msiexec.exe" -ArgumentList $Arguments -Wait -Passthru).ExitCode
				if ($ExitCode -ne 0)
				{
				throw "The MSI failed to get uninstalled. MSIEXEC.exe returned an exit code of $ExitCode."
				} 
			}
		}

		catch [Exception] {
		$EMsg = Write-Host  "Failed in WinUtils::uninstall-application : " + $_.Exception 
		throw
		}
	}
}
