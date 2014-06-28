Import-Module Carbon

function Load-SnapIn {
    param(
        $ModuleName
    )
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
	
	$iis = new-object Microsoft.Web.Administration.ServerManager 
	
	$App = $iis.Sites | foreach {$_.Applications | where { $_.Path -eq "$path"}}
	$temp = $App.GetWebConfiguration().GetSection("system.webServer/security/authentication/$type").GetAttributeValue("enabled")
	
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

    $WmiQuery = Get-WmiObject -Class Win32_OperatingSystem | Select-Object OSArchitecture
    # paths: x86 and x64 registry keys are different
    if ($WmiQuery.OSArchitecture -eq "64-bit") {
        $paths = @('HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\' + $MsiKey)
    }
    else {
        $paths = @('HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\' + $MsiKey)
    }
    
    #check if any of them is acutally a folder path
    foreach ($path in $paths.GetEnumerator()) {
		if ((Test-Path -Path $path -PathType Container)) {
			return $path    
		}
		else {
			return null
		}
	}
}

function Check-ProductIsInstalled
{
    param(
        $DisplayName
    )
    $r = Get-WmiObject Win32_Product | Where {$_.Name -match "$DisplayName"}
    if ($r -ne $null) {
        return $True
        }
    else  {
        return $False
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
            Remove-Item -Path HKLM:\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC\InstallationSettings
		}

		catch [Exception] {
		$EMsg = Write-Host  "Failed in WinUtils::uninstall-application : " + $_.Exception 
		throw
		}
	}
}

function UnZip-File(){
    param(
        [string]$zipfilename,
        [string] $destination
        )

    if(test-path($zipfilename))
    { 
        $shellApplication = new-object -com shell.application
        $zipPackage = $shellApplication.NameSpace($zipfilename)
        $destinationFolder = $shellApplication.NameSpace($destination)
        #this does not work as tfsintergration
        #$destinationFolder.CopyHere($zipPackage.Items(),20)

        #trying this instead
        $CMD = 'C:\Program Files\7-zip\7z'
        $arg1 = 'e'
        $arg2 = '-o' + $destination
        & $CMD $arg1 $arg2 $zipfilename
    }
} 

function Copy-ZippedMsi{
    param(
        $workingFolder,
        $version
    )
	#"\\HEBE\Installation\PBImsi\Kanbox\CCC-7.5.390-MSI"
    $scrFolder='\\hebe\Installation\PBImsi\Kanbox\CCC-' + $version + '-MSI'
    $destFolder=$workingFolder
    Write-Host 'source: ' $scrFolder
    $zipFileName = Get-ChildItem $scrFolder -filter "*.zip" | Select-Object -First 1
    
    if (!(Test-Path "$destFolder\$zipFileName")) {
        Copy-Item "$scrFolder\$zipFileName" "$destFolder"
    }
    return @("$destFolder\$zipFileName")
}


function destroy-WorkingFolder{
    param(
        $workingFolder
    )
	if (Test-Path "$workingFolder") {
		remove-item "$workingFolder\" -Recurse
	}
}

function create-WorkingFolder{
    param(
        $workingFolder
    )
	if (!(Test-Path "$workingFolder")) {
		& mkdir "$workingFolder"
	}
}

function Check-HttpStatus {     
	param(
	[string] $url,
    [System.Net.NetworkCredential]$credentials = $null
	)

	[net.httpWebRequest] $req = [net.webRequest]::create($url)
    $req.Credentials = $credentials;
	$req.Method = "GET"
    Write-Host 'Check-HttpStatus: ' $url
	[net.httpWebResponse] $res = $req.getResponse()
    
	if ($res.StatusCode -ge "200") {
		return $true
	}
	else {
		return $false
	}
}

function Install-TeleoptiCCCServer
{
    param (
    [string]$BatchFile,
    [array]$ArgArray
    )

	#add an extra argument, the final batch file
	$temp = (get-childitem -path env:temp).Value + "\temp.bat"
	$ArgArray +=$temp

	#create the final batch file
	Start-Process -FilePath $BatchFile -ArgumentList $ArgArray -NoNewWindow -Wait -RedirectStandardOutput stdout.log -RedirectStandardError stderr.log
	
	#run the final batch file
	Get-Content $temp
	Start-Process -FilePath $temp -NoNewWindow -Wait -RedirectStandardOutput stdout.log -RedirectStandardError stderr.log
}



function Add-UserToLocalGroup{
     Param(
        $groupName,
        $computer=$env:computername,
        $userdomain,
        $username
    )
		if(![ADSI]::Exists("WinNT://$computer/$groupName")) { 
			Write-Host "No group with that name: '$groupName'"
			return
		}
		
	$Group= [ADSI]"WinNT://$computer/$groupName,group"
    $members= $Group.psbase.invoke("Members") | %{$_.GetType().InvokeMember("Name", 'GetProperty', $null, $_, $null)}
    $userFound = $members -contains $username
    
	if ($userFound)
		{"The user '$username' already exists in group '$groupName'."}
	else {
			([ADSI]"WinNT://$computer/$groupName,group").psbase.Invoke("Add",([ADSI]"WinNT://$userdomain/$username").path)
		}
}

function Create-LocalGroup
{
	Param(
		$groupName,
		$computer=$env:computername
	)
	if(!$groupName)
	{
		$(Throw 'A value for $groupName is required!')
	}

	if(![ADSI]::Exists("WinNT://$computer/$groupName")) { 
		$computer=$env:computername
		$objOu = [ADSI]"WinNT://$computer"
		$objUser = $objOU.Create("Group", $groupName)
		$objUser.SetInfo()
	}
}

function Stop-TeleoptiCCC
{
    $batchFile = "C:\Program Files (x86)\Teleopti\SupportTools\StartStopSystem\StopSystem.bat"
    
    [string]$ErrorMessage = "Stop system failed!"
    & "$BatchFile" | Out-Null
    if ($LastExitCode -ne 0) {
        throw "Exec: $ErrorMessage"
    }
}

function Start-TeleoptiCCC
{
    $batchFile = "C:\Program Files (x86)\Teleopti\SupportTools\StartStopSystem\StartSystem.bat"
    
    [string]$ErrorMessage = "Start system failed!"
    & "$BatchFile" | Out-Null
    if ($LastExitCode -ne 0) {
        throw "Exec: $ErrorMessage"
    }
}

function Check-ServiceIsRunning{
	param(
		$ServiceName
 	)
    $arrService = Get-Service -Name $ServiceName
    if ($arrService.Status -eq "Running") {
    	return $True
    }
    else {
	return $False
    }
}

function Start-MyService{
	param($ServiceName)
	$arrService = Get-Service -Name $ServiceName
	if ($arrService.Status -ne "Running"){
		Start-Service $ServiceName
	}
}

function Stop-MyService{
	param($ServiceName)
	$arrService = Get-Service -Name $ServiceName
	if ($arrService.Status -ne "Stopped"){
		Stop-Service $ServiceName
	}
}

function stop-AppPool{
    param($PoolName)
            Invoke-AppCmd Stop Apppool "$PoolName"
			Invoke-AppCmd Set Apppool "$PoolName" /autoStart:false
}

function start-AppPool{
    param($PoolName)
            Invoke-AppCmd Start Apppool "$PoolName"
			Invoke-AppCmd Set Apppool "$PoolName" /autoStart:true
}

function restoreToBaseline
{
    param($computerName,
            $spContent)
$stringDrop = "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RestoreToBaseline]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RestoreToBaseline]"
$con = New-Object System.Data.SqlClient.SqlConnection
$con.ConnectionString = "Server=$Server;Database=master;Integrated Security=true"
$con.Open()

# Create SqlCommand object, define command text, and set the connection
$cmd = New-Object System.Data.SqlClient.SqlCommand
$cmd.Connection = $con
$cmd.CommandTimeout = 0
#the sp
$cmd.CommandText = $stringDrop
$cmd.ExecuteNonQuery()

$cmd.CommandText = $spContent
$cmd.ExecuteNonQuery()
#and run it
$cmd.CommandText = "RestoreToBaseline '$computerName'" 
$cmd.ExecuteNonQuery()


}
 
function insert-License{
    param($Server,
            $Db,
            $xmlString)
# Create SqlConnection object, define connection string, and open connection
$con = New-Object System.Data.SqlClient.SqlConnection
$con.ConnectionString = "Server=$Server;Database=$Db;Integrated Security=true"
$con.Open()

# Create SqlCommand object, define command text, and set the connection
$cmd = New-Object System.Data.SqlClient.SqlCommand
$cmd.Connection = $con
$cmd.CommandText = "DELETE FROM License"
$cmd.ExecuteNonQuery()

$cmd.CommandText = "if exists(select object_id from sys.columns  where Name = N'CreatedBy' and Object_ID = Object_ID(N'License')) 
select 1 else select 0"
$sqlReader = $cmd.ExecuteReader()

while ($sqlReader.Read()) { $exists = $sqlReader[0]}

$sqlReader.Close()

if($exists -eq 1)
{
    $cmd.CommandText = "INSERT INTO License
      (Id, Version, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, XmlString)
      VALUES (@Id, @Version, @CreatedBy, @UpdatedBy, @CreatedOn, @UpdatedOn, @XmlString)"
}
else
{
    $cmd.CommandText = "INSERT INTO License
      (Id, Version,  UpdatedBy,  UpdatedOn, XmlString)
      VALUES (@Id, @Version, @UpdatedBy, @UpdatedOn, @XmlString)"
}
$superUser = "3f0886ab-7b25-4e95-856a-0d726edc2a67"
$now = Get-Date

# Add parameters to pass values to the INSERT statement
$cmd.Parameters.AddWithValue("@Id", [guid]::NewGuid()) | Out-Null
$cmd.Parameters.AddWithValue("@Version", 1) | Out-Null
$cmd.Parameters.AddWithValue("@UpdatedBy", $superUser) | Out-Null
$cmd.Parameters.AddWithValue("@UpdatedOn", $now) | Out-Null
$cmd.Parameters.AddWithValue("@XmlString", $xmlString) | Out-Null

if($exists -eq 1)
{
    $cmd.Parameters.AddWithValue("@CreatedBy", $superUser) | Out-Null
    $cmd.Parameters.AddWithValue("@CreatedOn", $now) | Out-Null
}

# Execute INSERT statement
$global:insertedLicense = $cmd.ExecuteNonQuery()
Write-Host 'Inserted License: ' $global:insertedLicense
return $global:insertedLicense -eq 1
}