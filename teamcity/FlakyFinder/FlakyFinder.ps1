Param(

    [String]$BuildType,        #Build configuration ID, ex TeleoptiWFM_WebScenarioAll
    [String]$FlakyProjectName  #Build configuration ID, ex TeleoptiWFM_FlakyWebAll

)

#Properties
$username = "toptinet\tfsintegration"
$password = "m8kemew0rk"
$server = "http://buildsrv01"
$buildTypeMain = "TeleoptiWFM_WfmMain"
$secpasswd = ConvertTo-SecureString "m8kemew0rk" -AsPlainText -Force
$cred = New-Object System.Management.Automation.PSCredential($username,$secpasswd)


function Get-LatestBuildNumberFromTC() {
    param(
        [string] $server,
        [string] $buildType,
        [string] $username,
        [string] $password
    )

    $client = New-Object System.Net.WebClient
    $client.Credentials = New-Object System.Net.NetworkCredential $username, $password

    $latestBuildUrl = [System.String]::Format("{0}/httpAuth/app/rest/builds/?locator=buildType:{1},count:1,status:SUCCESS", $server, $buildType);
    [xml]$latestBuild = $client.DownloadString($latestBuildUrl)
    $latestBuildNumber = $latestBuild.builds.build.number
    
    [string] $Script:BuildNumber = $latestBuildNumber.split('-')[1].split(' ');

    return $Script:BuildNumber

}


function Get-DependentBuildFromMain() {
    param(
        [string] $server,
        [string] $buildTypeMain,
        [string] $username,
        [string] $password
    )

    $latestBuildUrl = [System.String]::Format("{0}/httpAuth/app/rest/builds/?locator=buildType:{1},count:20,status:SUCCESS", $server, $buildTypeMain);
    $client = New-Object System.Net.WebClient
    $client.Credentials = New-Object System.Net.NetworkCredential $username, $password
    [xml]$latestMainBuilds = $client.DownloadString($latestBuildUrl)

    $MainBuilds = $latestMainBuilds.builds.build.number

    $Script:DependencyBuildMain = $MainBuilds -match $BuildNumber

}


function Set-CorrectBodytoPost() {
    param(
        [string]$DependencyBuildMain,
        [string]$FlakyProjectName
    )

    $Build = {<build>
    <triggeringOptions queueAtTop="true"/>
    <buildType id="FLAKYPROJECTNAME"/>
    <comment><text>FlakyFinder</text></comment>
    <properties>
    <property name="FlakyBuild" value="BUILDNUMBER"/>
    </properties>
    </build>}

    $ScriptBlockString = $Build.ToString()
    $ScriptBlockString = $ScriptBlockString.Replace("BUILDNUMBER", "$DependencyBuildMain")
    $ScriptBlockString = $ScriptBlockString.Replace("FLAKYPROJECTNAME", "$FlakyProjectName")

    $Script:Build = [ScriptBlock]::Create($ScriptBlockString)

}


#Main

Get-LatestBuildNumberFromTC $server $buildType $username $password
Write-Output "Latest successful build number from $buildType is: $Script:BuildNumber"

Get-DependentBuildFromMain $server $buildTypeMain $username $password
Write-Output "This is the Dependency WFMMain build: $Script:DependencyBuildMain"

Set-CorrectBodytoPost $Script:DependencyBuildMain $FlakyProjectName
Write-Output "This 'Body' will be POSTed to Teamcity host: `n $Script:Build"


Invoke-WebRequest -Uri $server/httpAuth/app/rest/buildQueue -Method POST -ContentType application/xml -Body $Script:Build -Credential $cred


