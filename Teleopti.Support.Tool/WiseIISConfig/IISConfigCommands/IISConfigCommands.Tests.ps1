$here = Split-Path -Parent $MyInvocation.MyCommand.Path
    $sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
    . "$here\$sut"

##============
#how to debug pester tests
##============
#0 run this file from command line (to get pester): C:\data\main\ccnet\pester\runAllTests.bat
#1 launch Windows Powershell ISE
#2 open all .ps1 file of interest *.ps1 + *Tests.ps1
#3 set a break point ("F9") in *.Tests.ps1 file
#4 from the "command line" in ISE:
#    Import-Module "C:\data\main\ccnet\pester\Pester.2.0.3\tools\Pester.psm1"
#    Invoke-Pester "C:\data\main\Teleopti.Support.Tool\WiseIISConfig\IISConfigCommands\"
#5 step step ("F10")
##============

#Add IIS admin module

Write-Host 'Put you break point here'

Load-SnapIn -ModuleName "WebAdministration"

$Ntml = "Ntlm"
$None = "None"
$InstallationAuthSetting = "Ntlm"
$CccServerMsiKey='{52613B22-2102-4BFB-AAFB-EF420F3A24B5}'
$displayName = "Teleopti CCC Server, version 7"
$workingFolder = "c:\temp\PesterTest"
$username = "tfsintegration"
$domain = "toptinet"
$password = "m8kemew0rk"
$secstr = New-Object -TypeName System.Security.SecureString
$password.ToCharArray() | ForEach-Object {$secstr.AppendChar($_)}
$cred = new-object -typename System.Management.Automation.PSCredential -argumentlist $domain\$username, $secstr
$computerName=(get-childitem -path env:computername).Value
$global:BaseURL = "http://" + $computerName + "/"
$global:zipFile
$global:MsiFile
$global:version = 'main'
$global:batName = 'PesterTest-DbSQL'
$global:Server = ''
$global:Db = ''
$global:resetToBaseline="False"
$global:insertedLicense=0

function Config-Load {
	Describe "Shold load config from Hebe "{
		[string] $serverConfigFile = '\\hebe\Installation\PBImsi\testservers.config'

        It "Should find the right version from the config file"{
        
            # initialize the xml object
            $serverConfig = New-Object XML
            # load the config file as an xml object
            $serverConfig.Load($serverConfigFile)
            # iterate over the settings
            foreach($testServer in $serverConfig.configuration.servers.add)
            {
                if ($testServer.name -eq  $computerName)
                {
                    $global:version =  $testServer.version
                    $global:batName =  $testServer.batname
                    $global:Server =  $testServer.DBServerInstance
                    $global:Db = $testServer.DB
                    $global:resetToBaseline = $testServer.resetToBaseline
                    
					if ($testServer.BaseURL)
					{
						$global:BaseURL = $testServer.BaseURL
					}
                }
                
            }
            Write-Host 'version: ' $global:version
            Write-Host 'restToBaseline: '$global:resetToBaseline
        }
    }
}


function TearDown {
	Describe "Tear down previous test"{
		[string] $path = Get-UninstallRegPath -MsiKey "$CccServerMsiKey"
		

        #stop system
    	It "should stop ETL Service" {
        $serviceName="TeleoptiETLService"
        Stop-MyService -ServiceName "$serviceName"
		Check-ServiceIsRunning "$serviceName" | Should Be $False
		}

		It "should stop Service Bus" {
        $serviceName="TeleoptiServiceBus"
        Stop-MyService -ServiceName "$serviceName"
		Check-ServiceIsRunning "$serviceName" | Should Be $False
		}
        
		It "should stop the SDK" {
			stop-AppPool -PoolName "Teleopti ASP.NET v4.0 SDK"
			$SDKUrl = $global:BaseURL + "TeleoptiCCC/SDK/TeleoptiCCCSdkService.svc"
			{Check-HttpStatus -url $SDKUrl -credentials $cred}  | Should Throw
		}

		It "should uninstall product"{
			[bool] $isInstalled = Check-ProductIsInstalled -DisplayName "$displayName"
			if ($isInstalled) {
				Uninstall-ByRegPath -path $path
			}

			$isInstalled = Check-ProductIsInstalled -DisplayName "$displayName"
			$isInstalled | Should Be $False
		}
        
        It "should have a default web site" {
			$computerName=(get-childitem -path env:computername).Value
			$httpStatus=Check-HttpStatus -url $global:BaseURL
			$httpStatus | Should Be $True
		}
			
		It "should throw exeption when http URL does not exist" {
			$computerName=(get-childitem -path env:computername).Value
			{Check-HttpStatus -url $global:BaseURL + "TeleoptiCCC/"}  | Should Throw
		}
		
		It "Should destroy working folder" {
			destroy-WorkingFolder -workingFolder "$workingFolder"
			Test-Path "$workingFolder" | Should Be $False
		}
	}
}

