<#
.Synopsis
   Short description
.DESCRIPTION
   Long description
.EXAMPLE
   Example of how to use this cmdlet
.EXAMPLE
   Another example of how to use this cmdlet
.INPUTS
   Inputs to this cmdlet (if any)
.OUTPUTS
   Output from this cmdlet (if any)
.NOTES
   Version 1.2
.COMPONENT
   The component this cmdlet belongs to
.ROLE
   The role this cmdlet belongs to
.FUNCTIONALITY
   The functionality that best describes this cmdlet
#>
Param(
[parameter(mandatory=$False,HelpMessage='Test file size in GB')] 
[int]$TestFileSizeInGB = 1,

[parameter(mandatory=$False,HelpMessage='Path to test folder')] 
[ValidateLength(3,254)] 
$TestFilepath = 'C:\Test',

[parameter(mandatory=$True,HelpMessage='Test mode, use Get-SmallIO for IOPS and Get-LargeIO for MB/s ')] 
[ValidateSet('SequentialReads64kb','RandomReads64kb','RandomWrite8kb','SequentialWrite2kb','SequentialWrite6kb')]
$TestMode,

[parameter(mandatory=$False,HelpMessage='Fast test mode or standard')] 
[ValidateSet('True','False')] 
$FastMode = 'True',

[parameter(mandatory=$False,HelpMessage='present in GUI or text')] 
[ValidateSet('Out-GridView','Format-Table')] 
$OutputFormat='Out-GridView'
)

Function New-TestFile{
    $Folder = New-Item -Path $TestFilePath -ItemType Directory -Force -ErrorAction SilentlyContinue
    $TestFileAndPath = "$TestFilePath\$TestFileName"
    $FileExist = Test-Path $TestFileAndPath
    if ($FileExist -eq $True)
    {
        Write-host 'file exists'
    }
    else
    {
        $TestFileSizeInBytes = $TestFileSizeInGB*1024*1024*1024
        $cmdArgs = @("file","createnew","$TestFileAndPath","$TestFileSizeInBytes")
        & FSUTIL.EXE @cmdArgs
        $cmdArgs = @("file","setvaliddata","$TestFileAndPath","$TestFileSizeInBytes")
        & FSUTIL.EXE @cmdArgs
    }
}

Function SequentialWrite6kb{
$KBytes = '6'
$Type = 'sequential'
Write-Host 'Initialize for SequentialWrite6kb ...'
Write-Host 'Test log operations with sequential writes of 6kb'
Write-Host "Writing $KBytes Bytes in $Type mode using $TestFilePath\$TestFileName as target"

1,2,4,8,16 | % {
    $b = "-b$KBytes";
    $f = "-f$Type";
    $o = "-o $_";  
    $t = "-t$cores";
    $Result = & $RunningFromFolder\..\sqlio.exe $Duration -kW $f $b $o $t -LS -BN "$TestFilePath\$TestFileName"
    Start-Sleep -Seconds 5 -Verbose
    $iops = $Result.Split("`n")[10].Split(':')[1].Trim() 
    $mbs = $Result.Split("`n")[11].Split(':')[1].Trim() 
    $latency = $Result.Split("`n")[14].Split(':')[1].Trim()
    #$SeqRnd = $Result.Split("`n")[14].Split(':')[1].Trim()
    New-object psobject -property @{
        Type = $($Type)
        SizeIOKBytes = $($KBytes)
        OutStndIOs = $($_)
        IOPS = $($iops)
        MBSec = $($mbs)
        LatencyMS = $($latency)
        #Target = $("$TestFilePath\$TestFileName")
        }
    }
}


