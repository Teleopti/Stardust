	$here = Split-Path -Parent $MyInvocation.MyCommand.Path
    $sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
    . "$here\$sut"

Describe "Find a string in a file and replace it with another string" {

	It "should throw exeption if file not exists" {
		{ FindAndReplace "dontexist.dontexist" "searchstring" "replacestring" } | Should Throw
	}
    
	It "should replace string" {
	    $testPath="TestDrive:\test.txt"
		Set-Content $testPath -value "gurka"

        FindAndReplace $testPath "gurka" "banan"
        [string] $testValue = Get-Content $testPath
        $testValue | Should Be "banan"
	}
    
	It "should replace some CApITaL letters in string" {
	    $testPath="TestDrive:\test.txt"
		Set-Content $testPath -value "gurka"
		
        FindAndReplace "$testPath" "GuRkA" "banan"
        [string] $testValue = Get-Content "$testPath"
        $testValue | Should Be "banan"
	}
    
	It "should not replace text if not present" {
	    $testPath="TestDrive:\test.txt"
		Set-Content $testPath -value "gurka"
		
        FindAndReplace "$testPath" "GzRkA" "banan"
        [string] $testValue = Get-Content "$testPath"
        $testValue | Should Be "gurka"
	}    

}
