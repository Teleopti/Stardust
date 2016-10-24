Framework "4.0"


properties {
	#$MSBuildPath = "C:\Program Files (x86)\MSBuild\14.0\bin\amd64\MSBuild.exe"
    $base_dir = resolve-path .\
    #$source_dir = "$base_dir\src"
    
	#TC Properties
    #Reading all TC Parametes, This is how you call them: $TCParams['build.number']
	$TCParams = ConvertFrom-StringData (Get-Content $env:TEAMCITY_BUILD_PROPERTIES_FILE -Raw)
	#$CSPackEXE = $TCParams['AzureSDK_2.8_Path'] + "\bin\cspack.exe"
	
	$WorkingDir = $TCParams['teamcity.build.workingDir']
	
	$TeleoptiBin = "$WorkingDir\TeleoptiCCC\bin"
	$ToBeArtifacted = "$WorkingDir\tobeartifact"
	$BinDependencies = "\\a380\T-Files\RnD\MSI_Dependencies"
	$Dependencies = "\\a380\T-Files\RnD\MSI_Dependencies\ccc7_server"
	$InternalFileShare = "\\hebe\Installation\msi\$env:CccVersion"

	$AzureDependencies = "\\a380\T-Files\RnD\MSI_Dependencies\ccc7_azure"
	$AzurePackagePath = "$WorkingDir\TeleoptiWFM.cspkg"
	$AzurePackagePath_Large = "$WorkingDir\TeleoptiWFM_Large.cspkg"
	
	$IndexMSBuildFile = "$WorkingDir\StartPage\index.html"
	$CSPackEXE = "$env:AzureSDK_2_8_Path\bin\cspack.exe"
	
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
	
	TeamCity-Block  "PreReq" { 
    
		PrepareCopyFiles
		CorrectingURLinHTML
    }    

	
}

task CreateAzurePkg -depends Init, PreReq -description "Create Azure Package" {
    workflow parallelAzurePackaging {
		param(
		$CSPackEXE,
		$AzurePackagePath,
		$AzurePackagePath_Large,
		$WorkingDir
			)
		parallel {
			#Create Azure pkg
			InlineScript {& $Using:CSPackEXE "$Using:WorkingDir\teamcity\Azure\ServiceDefinition.csdef" `
			  "/role:TeleoptiCCC;$Using:WorkingDir\TeleoptiCCC" `
			  "/rolePropertiesFile:TeleoptiCCC;$Using:WorkingDir\teamcity\Azure\AzureRoleProperties.txt" `
			  "/out:$Using:AzurePackagePath"}
			
			#Create Azure Large pkg
			InlineScript {& $Using:CSPackEXE "$Using:WorkingDir\teamcity\Azure\ServiceDefinition_Large.csdef" `
			  "/role:TeleoptiCCC;$Using:WorkingDir\TeleoptiCCC" `
			  "/rolePropertiesFile:TeleoptiCCC;$Using:WorkingDir\teamcity\Azure\AzureRoleProperties.txt" `
			  "/out:$Using:AzurePackagePath_Large"}
		}
	}
	
	parallelAzurePackaging -CSPackEXE $CSPackEXE -AzurePackagePath $AzurePackagePath -AzurePackagePath_Large $AzurePackagePath_Large -WorkingDir $WorkingDir

}

task PostReq -depends Init, PreReq, CreateAzurePkg -description "PostReq steps" {

	Copy-Item -Path "$WorkingDir\teamcity\Azure\Customer\" -Destination "$ToBeArtifacted" -Recurse -Force -ErrorAction Stop
	Copy-Item -Path "$AzurePackagePath" -Destination "$ToBeArtifacted\Azure-$env:CccVersion.cspkg" -Force -ErrorAction Stop
	Copy-Item -Path "$AzurePackagePath_Large" -Destination "$ToBeArtifacted\Azure-$env:CccVersion-Large.cspkg" -Force -ErrorAction Stop
	
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
	
	$path = $IndexMSBuildFile
	$word = "/TeleoptiWFM/"
	$replacement = "/"
	$text = get-content $path 
	
	Write-Host "Updating $IndexMSBuildFile..."
	Write-Host "Replacing: $word  with: $replacement"
	
	$newText = $text -replace $word,$replacement | Out-File $Path -Encoding UTF8
		
}
