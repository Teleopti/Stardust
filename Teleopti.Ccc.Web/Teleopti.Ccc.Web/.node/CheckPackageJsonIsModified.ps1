$file = "../WFM/Package.json"
$hashExists = Test-Path PackageJsonHash.txt
$checkModulelFolders = (Test-Path ..\WFM\node_modules) -and (Test-Path ..\..\..\packages\NodeEnv.*\node_modules)
$isModified = "False"

if ($hashExists){
    $latestKnownHash = get-content -Path ./PackageJsonHash.txt
    if($checkModulelFolders -ne $True){
        $isModified = "True"
        return $isModified
        exit
    }
    if($latestKnownHash -ne (Get-FileHash $file).hash){
        $hash = (Get-FileHash $file).hash | Format-List | Out-file PackageJsonHash.txt
        $isModified = "True"
        return $isModified
        exit
    }else{
        return $isModified
        exit
    }
}else{
    $hash = (Get-FileHash $file).hash | Format-List | Out-file PackageJsonHash.txt
    $isModified = "True"
    return $isModified
}