Function SequentialWrite2kb{
$KBytes = '2'
$Type = 'sequential'
Write-Host 'Initialize for SequentialWrite2kb ...'
Write-Host 'Test log operations with sequential writes of 2kb'
Write-Host "Writing $KBytes Bytes in $Type mode using $TestFilePath\$TestFileName as target"

1,2,4,8,16 | % {
    $b = "-b$KBytes";
    $f = "-f$Type";
    $o = "-o $_";  
    $t = "-t$cores";
    $Result = & $RunningFromFolder\..\sqlio.exe $Duration -kW $f $b $o $t -LS -BN "$TestFilePath\$TestFileName"
    Start-Sleep -Seconds 5 -Verbose
    $iops = $Result.Split("`n")[10].Split(':')[1].Trim() 
    $mbs = $Result.Split("`n")[11].Split(':')[1].Trim() 
    $latency = $Result.Split("`n")[14].Split(':')[1].Trim()
    #$SeqRnd = $Result.Split("`n")[14].Split(':')[1].Trim()
    New-object psobject -property @{
        Type = $($Type)
        SizeIOKBytes = $($KBytes)
        OutStndIOs = $($_)
        IOPS = $($iops)
        MBSec = $($mbs)
        LatencyMS = $($latency)
        #Target = $("$TestFilePath\$TestFileName")
        }
    }
}

Function SequentialRead2kb{
$KBytes = '2'
$Type = 'sequential'
Write-Host 'Initialize for SequentialRead2kb ...'
Write-Host 'Test log operations with sequential read of 2kb'
Write-Host "Writing $KBytes Bytes in $Type mode using $TestFilePath\$TestFileName as target"

1,2,4,8,16 | % {
    $b = "-b$KBytes";
    $f = "-f$Type";
    $o = "-o $_";  
    $t = "-t$cores";
    $Result = & $RunningFromFolder\..\sqlio.exe $Duration -kR $f $b $o $t -LS -BN "$TestFilePath\$TestFileName"
    Start-Sleep -Seconds 5 -Verbose
    $iops = $Result.Split("`n")[10].Split(':')[1].Trim() 
    $mbs = $Result.Split("`n")[11].Split(':')[1].Trim() 
    $latency = $Result.Split("`n")[14].Split(':')[1].Trim()
    #$SeqRnd = $Result.Split("`n")[14].Split(':')[1].Trim()
    New-object psobject -property @{
        Type = $($Type)
        SizeIOKBytes = $($KBytes)
        OutStndIOs = $($_)
        IOPS = $($iops)
        MBSec = $($mbs)
        LatencyMS = $($latency)
        #Target = $("$TestFilePath\$TestFileName")
        }
    }
}
Function RandomWrite8kb{
$KBytes = '8'
$Type = 'random'
Write-Host 'Initialize for RandomWrite8kb ...'
Write-Host 'e.g checkpoint operations with many random writes of 8kb'
Write-Host "Writing $KBytes Bytes in $Type mode using $TestFilePath\$TestFileName as target"

1,2,4,8,16,32 | % {
    $b = "-b$KBytes";
    $f = "-f$Type";
    $o = "-o $_";  
    $t = "-t$cores";
    $Result = & $RunningFromFolder\..\sqlio.exe $Duration -kW $f $b $o $t -LS -BN "$TestFilePath\$TestFileName"
    Start-Sleep -Seconds 5 -Verbose
    $iops = $Result.Split("`n")[10].Split(':')[1].Trim() 
    $mbs = $Result.Split("`n")[11].Split(':')[1].Trim() 
    $latency = $Result.Split("`n")[14].Split(':')[1].Trim()
    #$SeqRnd = $Result.Split("`n")[14].Split(':')[1].Trim()
    New-object psobject -property @{
        Type = $($Type)
        SizeIOKBytes = $($KBytes)
        OutStndIOs = $($_)
        IOPS = $($iops)
        MBSec = $($mbs)
        LatencyMS = $($latency)
        #Target = $("$TestFilePath\$TestFileName")
        }
    }
}

