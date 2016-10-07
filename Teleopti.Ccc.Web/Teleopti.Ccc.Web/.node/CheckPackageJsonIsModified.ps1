$file = "../WFM/Package.json"
$fileExists = Test-Path PackageJsonHash.txt
$isModified = "False"

if ($fileExists){
$checkModulelFolders = Test-Path ..\WFM\node_modules
    if($checkModulelFolders -ne $True){
        $isModified = "True"
        return $isModified
        exit
    }
    $latestKnownHash = get-content -Path ./PackageJsonHash.txt
        $currentHash = Get-FileHash $file | Format-List
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


