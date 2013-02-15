$here = Split-Path -Parent $MyInvocation.MyCommand.Path
    $sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
    . "$here\$sut"

Describe "Write and read files with different encoding" {
    
    #This is UTF-8
	It "File should be utf8" {
        $testPath="TestDrive:\test.txt"
		Set-Content $testPath -value "my test text."
		
		$testPath | Out-File $testPath -Force -Encoding utf8
		Get-FileEncoding "$testPath" | Should Be "Unicode (UTF-8)"
	}
	
	#This is ANSI
	It "File should be utf7" {
        $testPath="TestDrive:\test.txt"
		Set-Content $testPath -value "my test text."
		
		$testPath | Out-File $testPath -Force -Encoding utf7
		Get-FileEncoding "$testPath" | Should Be "Unicode (UTF-7)"
	}

	#Test files, dependency later on!
	$testFile1="TestDrive:\test.txt"
	Set-Content $testFile1 -value "my test text."
	$testFile1 | Out-File $testFile1 -Force -Encoding utf7
	
	$testFile2="TestDrive:\test.txt"
	Set-Content $testFile2 -value "my test text."
	$testFile2 | Out-File $testFile2 -Force -Encoding utf7
	
	Setup -File "\subDir\forTest.txt" "some other content"
	$testFile3="$TestDrive\SubDir\test.txt"
	$testFile3 | Out-File $testFile3 -Force -Encoding utf7
		
	It "All files in a directory are encoded as utf7" {
		$ZeroFiles = @{};
		$ZeroFiles = Get-FileListOtherEncoding "$TestDrive" "*.txt" "Unicode (UTF-7)"
		$ZeroFiles.Count | Should Be 0
	}
	
	#More test files, depends  on previous
	Setup -File "\SubDir3\forTest.txt" "some other content"
	$testFile4="$TestDrive\SubDir3\test.txt"
	$testFile4 | Out-File $testFile4 -Force -Encoding utf8
		
	It "All files but 1 in a directory are encoded as utf7" {
		$OneFiles = @{};
		$OneFiles = Get-FileListOtherEncoding "$TestDrive" "*.txt" "Unicode (UTF-7)"
		$OneFiles.Count | Should Be 1
	}
}