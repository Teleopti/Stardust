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

#Add IIS admin module
Load-SnapIn -ModuleName "WebAdministration"

$Ntml = "Ntlm"
$None = "None"
$InstallationAuthSetting = "Ntlm"
$CccServerMsiKey='{52613B22-2102-4BFB-AAFB-EF420F3A24B5}'

function TearDown {
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
}

function Setup-PreReqs {
	Describe "Copy and Unzip the latest .zip file into local MSI"{  
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
		
		It "should create a local group in Windows"{
			$groupName = "TeleoptiCCC_Users"
			Create-LocalGroup -groupName "$groupName"
			[ADSI]::Exists("WinNT://./$groupName") | Should be $True
		}
		
		It "should add domain user to a local group"{
			$userDomain = "Toptinet"
			$username = "TfsIntegration"
            $groupName = "TeleoptiCCC_Users"

			Add-UserToLocalGroup -groupName "$groupName" -userdomain "$userDomain" -userName "$username"
            
            $strcomputer = [ADSI]("WinNT://.,computer")
            $Group = $strcomputer.psbase.children.find("$groupName")
            $members= $Group.psbase.invoke("Members") | %{$_.GetType().InvokeMember("Name", 'GetProperty', $null, $_, $null)}

			$members -contains $username | Should Be $True
		}
	}
}

function Test-InstallationSQLLogin {
	Describe "Installation test - SQL DB Login"{  

		It "Should install integrated security in SQL Server"{
			$zipFile = Copy-ZippedMsi
			$zipFile = Get-Item $zipFile
			$MsiFile = $zipFile.fullname -replace ".zip", ".msi"
			
			$BatchFile = $here + "\..\..\..\ccnet\SilentInstall\server\SilentInstall.bat"
		  
			Install-TeleoptiCCCServer -BatchFile "$BatchFile" -MsiFile "$MsiFile" -machineConfig "PesterTest-DbSQL" -WinUser "" -WinPassword ""

		}
		
		It "SDK should be windows" {
			$enabled = Get-Authentication "TeleoptiCCC/SDK" "windowsAuthentication"
			$enabled | Should Be "True"
		}

		It "SDK should not be anonymous" {
			$enabled = Get-Authentication "TeleoptiCCC/SDK" "anonymousAuthentication"
			$enabled | Should Be "False"
		}
		
		It "Nhib file should exist and contain SQL Auth connection string" {
			$nhibFile = "C:\Program Files (x86)\Teleopti\TeleoptiCCC\SDK\TeleoptiCCC7.nhib.xml"
			$computerName=(get-childitem -path env:computername).Value
			$connectionString="Data Source=$computerName;User Id=TeleoptiDemoUser;Password=TeleoptiDemoPwd2;initial Catalog=TeleoptiCCC7_Demo;Current Language=us_english"
			$nhibFile | Should Exist
			$nhibFile | Should Contain "$connectionString"
		}
		
		Add-CccLicenseToDemo
		
		It "should stop system" {
			Stop-TeleoptiCCC
			
			Check-ServiceIsRunning "TeleoptiETLService" | Should Be $False
			Check-ServiceIsRunning "TeleoptiServiceBus" | Should Be $False
			$computerName=(get-childitem -path env:computername).Value
			{Check-HttpStatus -url "ttp://$computerName/TeleoptiCCC/SDK/TeleoptiCCCSdkService.svc"}  | Should Throw
		}

		It "should start system" {
			Start-TeleoptiCCC
		}
	
		It "should have a working SDK" {
			$computerName=(get-childitem -path env:computername).Value
			{Check-HttpStatus -url "http://$computerName/TeleoptiCCC/SDK/TeleoptiCCCSdkService.svc"}  | Should be $True
		}
		
		It "should have a ETL Service running" {
		Check-ServiceIsRunning "TeleoptiETLService" | Should Be $True
		}

		It "should have a Service Bus running" {
		Check-ServiceIsRunning "TeleoptiServiceBus" | Should Be $True
		}
	}
}

function Test-InstallationWinAuth {

	Describe "Installation test - Win Integrated Login"{ 

		It "Should install integrated security in SQL Server"{
			$zipFile = Copy-ZippedMsi
			$zipFile = Get-Item $zipFile
			$MsiFile = $zipFile.fullname -replace ".zip", ".msi"
			
			$BatchFile = $here + "\..\..\..\ccnet\SilentInstall\server\SilentInstall.bat"
		  
			Install-TeleoptiCCCServer -BatchFile "$BatchFile" -MsiFile "$MsiFile" -machineConfig "PesterTest-DbIntegrated" -WinUser "toptinet\tfsintegration" -WinPassword "m8kemew0rk"
		}
		
		It "SDK should be windows" {
			$enabled = Get-Authentication "TeleoptiCCC/SDK" "windowsAuthentication"
			$enabled | Should Be "True"
		}

		It "SDK should not be anonymous" {
			$enabled = Get-Authentication "TeleoptiCCC/SDK" "anonymousAuthentication"
			$enabled | Should Be "False"
		}
		
		It "Nhib file should exist and contain Win Auth connection string" {
			$nhibFile = "C:\Program Files (x86)\Teleopti\TeleoptiCCC\SDK\TeleoptiCCC7.nhib.xml"
			$computerName=(get-childitem -path env:computername).Value
			$connectionString="Data Source=$computerName;Integrated Security=SSPI;initial Catalog=TeleoptiCCC7_Demo;Current Language=us_english"
			$nhibFile | Should Exist
			$nhibFile | Should Contain "$connectionString"
		}
		
		Add-CccLicenseToDemo
	}
}

#Main	
TearDown
Setup-PreReqs
Test-InstallationSQLLogin
TearDown