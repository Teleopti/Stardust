function Get-FileEncoding4bytes
{
    [CmdletBinding()] Param (
     [Parameter(Mandatory = $True, ValueFromPipelineByPropertyName = $True)] [string]$Path
    )

    [byte[]]$byte = get-content -Encoding byte -ReadCount 4 -TotalCount 4 -Path $Path
	
	if (!$byte) #file is empty
	{ return "UTF8" }
    if ( $byte[0] -eq 0xef -and $byte[1] -eq 0xbb -and $byte[2] -eq 0xbf )
    { return "UTF8" }
    elseif ($byte[0] -eq 0 -and $byte[1] -eq 0 -and $byte[2] -eq 0xfe -and $byte[3] -eq 0xff)
    { return "UTF32" }
    elseif ($byte[0] -eq 0x2b -and $byte[1] -eq 0x2f -and $byte[2] -eq 0x76)
    { return "UTF7"}
    elseif ($byte[0] -eq 0xff -and $byte[1] -eq 0xfe)
    { return "UCS-2 Little Endian"}
    elseif ($byte[0] -eq 0xfe -and $byte[1] -eq 0xff)
    { return "UCS-2 Big Endian"}	
    else
	{ return "UTF8 without BOM"}	
}

function Get-IsEncodingUTF8 {
    param(
        ## The path of the file to get the encoding of.
        $Path
    )
	$EncodingType = "UTF8"
	$fileEncoding = Get-FileEncoding4bytes -Path $Path
	return [bool]$fileEncoding.StartsWith($EncodingType)
}

function Get-IsEncodingType {
    param(
        ## The path of the file to get the encoding of.
        $Path,
		$EncodingType
    )
	$fileEncoding = Get-FileEncoding4bytes -Path $Path
	return [bool]($fileEncoding -eq $EncodingType)
}

function Get-FileEncoding {
    param(
        ## The path of the file to get the encoding of.
        $Path
    )

	Set-StrictMode -Version Latest

	$result = "utf7";
	
    ## The hashtable used to store our mapping of encoding bytes to their
    ## name. For example, "255-254 = Unicode"
    $encodings = @{}

    ## Find all of the encodings understood by the .NET Framework. For each,
    ## determine the bytes at the start of the file (the preamble) that the .NET
    ## Framework uses to identify that encoding.
    $encodingMembers = [System.Text.Encoding] |
        Get-Member -Static -MemberType Property

    $encodingMembers | Foreach-Object {
        $encodingBytes = [System.Text.Encoding]::($_.Name).GetPreamble() -join '-'
        $encodings[$encodingBytes] = $_.Name
    }

    ## Find out the lengths of all of the preambles.
    $encodingLengths = $encodings.Keys | Where-Object { $_ } |
        Foreach-Object { ($_ -split "-").Count }

    ## Go through each of the possible preamble lengths, read that many
    ## bytes from the file, and then see if it matches one of the encodings
    ## we know about.
    foreach($encodingLength in $encodingLengths | Sort -Descending)
    {
        $bytes = (Get-Content -encoding byte -readcount $encodingLength $path)[0]
        $encoding = $encodings[$bytes -join '-']

        ## If we found an encoding that had the same preamble bytes,
        ## save that output and break.
        if($encoding)
        {
            $result = $encoding
            break
        }
    }

	#Test fail when returning ( and ) in encodingName string
	[string] $return = "";
	$return = [System.Text.Encoding]::$result.EncodingName

    ## Finally, output the encoding.
    return $return
}

function Get-FileListOtherEncoding {
    param(
        $path,
		$filter,
		$ExpectedEncoding
    )
	$fileHash=@{}
	$files = Get-FileListByFilter -path "$path" -filter "$filter"
	foreach ($file in $files) {
		$encoding = Get-FileEncoding $file.FullName
		if ($encoding -ne $ExpectedEncoding) {
			$fileHash[$file.FullName] = $encoding
		}
	}
	return $fileHash
}

function Get-FileListByFilter {
    param(
        $path,
		$filter
	)

	#bug: http://connect.microsoft.com/PowerShell/feedback/details/354672/get-childitem-count-problem-with-one-file
	$files = Get-ChildItem -path "$path" -recurse -filter "$filter" | Where-object {!$_.psIsContainer -eq $true}
	return $files
}

function Get-Banan {
return "banan"
}

function CheckRepoEncoding {
    param(
        $path,
		[string]$filter
    )
	
	$wrongFiles=@{}
	$files = @(get-childitem -recurse -path $path -filter $filter)
	
	foreach ($file in $files) {
		if (!(Get-IsEncodingUTF8 -Path $file.FullName)) {
			$wrongFiles[$file.FullName] = Get-FileEncoding4bytes -Path $file.FullName
		}
	}
	return $wrongFiles
}
