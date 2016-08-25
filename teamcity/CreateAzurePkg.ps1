Framework "4.0"


properties {
	#$MSBuildPath = "C:\Program Files (x86)\MSBuild\14.0\bin\amd64\MSBuild.exe"
    $base_dir = resolve-path .\
    #$source_dir = "$base_dir\src"
    
	#TC Properties
    #Reading all TC Parametes, This is how you call them: $TCParams['build.number']
	$TCParams = ConvertFrom-StringData (Get-Content $env:TEAMCITY_BUILD_PROPERTIES_FILE -Raw)
	
	$WorkingDir = $TCParams['teamcity.build.workingDir']
	
	$TeleoptiBin = "$WorkingDir\TeleoptiCCC\bin"
	$ToBeArtifacted = "$WorkingDir\tobeartifact"
	$BinDependencies = "\\a380\T-Files\RnD\MSI_Dependencies\Azure_Net"
	$Dependencies = "\\a380\T-Files\RnD\MSI_Dependencies\ccc7_server"
	$InternalFileShare = "\\hebe\Installation\msi\$env:CccVersion"

	$AzureDependencies = "\\a380\T-Files\RnD\MSI_Dependencies\ccc7_azure"
	$AzurePackagePath = "$WorkingDir\TeleoptiWFM.cspkg"
	$AzurePackagePath_Large = "\TeleoptiWFM.cspkg\TeleoptiWFM_Large.cspkg"
	
	$IndexMSBuildFile = "$WorkingDir\teamcity\Azure\UpdateIndexHtml.msbuild"
	$CSPackEXE = $TCParams['AzureSDK_2.8_Path'] + "\bin\cspack.exe"
		
}

Set-ExecutionPolicy bypass -force
Import-module .\teamcity.psm1 -Force

TaskSetup {
    TeamCity-ReportBuildProgress "Running task $($psake.context.Peek().currentTaskName)"
}

task default -depends init, PreReq, CreateAzurePkg, PostReq

task Init {

#    delete_file $package_file
#    delete_directory $build_dir
#    create_directory $test_dir
#    create_directory $build_dir

}

task PreReq -depends init -description "Move/Copy preparation of files" {

	PrepareCopyFiles
	CorrectingURLinHTML
	
}

task CreateAzurePkg -depends Init, PreReq -description "Create Azure Package" {
    
	#Create Azure Pkg
	$Arg = @("Azure\ServiceDefinition.csdef", '/role:TeleoptiCCC;Azure\TeleoptiCCC', '/rolePropertiesFile:TeleoptiCCC;Azure\AzureRoleProperties.txt', "/out:$AzurePackagePath")
    & $CSPackEXE $Arg
	
	#Create Azure Pkg Large version
	$Arg = @('Azure\ServiceDefinition_Large.csdef', '/role:TeleoptiCCC;Azure\TeleoptiCCC', '/rolePropertiesFile:TeleoptiCCC;Azure\AzureRoleProperties.txt', "/out:$AzurePackagePath_Large")
	& $CSPackEXE $Arg
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
	Copy-Item -Path "$WorkingDir\teamcity\azure\TeleoptiCCC\bin" -Destination "$TeleoptiBin" -Force -ErrorAction Stop
	#Copy RegisterEventLogSource
	Copy-Item -Path "$Dependencies\RegisterEventLogSource.exe" -Destination "$TeleoptiBin" -Force -ErrorAction Stop
	#Copy azure dependencies
	Copy-Item -Path "$AzureDependencies" -Destination "$TeleoptiBin\ccc7_azure" -Recurse -Force -ErrorAction Stop
	#Copy azure .NET461
	Copy-Item -Path "$BinDependencies\" -Destination "$TeleoptiBin" -Recurse -Force -ErrorAction Stop
	
}

function global:CorrectingURLinHTML {

	#Add Msbuild to env path temporary
    $env:Path = $env:Path + ";C:\Program Files (x86)\MSBuild\14.0\bin\amd64"
	
	#Compile UpdateIndexHtml.msbuild
    
	exec { msbuild $IndexMSBuildFile /p:WorkingDirectory=$WorkingDir /toolsversion:14.0 }
	
}
