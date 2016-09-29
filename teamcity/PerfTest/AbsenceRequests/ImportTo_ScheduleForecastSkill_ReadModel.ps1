param( [Parameter(Mandatory=$true)][string]$server)

invoke-sqlcmd -inputfile "$PSScriptRoot/Import_ScheduleForecastSkill_ReadModel.sql" -serverinstance "$server" -database "PerfA" 