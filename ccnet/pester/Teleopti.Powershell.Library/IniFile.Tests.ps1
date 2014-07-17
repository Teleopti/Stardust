	$here = Split-Path -Parent $MyInvocation.MyCommand.Path
    $sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
    . "$here\$sut"

Describe "Write to .ini-file" {

	it "Should write one key/value to the ini file" {
		Setup -File "\test.ini" ""
		$testPath = (get-item "$TestDrive\test.ini").FullName

		$Category1 = @{"Key1"="Value1";"Key2"="Value2"}
		$Category2 = @{"Key1"="Value1";"Key2"="Value2"}
		$Hashtable = @{"Category1"=$Category1;"Category2"=$Category2}

		Out-IniFile -InputObject $Hashtable -FilePath "$testPath" -force
	
        [string] $testValue = Get-Content $testPath | Select-Object -last 2
        $testValue | Should Be "Key2=Value2 "
	}
 }

 
 Describe "Read from .ini-file" {

	it "Should read one key/value to the ini file" {
		Setup -File "\test.ini" ""
		$testPath = (get-item "$TestDrive\test.ini").FullName

		$Category1 = @{"Key1"="Value1";"Key2"="Value2"}
		$Category2 = @{"Key3"="Value3";"Key4"="Value4"}
		$Hashtable = @{"Category1"=$Category1;"Category2"=$Category2}

		Out-IniFile -InputObject $Hashtable -FilePath "$testPath" -force

		$FileContent = Get-IniContent "$testPath"
		
        [string] $testValue = $FileContent["Category2"]["Key3"]
        $testValue | Should Be "Value3"
	}
 }