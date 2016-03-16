Framework "4.0"

properties {
	#$MSBuildPath = "C:\Program Files (x86)\MSBuild\12.0\bin\amd64\MSBuild.exe"
    $base_dir = resolve-path .\
    #$source_dir = "$base_dir\src"
    
    #TC Properties
    $BuildVCSNumber = "$env:BUILD_VCS_NUMBER"
    $DEPENDENCIESSRC = "$env:DEPENDENCIESSRC"
    $MountKDirectory = "$env:MountKDirectory"
    $ProductVersion = "$env:CccVersion"
    $SourceDir = "$MountKDirectory\src"
   	$DYNAMICCONTENT = "$SourceDir\WiseArtifact"
    $ArtifactDir = "$SourceDir\BuildArtifacts"
    $OutDir = "$MountKDirectory\out"
    
    $ClamWinTool = "$env:ClamWinTool"
    $ClamWinDb = "$env:ClamWinDb"
    
	$OutputPath = "$MountKDirectory\SdkDocOutput"
    $SdkHostPath = "$SourceDir\SDK\bin"
    $SdkFile = "$MountKDirectory\Teamcity\SdkDoc\docSdkx64.shfbproj" 
}

Include .\teamcity.psm1

TaskSetup {
    TeamCity-ReportBuildProgress "Running task $($psake.context.Peek().currentTaskName)"
}

task default -depends init, PreReq, MountK, CompileWse, CompileWsi, ProductVersion, PostReq, UnMountK, CHM-SDK-File, MalewareScan

task Init {

#    delete_file $package_file
#    delete_directory $build_dir
#    create_directory $test_dir
#    create_directory $build_dir

}

task MountK -description "Mount working directory to K:" {
	
	Invoke-Expression "Subst K: /D" -ErrorAction SilentlyContinue
    Invoke-Expression "Subst K: $MountKDirectory"
	
	Write-Host "Mount $MountKDirectory to K:"
}

task PreReq -depends init -description "Move/Copy preparation of files" {

    MoveWiseFiles
    CopyDependencies
    CopyWiseArtifacts
}

task CompileWse -depends Init, PreReq, MountK -description "Complie all WSE files to EXE" {
    
    #Locate WSE files
    $WseFiles = Get-ChildItem $SourceDir -Recurse -Include *.wse
    #Path to EXE
    $WseCompiler = "C:\Program Files (x86)\Altiris\Wise\WiseScript Package Editor\Wise32.exe"

    foreach ($Files in $WseFiles) {
    $Command = $WseCompiler
    $Arg = @($Files.FullName, '/c')
    Write-Output "$Command $Arg"
    & $Command $Arg | Out-Null
    }
}

task CompileWsi -depends Init, PreReq, MountK -description "Complie all WSI files to MSI" {

    #Locate WSI files (Exclude Forecast)
    $WsiFiles = Get-ChildItem $SourceDir -Recurse -Include *.wsi -Exclude ccc7_forecast.wsi
    #Path to EXE
    $WsiCompiler = "C:\Program Files (x86)\Altiris\Wise\Windows Installer Editor\WfWI.exe"

    foreach ($Files in $WsiFiles) {
    $Command = $WsiCompiler
    $Arg = @($Files.FullName, '/c')
    Write-Output "$Command $Arg"
    & $Command $Arg | Out-Null
    }
}

task ProductVersion -depends CompileWse, CompileWsi -description "Sets the current version number in MSI" {
    
	Set-ProductVersion "$SourceDir\Wise\ccc7_client\ccc7_client.msi" "$ProductVersion"
    Set-ProductVersion "$SourceDir\Wise\ccc7_server\ccc7_server.msi" "$ProductVersion"
}

task PostReq -depends CompileWse, CompileWsi, ProductVersion -description "Tasks that needs to be performed after MSI creation" {

    CopyFilesToOutput

}

task CHM-SDK-File -depends CompileWse, CompileWsi -description "Create chm sdk file" {

    #Add Msbuild to env path temporary
    $env:Path = $env:Path + ";C:\Program Files (x86)\MSBuild\12.0\bin\amd64"
    
    #Compile docSdkx64.shfbproj
    exec { msbuild $SdkFile /p:WorkingDirectory=$MountKDirectory /p:SdkHostPath=$SdkHostPath /p:OutputPath=$OutputPath }

}

