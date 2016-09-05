Param(

    [String]$BuildType,        #Build configuration ID, ex TeleoptiWFM_WebScenarioAll
    [String]$FlakyProjectName  #Build configuration ID, ex TeleoptiWFM_FlakyWebAll

)

#TeleoptiWFM_WebScenarioAll or TeleoptiWFM_WebScenarioCustomer
$username = "toptinet\tfsintegration"
$password = "m8kemew0rk"
$client = New-Object System.Net.WebClient
$client.Credentials = New-Object System.Net.NetworkCredential $username, $password

$latestBuildUrl = "http://devbuild01/httpAuth/app/rest/builds/?locator=buildType:$BuildType,count:1,status:SUCCESS"
[xml]$latestBuild = $client.DownloadString($latestBuildUrl)
$latestBuildNumber = $latestBuild.builds.build.number

$BuildNumber = $latestBuildNumber.split('-')[1].split(' ')

Write-Output ''
Write-Output "Latest successful build number from $BuildType is: $BuildNumber"
Write-Output ''

#TeleoptiWFM_WfmMain
$BuildTypeMain = "TeleoptiWFM_WfmMain"
$latestBuildUrl = "http://devbuild01/httpAuth/app/rest/builds/?locator=buildType:$BuildTypeMain,count:20,status:SUCCESS"
[xml]$latestMainBuilds = $client.DownloadString($latestBuildUrl)

$MainBuilds = $latestMainBuilds.builds.build.number

$DependencyBuildMain = $MainBuilds -match $BuildNumber

Write-Output "This is the Dependency WFMMain build: $DependencyBuildMain"
Write-Output ''

#Setting correct dependency from TeleoptiWFM_WfmMain
$Build = {<build>
    <triggeringOptions queueAtTop="true"/>
    <buildType id="FLAKYPROJECTNAME"/>
    <comment><text>FlakyFinder</text></comment>
    <properties>
        <property name="FlakyBuild" value="BUILDNUMBER"/>
    </properties>
</build>}

#Replacing BUILDNUMBER & FLAKYPROJECT name in Scriptblock $Build
$ScriptBlockString = $Build.ToString()
$ScriptBlockString = $ScriptBlockString.Replace("BUILDNUMBER", "$DependencyBuildMain")
$ScriptBlockString = $ScriptBlockString.Replace("FLAKYPROJECTNAME", "$FlakyProjectName")

$Build = [ScriptBlock]::Create($ScriptBlockString)

Write-Output "This 'Body' will be POSTed to Teamcity host: `n $Build"
Write-Output ''

#Used for accessing Teamcity
$secpasswd = ConvertTo-SecureString "m8kemew0rk" -AsPlainText -Force
$cred = New-Object System.Management.Automation.PSCredential($username,$secpasswd)

Invoke-WebRequest -Uri http://devbuild01/httpAuth/app/rest/buildQueue -Method POST -ContentType application/xml -Body $Build -Credential $cred