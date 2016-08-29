$baseUri = "%OctopusServerUrl%" # <-- Update this to the base URL to your Octopus server(i.e. not including 'app' or 'api'
$apiKey = "%OctopusApiKey%" # <-- Update this to your API key
$headers = @{"X-Octopus-ApiKey" = $apiKey}
$libraryVariableSetId = "%OctopusLibraryVariableSet%" # <-- Update this to the Id of your variable set
$projectName = "teleopti-wfm-server" # <-- Update this to the name of your project

function Get-OctopusResource([string]$uri) {
    Write-Host "[GET]: $uri"
    return Invoke-RestMethod -Method Get -Uri "$baseUri/$uri" -Headers $headers
}

function Put-OctopusResource([string]$uri, [object]$resource) {
    Write-Host "[PUT]: $uri"
    #Write-Output $resource | ConvertTo-Json -Depth 10
    Invoke-RestMethod -Method Put -Uri "$baseUri/$uri" -Body $($resource | ConvertTo-Json -Depth 10) -Headers $headers
}

$project = Get-OctopusResource "api/projects/$projectName"
Write-Output "Current included variables set '$($project.IncludedLibraryVariableSetIds)'"

$project.IncludedLibraryVariableSetIds = $project.IncludedLibraryVariableSetIds + @($libraryVariableSetId)
Write-Output "Changed included variables set '$($project.IncludedLibraryVariableSetIds)'"

Put-OctopusResource "api/projects/$($project.Id)" $project