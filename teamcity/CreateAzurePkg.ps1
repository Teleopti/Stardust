Framework "4.0"


properties {
	#$MSBuildPath = "C:\Program Files (x86)\MSBuild\14.0\bin\amd64\MSBuild.exe"
    $base_dir = resolve-path .\
    #$source_dir = "$base_dir\src"
    
	#TC Properties
    #Reading all TC Parametes, This is how you call them: $TCParams['build.number']
	$TCParams = ConvertFrom-StringData (Get-Content $env:TEAMCITY_BUILD_PROPERTIES_FILE -Raw)
	#$CSPackEXE = $TCParams['AzureSDK_2.9_Path'] + "\bin\cspack.exe"
	
	$WorkingDir = $TCParams['teamcity.build.workingDir']
	
	$TeleoptiBin = "$WorkingDir\TeleoptiCCC\bin"
	$ToBeArtifacted = "$WorkingDir\tobeartifact"
	$BinDependencies = "\\a380\T-Files\RnD\MSI_Dependencies"
	$Dependencies = "\\a380\T-Files\RnD\MSI_Dependencies\ccc7_server"
	$InternalFileShare = "\\hebe\Installation\msi\$env:CccVersion"

	$AzureDependencies = "\\a380\T-Files\RnD\MSI_Dependencies\ccc7_azure"
	$AzurePackagePath_Medium = "$WorkingDir\TeleoptiWFM_Medium.cspkg"
	$AzurePackagePath_Large = "$WorkingDir\TeleoptiWFM_Large.cspkg"
	
	$IndexMSBuildFile = "$WorkingDir\StartPage\index.html"
	$ForecastHtmlFile = "$WorkingDir\StartPage\forecast.html"
	$CSPackEXE = "$env:AzureSDK_2_9_Path\bin\cspack.exe"
	#
}

Set-ExecutionPolicy bypass -force
Import-module .\teamcity.psm1 -Force

TaskSetup {
    TeamCity-ReportBuildProgress "Running task $($psake.context.Peek().currentTaskName)"
}

task default -depends init, PreReq, CreateAzurePkg, PostReq

task Init {

#    delete_file $package_file dummy
#    delete_directory $build_dir
#    create_directory $test_dir
#    create_directory $build_dir

}

task PreReq -depends init -description "Move/Copy preparation of files" {
	
	Write-Output "##teamcity[blockOpened name='<PreReq>']"
    
	PrepareCopyFiles
	CorrectingURLinHTML
	
	Write-Output "##teamcity[blockClosed name='<PreReq>']"
}

task CreateAzurePkg -depends Init, PreReq -description "Create Azure Package" {
    
	Write-Output "##teamcity[blockOpened name='<CreateAzurePkg>']"
	
	workflow parallelAzurePackaging {
		param(
		$CSPackEXE,
		$AzurePackagePath_Medium,
		$AzurePackagePath_Large,
		$WorkingDir
			)
		parallel {
			#Create Azure pkg
			InlineScript {& $Using:CSPackEXE "$Using:WorkingDir\teamcity\Azure\ServiceDefinition_Medium.csdef" `
			  "/role:TeleoptiCCC;$Using:WorkingDir\TeleoptiCCC" `
			  "/rolePropertiesFile:TeleoptiCCC;$Using:WorkingDir\teamcity\Azure\AzureRoleProperties.txt" `
			  "/out:$Using:AzurePackagePath_Medium"}
			
			#Create Azure Large pkg
			InlineScript {& $Using:CSPackEXE "$Using:WorkingDir\teamcity\Azure\ServiceDefinition_Large.csdef" `
			  "/role:TeleoptiCCC;$Using:WorkingDir\TeleoptiCCC" `
			  "/rolePropertiesFile:TeleoptiCCC;$Using:WorkingDir\teamcity\Azure\AzureRoleProperties.txt" `
			  "/out:$Using:AzurePackagePath_Large"}
		}
	}
	
	parallelAzurePackaging -CSPackEXE $CSPackEXE -AzurePackagePath_Medium $AzurePackagePath_Medium -AzurePackagePath_Large $AzurePackagePath_Large -WorkingDir $WorkingDir
	
	Write-Output "##teamcity[blockClosed name='<CreateAzurePkg>']"
}

task PostReq -depends Init, PreReq, CreateAzurePkg -description "PostReq steps" {
	
	Write-Output "##teamcity[blockOpened name='<PostReq>']"
	
	Copy-Item -Path "$WorkingDir\teamcity\Azure\Customer\" -Destination "$ToBeArtifacted" -Recurse -Force -ErrorAction Stop
	Copy-Item -Path "$AzurePackagePath_Medium" -Destination "$ToBeArtifacted\Azure-$env:CccVersion-Medium.cspkg" -Force -ErrorAction Stop
	Copy-Item -Path "$AzurePackagePath_Large" -Destination "$ToBeArtifacted\Azure-$env:CccVersion-Large.cspkg" -Force -ErrorAction Stop
	
	Write-Output "##teamcity[blockClosed name='<PostReq>']"
}

function global:PrepareCopyFiles {
	
	#Create bin folder
	New-Item "$TeleoptiBin\ccc7_azure" -Itemtype Directory -Force | Out-Null
	#Copy azure bin folder
	Copy-Item -Path "$WorkingDir\teamcity\azure\TeleoptiCCC\bin\*" -Destination "$TeleoptiBin" -verbose -Recurse -Force -ErrorAction Stop
	#Copy RegisterEventLogSource
	Copy-Item -Path "$Dependencies\RegisterEventLogSource.exe" -Destination "$TeleoptiBin" -verbose -Recurse -Force -ErrorAction Stop
	#Copy azure dependencies
	Copy-Item -Path "$AzureDependencies" -Destination "$TeleoptiBin" -verbose -Recurse -Force -ErrorAction Stop
	#Copy azure .NET461
	Copy-Item -Path "$BinDependencies\Azure_Net\*" -Destination "$TeleoptiBin" -verbose -Recurse -Force -ErrorAction Stop
	
}

function global:CorrectingURLinHTML {
	
	## Obsolete solution ##
	#Add Msbuild to env path temporary
    #$env:Path = $env:Path + ";C:\Program Files (x86)\MSBuild\14.0\bin\amd64"
	#Compile UpdateIndexHtml.msbuild
    #exec { msbuild $IndexMSBuildFile /p:WorkingDirectory=$WorkingDir /toolsversion:14.0 }
	$word = "/TeleoptiWFM/"
	$replacement = "/"
	
	$path = $IndexMSBuildFile
	$text = get-content $path 
	Write-Host "Updating $IndexMSBuildFile..."
	Write-Host "Replacing: $word  with: $replacement"
	$newText = $text -replace $word,$replacement | Out-File $Path -Encoding UTF8
	
	$path = $ForecastHtmlFile
	$text = get-content $path
	Write-Host "Updating $ForecastHtmlFile..."
	Write-Host "Replacing: $word  with: $replacement"
	$newText = $text -replace $word,$replacement | Out-File $Path -Encoding UTF8
}
