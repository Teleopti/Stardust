	$here = Split-Path -Parent $MyInvocation.MyCommand.Path
    $sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
    . "$here\$sut"

Describe "Find a string in a file and replace it with another string" {

	It "should fail throw exeption if file not exists" {
		[bool] $isOk = $false;
		try {
		  FindAndReplace "dontexist.dontexist" "searchstring" "replacestring"
		}
		catch
		{
            $isOk = $true;
		}
		$isOk.should.be($true)
	}
    
	It "should replace string" {
        Setup -File "forTest.txt" "gurka"
        FindAndReplace "$TestDrive\forTest.txt" "gurka" "banan"
        [string] $testValue = Get-Content "$TestDrive\forTest.txt"
        $testValue.should.be("banan")
	}
    
	It "should replace some CApITaL letters in string" {
        Setup -File "forTest.txt" "gurka"
        FindAndReplace "$TestDrive\forTest.txt" "GuRkA" "banan"
        [string] $testValue = Get-Content "$TestDrive\forTest.txt"
        $testValue.should.be("banan")
	}
    
	It "should not replace text fi not present" {
        Setup -File "forTest.txt" "gurka"
        FindAndReplace "$TestDrive\forTest.txt" "GzRkA" "banan"
        [string] $testValue = Get-Content "$TestDrive\forTest.txt"
        write-host "testValue is: $testValue"
        $testValue.should.be("gurka")
	}    

}