task MalewareScan -depends CompileWse, CompileWsi -description "Proves our MSI is clean" {

    & $ClamWinTool --database="$ClamWinDb" $OurDir\ > "$OutDir\MalwareScan.log"

}

task UnMountK {
    
    Invoke-Expression "Subst K: /D" -ErrorAction Continue
}

function global:Set-ProductVersion($FilePath,$ProductVersion) {
    
    Write-Host "This Version number will be set: $ProductVersion"

    # Add Required WiX Type Libraries
    Add-Type -Path "C:\Program Files (x86)\WiX Toolset v4.0\bin\WixToolset.Dtf.WindowsInstaller.dll";

    # Open MSI Database
    $OpenMSIDB = New-Object WixToolset.Dtf.WindowsInstaller.Database("$FilePath", [WixToolset.Dtf.WindowsInstaller.DatabaseOpenMode]::Direct);
 
    #Create a Select Query against ProductVersion property
    $SQLQuery = "SELECT * FROM Property WHERE Property= 'ProductVersion'"
 
    #Create and Execute a View object
    [WixToolset.Dtf.WindowsInstaller.View]$oView = $OpenMSIDB.OpenView($SQLQuery)
    $oView.Execute()
 
    #Fetch Result
    $oRecord = $oView.Fetch()
    $sProductVersion = $oRecord.GetString(2)
 
    #Display current ProductVersion Field
    Write-Host "This is the old Version number:"
    "ProductVersion = $($sProductVersion)"
 
    #Set new ProductVersion nr
    $oRecord.SetString("Value","$ProductVersion")
    $oView.Modify([WixToolset.Dtf.WindowsInstaller.ViewModifyMode]::Update,$oRecord)

    #Close Database.
    $oView.Close();
    $OpenMSIDB.Dispose();
}

function global:MoveWiseFiles {
  
    Move-Item -path "$MountKDirectory\ccc7_server*" -Destination "$SourceDir\Wise\ccc7_server\" -Force -ErrorAction Stop
    Move-Item -path "$MountKDirectory\ccc7_forecast*" -Destination "$SourceDir\Wise\ccc7_forecast\" -Force -ErrorAction Stop
    Move-Item -Path "$MountKDirectory\ccc7_client.wsi*" -Destination "$SourceDir\Wise\ccc7_client\" -Force -ErrorAction Stop
    
}

function global:CopyDependencies {

    Copy-Item -Path "$DEPENDENCIESSRC\ccc7_forecast\SQLEXPR_x86_ENU.EXE" -Destination "$SourceDir\Wise\ccc7_forecast\SQL2008R2\" -Force -ErrorAction Continue
    Copy-Item -Path "$DEPENDENCIESSRC\ccc7_forecast\ForecastDatabase\TeleoptiCCC_Forecasts.BAK" -Destination "$SourceDir\ForecastDatabase\" -Force -ErrorAction Continue

    Copy-Item -Path "$DEPENDENCIESSRC\ccc7_server\DemoDatabase\TeleoptiAnalytics_Demo.bak" -Destination "$SourceDir\DemoDatabase\" -Force -ErrorAction Continue
    Copy-Item -Path "$DEPENDENCIESSRC\ccc7_server\DemoDatabase\TeleoptiCCC7_Demo.bak" -Destination "$SourceDir\DemoDatabase\" -Force -ErrorAction Continue
    Copy-Item -Path "$DEPENDENCIESSRC\ccc7_server\DemoDatabase\TeleoptiCCC7Agg_Demo.BAK" -Destination "$SourceDir\DemoDatabase\" -Force -ErrorAction Continue
    Copy-Item -Path "$DEPENDENCIESSRC\ccc7_server\RegisterEventLogSource.exe" -Destination "$SourceDir\Wise\ccc7_server\Logs\" -Force -ErrorAction Continue
    Copy-Item -Path "$DEPENDENCIESSRC\ccc7_server\ntrights.exe" -Destination "$SourceDir\Wise\ccc7_server\Logs\" -Force -ErrorAction Continue
    Copy-Item -Path "$DEPENDENCIESSRC\ccc7_server\ntrights.exe" -Destination "$SourceDir\Wise\ccc7_server\" -Force -ErrorAction Continue
    Copy-Item -Path "$DEPENDENCIESSRC\ccc7_server\sqlio.exe" -Destination "$SourceDir\SupportTools\SQLServerPerformance\SQLIO\" -Force -ErrorAction Continue

    Copy-Item -Path "$DEPENDENCIESSRC\images\*.*" -Destination "$SourceDir\images\" -Force -ErrorAction Continue

}

