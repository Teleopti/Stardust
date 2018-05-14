$relPath = Join-Path (Join-Path (pwd) .) '..\..\..\packages\nodeenv.1.0.7\'
$absPath = [System.IO.Path]::GetFullPath($relPath)

# Add npm folder to path
$env:Path = "$env:APPDATA\npm;$env:Path"

# Add NodeEnv folder to path
$env:Path = "$absPath;$env:Path"
