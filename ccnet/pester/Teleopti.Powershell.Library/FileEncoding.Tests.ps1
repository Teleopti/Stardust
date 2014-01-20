$here = Split-Path -Parent $MyInvocation.MyCommand.Path
    $sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
    . "$here\$sut"

function CreateFolderStructureUTF7Only {
<# FolderStructur:
+-TestDrive/
  +-subDir/
  | +-test.txt
| +-test.txt
| +-test2.txt
#>

	Setup -File "\subDir\forTest.txt" "some content"
	$testFile3="$TestDrive\SubDir\test.txt"
	$testFile3 | Out-File $testFile3 -Force -Encoding utf7
	
	$testFile1="TestDrive:\test.txt"
	Set-Content $testFile1 -value "my test text"
	$testFile1 | Out-File $testFile1 -Force -Encoding utf7
	
	$testFile2="TestDrive:\test2.txt"
	Set-Content $testFile2 -value "my other test text"
	$testFile2 | Out-File $testFile2 -Force -Encoding utf7
}

Describe "Write and read files with different encoding" {

	it "Should return two files in path based on filter" {
		Cleanup
		Setup -File "\forTest.txt" "some file content"
		Setup -File "\forTest2.txt" "some file content"

		$testPath = (get-item "$TestDrive\forTest.txt").FullName
		$Files = @(Get-FileListByFilter -path "$TestDrive" -filter "*.txt")
		$Files.count | Should Be 2
		$Files[0].FullName | Should Be $testPath
	}
    
		it "Should return one file in path based on filter" {
		Cleanup
		Setup -File "\forTest.txt" "some file content"
		$testPath = (get-item "$TestDrive\forTest.txt").FullName
		
		$Files = @(Get-FileListByFilter -path "$TestDrive" -filter "*.txt")
		$Files.count | Should Be 1
		$Files[0].FullName | Should Be $testPath
	}
	
    #This is UTF-8
	It "File should be utf8" {
		Cleanup
		Setup -File "\testUTF8.txt" "some file content"
        $testPath="TestDrive:\testUTF8.txt"
		Set-Content $testPath -value "my test text."
		$testPath | Out-File $testPath -Force -Encoding utf8
		Get-IsEncodingUTF8 "$testPath" | Should Be True
	}
	
	#This is ANSI
	It "File should be utf7" {
		Cleanup
		Setup -File "\testUTF7.txt" "some file content"
        $testPath="TestDrive:\testUTF7.txt"
		Set-Content $testPath -value "my test text."
		$testPath | Out-File $testPath -Force -Encoding utf7
		Get-FileEncoding "$testPath" | Should Be "Unicode (UTF-7)"
	}

	It "A empty ANSI file should pass as OK" {
		Cleanup
		Setup -File "\test.txt" "some file content"
        $testPath="TestDrive:\test.txt"
		Set-Content $testPath -value ""
		$testPath | Out-File $testPath -Force -Encoding utf7
		Get-FileEncoding "$testPath" | Should Be "Unicode (UTF-7)"
	}
		
	It "All files in a directory are encoded as utf7" {
		Cleanup
		CreateFolderStructureUTF7Only
		$ZeroFiles = Get-FileListOtherEncoding "$TestDrive" "*.txt" "Unicode (UTF-7)"
		$ZeroFiles.Count | Should Be 0
	}

	It "All files but 1 in a directory are encoded as utf7" {
		Cleanup
		CreateFolderStructureUTF7Only
		Setup -File "\SubDir3\forTest.txt" "some other content"
		$testFile4="$TestDrive\SubDir3\test.txt"
		$testFile4 | Out-File $testFile4 -Force -Encoding utf8
		
		$OneFiles = Get-FileListOtherEncoding "$TestDrive" "*.txt" "Unicode (UTF-7)"
		$OneFiles.Count | Should Be 1
	}
}

Describe "Write and read files with different encoding - Using Mock" {
#put a dummy mock here to be used in another "describe" block
#Mocks used for the rest of this descibe block:
Mock Get-FileListByFilter {return @{FullName="A_File.TXT"}} -ParameterFilter {$filter -eq "*.txt"}
Mock Get-FileEncoding {return "Unicode (UTF-7)"} -ParameterFilter {$Path -eq "A_File.TXT"}
Mock Get-FileListByFilter {return @{FullName="A_File.sql"}} -ParameterFilter {$filter -eq "*.sql"}
Mock Get-FileEncoding {return "Unicode (UTF-8)"} -ParameterFilter {$Path -eq "A_File.sql"}

	It "one file, correct encoding" {
		$ZeroFiles = Get-FileListOtherEncoding -path "$TestDrive" -filter "*.txt" -ExpectedEncoding "Unicode (UTF-7)"
		$ZeroFiles.count | Should Be 0
	}

	it "one file, incorrect encoding" {
		$OneFiles = Get-FileListOtherEncoding -path "$TestDrive" -filter "*.txt" -ExpectedEncoding "Unicode (UTF-8)"
		$OneFiles.Count | Should Be 1
	}

}

Describe "Investigate mock a cross describe" {
Mock Get-Banan {return "gurka"}

	It "I should get the mock if it's defined in the current describe block and I don't use Cleanup" {
		$string = Get-Banan
		$string | Should Be "gurka"
	}

	It "I should _not_ get the mock when defined in current describe block and I _do_ use Cleanup" {
		Cleanup
		$string = Get-Banan
		$string | Should Be "banan"
	}
}

Describe "Check Repo for wrong encoding" {

	It "All *.sql files should be UTF8 or UTF8 without BOM" {
		$files = CheckRepoEncoding -path "$here\..\..\.." -filter "*.sql"

		#Expose to CCNet
		If ($files.count -gt 0 ) {
			Write-Host "----------Wrong Encoding found!-------------"
			foreach($item in $files.GetEnumerator() | Sort Name)
			{
				Write-Host $item.Name ":" $item.Value
			}
			Write-Host "--------------------------------------------"
		}		
		$files.Count | Should Be 0
	}

	It "All *.cs  files should be UTF8 or UTF8 without BOM" {
		$files = CheckRepoEncoding -path "$here\..\..\.." -filter "*.cs"

		#Expose to CCNet
		If ($files.count -gt 0 ) {
			Write-Host "----------Wrong Encoding found!-------------"
			foreach($item in $files.GetEnumerator() | Sort Name)
			{
				Write-Host $item.Name ":" $item.Value
			}
			Write-Host "--------------------------------------------"
		}		
		$files.Count | Should Be 0
	}
}

