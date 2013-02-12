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
	
	$fileHash = @{};
	
	$files = Get-ChildItem -path "$path" -recurse -filter "$filter" | Where-object {!$_.psIsContainer -eq $true}
	foreach ($file in $files) {
		$encoding = Get-FileEncoding $file.FullName
		if ($encoding -ne $ExpectedEncoding) {
			$fileHash[$file.FullName] = $encoding
		}
	}

	return $fileHash
}