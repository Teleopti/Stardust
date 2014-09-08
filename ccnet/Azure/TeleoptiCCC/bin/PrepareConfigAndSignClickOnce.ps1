param([string]$ScriptPath)
write-host $ScriptPath
powershell -File "$ScriptPath" 2>&1 | more