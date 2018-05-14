function checkFiles ([String]$CurrentHash, [String]$LastHash) {
    if ($CurrentHash -ne $LastHash) {
        return "isChanged"
    }
    return "isNotChanged"
}

function getHashOfFile ([String]$Filepath) {
    return (Get-FileHash $Filepath).hash
}

function readHashFromFile ([String]$Filepath){
    return get-content -Path $Filepath
}

function renewHashFile ([String]$FilePath, [String]$HashFilePath) {
    return (Get-FileHash $FilePath).hash | Format-List | Out-file $HashFilePath
}

function checkNodeModules ([String]$FilePath) {
    if(Test-Path $FilePath) {
        return $True
    } else {
        return $False
    }
}

function renewCheck([String]$FilePath, [String]$HashFilePath) {
    $Status = "isChanged"
    if (Test-Path $HashFilePath) {
        $CurrentHash = getHashOfFile $FilePath 
        $LastHash = readHashFromFile $HashFilePath
        $Status = checkFiles $CurrentHash $LastHash
    }
    renewHashFile $FilePath $HashFilePath
    return $Status;
} 

$CheckWfm = renewCheck "..\WFM\Package.json" ".\WfmFileHash.txt"
$CheckNodeEnv = renewCheck "..\..\..\packages\NodeEnv.1.0.6\node.exe" ".\NodeHash.txt"
$CheckWfmNodeMoudle = checkNodeModules "..\WFM\node_modules"
$CheckNodeEnvNodeModule = checkNodeModules "..\..\..\packages\NodeEnv.1.0.6\node_modules"

if (($CheckNodeEnv -eq "isChanged") -or ($CheckNodeEnvNodeModule -eq $False)) {
    return "ALL"
} elseIf (($CheckWfm -eq "isChanged") -or ($CheckWfmNodeMoudle -eq $False)) {
    return "WFM"
} else {
    return "False"
}


