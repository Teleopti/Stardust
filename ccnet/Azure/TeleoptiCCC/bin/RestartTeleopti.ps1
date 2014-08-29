param([string]$ScriptPath)
powershell -File "$ScriptPath" 2>&1 | more