function Setup-PreReqs {
	Describe "Copy and Unzip the latest .zip file into local MSI"{

		It "Should create working folder" {
			create-WorkingFolder -workingFolder "$workingFolder"
			Test-Path "$workingFolder" | Should Be $True
		}
		
		It "should copy latest .zip-file from build server"{
			$global:zipFile = Copy-ZippedMsi -workingFolder "$workingFolder" -version "$global:version"
			Test-Path $global:zipFile | Should Be $True
		}

		It "should unzip file into MSI"{
			$zipFile = Get-Item $global:zipFile

			$global:MsiFile = $zipFile.fullname -replace ".zip", ".msi"
			UnZip-File -zipfilename $zipFile.fullname -destination $zipFile.DirectoryName
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

        It "should restore db to baseline if set in config" {
            if($global:resetToBaseline -eq "True")
                {
                    $spFile="$here\RestoreToBaseline.sql"
                    $spContent = [IO.File]::ReadAllText($spFile)
                    restoreToBaseline -computerName $computerName -spContent $spContent
                    #add Lic
                    Add-CccLicenseToDemo
                }
        }

        
        

		It "should install correct MSI from Hebe"{
			
			#add double quotes
			$global:MsiFile = '"' + $global:MsiFile + '"'
			
			$BatchFile = $here + "\..\..\..\ccnet\SilentInstall\server\SilentInstall.bat"
			
			[array]$ArgArray = @($MsiFile, $global:batName, "dummmyUser","dummmyPwd")
		  
			Install-TeleoptiCCCServer -BatchFile "$BatchFile" -ArgArray $ArgArray
			
		}
        It "should add license if not restore to Baseline"{
            if($global:resetToBaseline -eq "False")
            {
                #add Lic
                Add-CccLicenseToDemo
            }
        }
	}
}

function Test-SitesAndServicesOk {
	Describe "Run common test on services and web site config"{

        #start system
		It "should start SDK" {
			start-AppPool -PoolName "Teleopti ASP.NET v4.0 SDK"
			$SDKUrl = $global:BaseURL + "TeleoptiCCC/SDK/TeleoptiCCCSdkService.svc"
			$temp = Check-HttpStatus -url $SDKUrl -credentials $cred
			$temp | Should be $True
		}
		
		#something goes wrong with 32 vs. 64 bit implementation of management tools or IIS runtime
		# It "SDK should be windows" {
			# $enabled = Get-Authentication "/TeleoptiCCC/SDK" "windowsAuthentication"
			# $enabled | Should Be "True"
		# }

		# It "SDK should not be anonymous" {
			# $enabled = Get-Authentication "/TeleoptiCCC/SDK" "anonymousAuthentication"
			# $enabled | Should Be "False"
		# }

		#It "Nhib file should exist and contain SQL Auth connection string" {
		#	$nhibFile = "C:\Program Files (x86)\Teleopti\TeleoptiCCC\SDK\TeleoptiCCC7.nhib.xml"
		#	$computerName=(get-childitem -path env:computername).Value
		#	$connectionString="Data Source=$computerName;User Id=TeleoptiDemoUser;Password=TeleoptiDemoPwd2;initial Catalog=TeleoptiCCC7_Demo;Current Language=us_english"
		#	$nhibFile | Should Exist
		#	$nhibFile | Should Contain "$connectionString"
		#}
		
		It "should have a ETL Service running" {
        $serviceName="TeleoptiETLService"
        Start-MyService -ServiceName "$serviceName"
		Check-ServiceIsRunning "$serviceName" | Should Be $True
		}

		It "should have a Service Bus running" {
        $serviceName="TeleoptiServiceBus"
        Start-MyService -ServiceName "$serviceName"
		Check-ServiceIsRunning "$serviceName" | Should Be $True
		}
	}
}

function Add-CccLicenseToDemo
{
    if($global:Server -ne '')
    {
        It "should insert a new license" {
            #$LicFile="$here\..\..\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\License.xml"
            $LicFile="$here\..\..\..\LicenseFiles\Teleopti_RC.xml"
            $xmlString = [IO.File]::ReadAllText($LicFile)
            $InsertedLicense = insert-License -Server "$global:Server" -Db "$global:Db" -xmlString $xmlString
            $global:insertedLicense | Should Be 1
        }
    }
    else
    {
        $dir = Split-Path $MyInvocation.ScriptName
        $batchFile = "$dir\Add-CccLicenseToDemo.bat"
        & "$BatchFile"
    }
}

#Main
Config-Load
TearDown
Setup-PreReqs
Test-InstallationSQLLogin
Test-SitesAndServicesOk