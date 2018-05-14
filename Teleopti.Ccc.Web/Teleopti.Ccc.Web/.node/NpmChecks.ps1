# author: emil.sundin@teleopti.com

$preferedNodeVersion = [System.Version]::Parse("8.11.1")
$preferedNpmVersion = [System.Version]::Parse("5.6.0")

# Ensure npm & node exists
if (!(Get-Command "npm" -ErrorAction SilentlyContinue)) {
    throw "npm is not in path"
}
if (!(Get-Command "node" -ErrorAction SilentlyContinue)) {
    throw "node is not in path"
}
if (!(Get-Command "npx" -ErrorAction SilentlyContinue)) {
    throw "npx is not in path"
}

# Ensure acceptible version
$nodeVersion = [System.Version]::Parse((Get-Command "node").FileVersionInfo.FileVersion)
$npmVersion = [System.Version]::Parse((Invoke-Expression "npm -v"))
function semVer($v) {"{0}.{1}.{2}" -f $v.Major,$v.Minor,$v.Build}
if ($nodeVersion -lt $preferedNodeVersion) {
    Write-Warning ("node version {0} is too old" -f (semVer $nodeVersion))
}
if ($npmVersion -lt $preferedNpmVersion) {
    Write-Warning ("npm version {0} is too old" -f (semVer $npmVersion))
}
