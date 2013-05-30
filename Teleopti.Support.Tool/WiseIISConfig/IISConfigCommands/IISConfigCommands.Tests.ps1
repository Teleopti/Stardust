#how to debug
#1 launch Windows Powershell ISE
#2 open all .ps1 file of interest *.ps1 + *Tests.ps1
#3 from the "command line" in ISE:
# Import-Module "C:\data\main\ccnet\pester\Pester.2.0.3\tools\Pester.psm1"
# set a break point ("F9") in *.Tests.ps1 file
# Invoke-Pester "C:\data\main\Teleopti.Support.Tool\WiseIISConfig\IISConfigCommands\"
# step step ("F10")

$here = Split-Path -Parent $MyInvocation.MyCommand.Path
    $sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
    . "$here\$sut"

$Ntml = "Ntlm"
$None = "None"
$InstallationAuthSetting = "Ntlm"

#uninstall
Describe "Check if msi uninstalled"{
    It "Msi should be uninstalled"{
        $return= Uninstall 
        $return | Should Be 0  
    }
}


#Add IIS admin module
IISAdmin

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