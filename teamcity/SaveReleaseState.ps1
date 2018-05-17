function Get-PinnedOutputFromTC([string]$baseUrl, [string]$projectId) 
{

    $url = "http://buildsrv01/guestAuth/app/rest/buildTypes/id:TeleoptiWFM_Output/builds/?locator=pinned:true,tag:Release,count:1,status:success"

    $xml = [xml](invoke-RestMethod -Uri $url -Method GET)
    $xpath = "/builds/build"
    $latestBuilds = Select-xml -xpath $xpath -xml $xml

    $buildIDs = $xml.builds.build

    foreach ($id in $buildids) 
    {

        $url2 = "http://buildsrv01/guestAuth/app/rest/builds/id:" + $id.id
        $xml2 = [xml](invoke-RestMethod -Uri $url2 -Method GET)

        $nr = $xml2.build.number
        $pos = $nr.IndexOf("-")
        $buildNumber = $nr.Substring($pos+2)
        
        $finishDate = $xml2.build.finishdate
        $finishDate = $finishDate.Substring(0,$finishDate.Length-5)
        $finishDate = [DateTime]::ParseExact($finishDate, "yyyyMMddTHHmmss" , $null)

        #write-host "$buildNumber - $finishDate"
        
        $myArray = @()
        $myArray += $buildNumber
        $myArray += $finishDate

        return $myArray

    }

}

function WriteReleaseState 
{

    $conn = New-Object System.Data.SqlClient.SqlConnection
    $connectionString = "Server=Erebus\SQL2014;Database=ReleaseState;User Id=buildmonitor;Password=m8kemew0rk;Initial Catalog=ReleaseState"

    $conn.ConnectionString = $connectionString
    $conn.Open()

    $cmd = New-Object System.Data.SqlClient.SqlCommand
    $cmd.Connection = $conn
    $cmd.CommandType = [System.Data.CommandType]::Text

    $query = "SELECT COUNT(*) FROM vReleasesPerWeek where weeknr = $weekNr AND year = $year"
    $cmd.CommandText = $query
    $exists = $cmd.ExecuteScalar()

    if ($exists -ne 0)
    {
        $query = "DELETE FROM dbo.Releases WHERE version IN(SELECT version FROM vReleasesPerWeek where weeknr = $weekNr AND year = $year)"
        $cmd.CommandText = $query
        $cmd.ExecuteNonQuery()
    }

    $query = "INSERT INTO dbo.Releases (timestamp, version) VALUES ('" + $Date + "', '" + $BuildNr + "')"
    $cmd.CommandText = $query
    $result = $cmd.ExecuteNonQuery() 

    $conn.Close()

}

$TcOutput = Get-PinnedOutputFromTC
$BuildNr = $TcOutput[0]
$Date = [System.DateTime]$TcOutput[1]
$weekNr = get-date $Date -UFormat %V
$year = $date.Year

WriteReleaseState