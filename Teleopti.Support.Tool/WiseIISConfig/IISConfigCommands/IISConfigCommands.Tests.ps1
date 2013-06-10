$here = Split-Path -Parent $MyInvocation.MyCommand.Path
    $sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
    . "$here\$sut"

##============
#how to debug pester tests
##============
#1 launch Windows Powershell ISE
#2 open all .ps1 file of interest *.ps1 + *Tests.ps1
#3 set a break point ("F9") in *.Tests.ps1 file
#4 from the "command line" in ISE:
#    Import-Module "C:\data\main\ccnet\pester\Pester.2.0.3\tools\Pester.psm1"
#    Invoke-Pester "C:\data\main\Teleopti.Support.Tool\WiseIISConfig\IISConfigCommands\"
#5 step step ("F10")
##============

function Copy-ZippedMsi{
    $scrFolder='\\hebe\Installation\PBImsi\Kanbox\BuildMSI-main'
    $destFolder='c:\temp'

    $zipFileName = Get-ChildItem $scrFolder -filter "*.zip" | Select-Object -First 1
    
    if (!(Test-Path "$destFolder\$zipFileName")) {
        Copy-Item "$scrFolder\$zipFileName" "$destFolder"
    }
    return @("$destFolder\$zipFileName")
}

#Add IIS admin module
Load-SnapIn -ModuleName "WebAdministration"

$Ntml = "Ntlm"
$None = "None"
$InstallationAuthSetting = "Ntlm"
$CccServerMsiKey='{52613B22-2102-4BFB-AAFB-EF420F3A24B5}'

Describe "Tear down previous test"{
    [string] $path = Get-UninstallRegPath -MsiKey "$CccServerMsiKey"
	
    It "should uninstall product"{
        [bool] $isInstalled = Test-RegistryKeyValue -Path $path -Name "DisplayName"
        if ($isInstalled) {
            Uninstall-ByRegPath -path $path
        }

        $isInstalled = Test-RegistryKeyValue -Path $path -Name "DisplayName"
        $isInstalled | Should Be $False
	}

	It "should have a default web site" {
		$computerName=(get-childitem -path env:computername).Value
		$httpStatus=Check-HttpStatus -url "http://$computerName/"
		$httpStatus | Should Be $True
	}
		
	It "should throw exeption when http URL does not exist" {
		$computerName=(get-childitem -path env:computername).Value
		{Check-HttpStatus -url "http://$computerName/TeleoptiCCC/"}  | Should Throw
    }
}

Describe "Setup test"{  
    It "should copy latest .zip-file from build server"{
        $zipFile = Copy-ZippedMsi
        Test-Path $zipFile | Should Be $True
    }

    It "should unzip file into MSI"{
        $zipFile = Copy-ZippedMsi
        $zipFile = Get-Item $zipFile

        $MsiFile = $zipFile.fullname -replace ".zip", ".msi"
        UnZip-File –zipfilename $zipFile.fullname -destination $zipFile.DirectoryName
    }
	
	It "shuold install integrated security in SQL Server"{
		$zipFile = Copy-ZippedMsi
        $zipFile = Get-Item $zipFile
		$MsiFile = $zipFile.fullname -replace ".zip", ".msi"
        
        $BatchFile = $here + "\..\..\..\ccnet\SilentInstall\server\SilentInstall.bat"
      
		Install-TeleoptiCCCServer -BatchFile "$BatchFile" -MsiFile "$MsiFile" -machineConfig "PesterTest-DbSQL" -WinUser "" -WinPassword ""

	}
}

If ($InstallationAuthSetting -eq $Ntml) {
	#Installation is done with Windows+App login
	Describe "Check SDK is Windows enabled only" {
		It "SDK should be windows" {
			$enabled = Get-Authentication "TeleoptiCCC/SDK" "windowsAuthentication"
			$enabled | Should Be "True"
		}

		It "SDK should not be anonymous" {
			$enabled = Get-Authentication "TeleoptiCCC/SDK" "anonymousAuthentication"
			$enabled | Should Be "False"
		}
	}
}

If ($InstallationAuthSetting -eq $None) {

	#Installation is done with App login only
	Describe "Check SDK is Windows enabled only" {
		It "SDK should be windows" {
			$enabled = Get-Authentication "TeleoptiCCC/SDK" "windowsAuthentication"
			$enabled | Should Be "False"
		}

		It "SDK should not be anonymous" {
			$enabled = Get-Authentication "TeleoptiCCC/SDK" "anonymousAuthentication"
			$enabled | Should Be "True"
		}
	}
}