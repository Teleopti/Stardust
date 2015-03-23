## =============================================
## Author:      Gianluca Sartori - @spaghettidba
## Create date: 2012-11-07
## Description: Changes the version information
##              in the header of a SQL Server trace
##
## Auther:      DavidJonsson1
## Update date: 2013-03-06
## Description: Turned into Powershell function + for-each
## =============================================
Param(
    [AllowEmptyString()]
    [string]$folderpath
  )

##===========
# functions
##===========
function ConvertTraceFile
{
    Param(
      [string]$filename
      )
    Write-Host "Fixing trace file: $filename"
    # The version information we want to write: 0x0A = 10 = SQLServer 2008
    [Byte[]] $versionData = 0x0A

    # The offset of the version information in the file
    $offset = 390

    [System.IO.FileMode] $open = [System.IO.FileMode]::OpenOrCreate

    $stream = New-Object System.IO.FileStream -ArgumentList $fileName, $open

    $stream.Seek($offset, [System.IO.SeekOrigin]::Begin)  | out-null;

    $stream.Write($versionData, 0, $versionData.Length);

    $stream.Close()
}

##===========
#main
##===========
#check input for empty string
if (($folderpath -eq '')) {
$invocation = (Get-Variable MyInvocation).Value
$folderpath = Split-Path $invocation.MyCommand.Path
}

foreach ($i in Get-ChildItem $folderpath -filter "*.trc")
{
    ConvertTraceFile -filename $i.FullName
}