Function RandomReads64kb{
$KBytes = '64'
$Type = 'random'
Write-Host 'Initialize for RandomReads64kb ...'
Write-Host 'e.g bookmark lookups or when reading from fragmented tables.'
Write-Host "Reading $KBytes Bytes in $Type mode using $TestFilePath\$TestFileName as target"

1,2,4,8,16,32 | % {
    $b = "-b$KBytes";
    $f = "-f$Type";
    $o = "-o $_";  
    $t = "-t$cores";
    $Result = & $RunningFromFolder\..\sqlio.exe $Duration -kR $f $b $o $t -LS -BN "$TestFilePath\$TestFileName"
    Start-Sleep -Seconds 5 -Verbose
    $iops = $Result.Split("`n")[10].Split(':')[1].Trim() 
    $mbs = $Result.Split("`n")[11].Split(':')[1].Trim() 
    $latency = $Result.Split("`n")[14].Split(':')[1].Trim()
    #$SeqRnd = $Result.Split("`n")[14].Split(':')[1].Trim()
    New-object psobject -property @{
        Type = $($Type)
        SizeIOKBytes = $($KBytes)
        OutStndIOs = $($_)
        IOPS = $($iops)
        MBSec = $($mbs)
        LatencyMS = $($latency)
        #Target = $("$TestFilePath\$TestFileName")
        }
    }
}

Function SequentialReads64kb{
$KBytes = '64'
$Type = 'sequential'
Write-Host 'Initialize for SequentialReads64kb...'
Write-Host 'e.g when we do seeks or scans on indexes or heaps that are not fragmented'

Write-Host "Reading $KBytes Bytes in $Type mode using $TestFilePath\$TestFileName as target"
1,2,4,8,16,32 | % {
    $b = "-b$KBytes";
    $f = "-f$Type";
    $o = "-o $_";  
    $t = "-t$cores";
    $Result = & $RunningFromFolder\..\sqlio.exe $Duration -kR $f $b $o $t -LS -BN "$TestFilePath\$TestFileName"
    Start-Sleep -Seconds 5 -Verbose
    $iops = $Result.Split("`n")[10].Split(':')[1].Trim() 
    $mbs = $Result.Split("`n")[11].Split(':')[1].Trim() 
    $latency = $Result.Split("`n")[14].Split(':')[1].Trim()
    #$SeqRnd = $Result.Split("`n")[14].Split(':')[1].Trim()
    New-object psobject -property @{
        Type = $($Type)
        SizeIOKBytes = $($KBytes)
        OutStndIOs = $($_)
        IOPS = $($iops)
        MBSec = $($mbs)
        LatencyMS = $($latency)
        #Target = $("$TestFilePath\$TestFileName")
        }
    }
}

#================================
# Main
#================================

# Are we elevated? else restart script
# Get the ID and security principal of the current user account
$myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()
$myWindowsPrincipal=new-object System.Security.Principal.WindowsPrincipal($myWindowsID)
 
# Get the security principal for the Administrator role
$adminRole=[System.Security.Principal.WindowsBuiltInRole]::Administrator
 
# Check to see if we are currently running "as Administrator"
if ($myWindowsPrincipal.IsInRole($adminRole))
{
    # We are running EventlogSource-Create"as Administrator" - so change the title and background color to indicate this
    $Host.UI.RawUI.WindowTitle = $myInvocation.MyCommand.Definition + "(Elevated)"
    clear-host
}
else
{
   throw "You need to be in admin mode to execute this script!"
}

Try
{

	#Checking for fast mode
	if ($FastMode -lt $True){$Duration = '-s60'}else{$Duration = '-s10'}

    #cores in this server
    $cores = (Get-WmiObject –class Win32_processor | select NumberOfCores).NumberOfCores
	$TestFileName = "test.dat"
	
	#Setting script location to find the exe's
	$RunningFromFolder = $MyInvocation.MyCommand.Path | Split-Path -Parent 

	#Main
	. New-TestFile
	switch ($OutputFormat){
		'Out-GridView' {
		. $TestMode | Select-Object MBSec,IOPS,SizeIOKBytes,LatencyMS,OutStndIOs,Type| Out-GridView
		}
		'Format-Table' {
		. $TestMode | Select-Object MBSec,IOPS,SizeIOKBytes,LatencyMS,OutStndIOs,Type | Format-Table
		}
		Default {}
	}
}

Catch [Exception]
{
	$ErrorMessage = $_.Exception.Message
    Write-Host "$ErrorMessage"
	Throw "Script failed, Check log for details";
	Start-Sleep -s 3
}
Finally
{
	Write-Host "Done!"
}