function global:CopyWiseArtifacts {

    New-Item "$DYNAMICCONTENT" -Itemtype Directory -Force | Out-Null
    New-Item "$DYNAMICCONTENT\Client" -Itemtype Directory -Force | Out-Null
    New-Item "$DYNAMICCONTENT\Client\Forecasts" -Itemtype Directory -Force | Out-Null
    New-Item "$DYNAMICCONTENT\Client\StandAlone" -Itemtype Directory -Force | Out-Null

    Remove-Item -Path "$SourceDir\Client\StandAlone\Teleopti.Ccc.SmartClientPortal.Shell.exe.config" -Force -ErrorAction SilentlyContinue
    Copy-Item -Path "$ArtifactDir\AppRaptor.config" -Destination "$DYNAMICCONTENT\Client\StandAlone\Teleopti.Ccc.SmartClientPortal.Shell.exe.config" -Force -ErrorAction Stop

    Remove-Item -Path "$SourceDir\Client\Forecasts\Teleopti.Ccc.SmartClientPortal.Shell.exe.config" -Force -ErrorAction SilentlyContinue
    Copy-Item -Path "$ArtifactDir\AppForecasts.config" -Destination "$DYNAMICCONTENT\Client\Forecasts\Teleopti.Ccc.SmartClientPortal.Shell.exe.config" -Force -ErrorAction Stop

    #Non Dynamic Artifacts
    Copy-Item "$ArtifactDir\licensecontext.slf" -Destination "$SourceDir\Client\Forecasts\licensecontext.slf" -Force -ErrorAction Stop
    Copy-Item "$ArtifactDir\licensecontext.slf" -Destination "$SourceDir\Client\StandAlone\licensecontext.slf" -Force -ErrorAction Stop
    Copy-Item "$ArtifactDir\forecastconfig.json" -Destination "$SourceDir\Client\Forecasts\licensecontext.slf" -Force -ErrorAction Stop

}

function global:CopyFilesToOutput {

    New-Item "$OutDir" -Itemtype Directory -Force | Out-Null
    
    Copy-Item -Path "$SourceDir\Wise\ccc7_client\ccc7_client.msi" -Destination "$OutDir\Teleopti WFM Client $ProductVersion.msi" -Force -ErrorAction Stop
    Copy-Item -Path "$SourceDir\Wise\ccc7_server\ccc7_server.msi" -Destination "$OutDir\Teleopti WFM $ProductVersion.msi" -Force -ErrorAction Stop
   #Copy-Item -Path "$SourceDir\Wise\ccc7_forecast\ccc7_forecast.exe" -Destination "$OutDir\Teleopti WFM Forecasts $CccVersion.msi" -Force -ErrorAction Continue

    #Copy UnInstall bat to Output folder
    Copy-Item -Path "$SourceDir\Wise\Uninstall\Uninstall Teleopti WFM server.bat" -Destination "$OutDir\Uninstall Teleopti WFM server.bat" -Force -ErrorAction Stop

    #Copy PreReq tool to Output folder
    Copy-Item -Path "$MountKDirectory\PreReqsCheck\CheckPreRequisites.exe" -Destination "$OutDir\CheckPreRequisites.exe" -Force -ErrorAction stop

    #Create lastchangeset.txt file for Output directory
   
       New-Item $OutDir\lastchangeset.txt -type file -force -value "$BuildVCSNumber" | Out-Null

}