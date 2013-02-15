	$here = Split-Path -Parent $MyInvocation.MyCommand.Path
    $sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
    . "$here\$sut"

Describe "Azure, copy from BlobStorage, check our pre-reqs" {
    
	It "We must be admin" {
		[bool] $isAdmin
		$isAdmin = Test-Administrator($myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent());
		$isAdmin | Should Be $true
	}
}

Describe "Azure, copy from BlobStorage, use Robocopy for internal copy" {
    
    It "Return code from Robocopy shold be 1 when copy one file, first time" {
        Setup -File "srcFolder\someFile.txt" "ContentInRoboCopyFile"
        $returnValue = Roby-Copy "$TestDrive\srcFolder" "$TestDrive\destFolder"
        $returnValue | Should Be 1
	}


    It "Return code from Robocopy shold be 0 when file already exist" {
        Setup -File "\srcFolder\forTest.txt" "ContentInRoboCopyFile"
        $returnValue = Roby-Copy "$TestDrive\srcFolder" "$TestDrive\destFolder"
        $returnValue = Roby-Copy "$TestDrive\srcFolder" "$TestDrive\destFolder"
        $returnValue | Should Be 0
	}

    It "File content shold be the same in destFile and srcFile after Robocopy" {
        $fileContent = "ContentInRoboCopyFile"
        Setup -File "\srcFolder\forTest.txt" $fileContent
        Roby-Copy "$TestDrive\srcFolder" "$TestDrive\destFolder"
        [string] $testValue = Get-Content "$TestDrive\destFolder\forTest.txt"
        $testValue | Should Be $fileContent
	}
}
    
    
Describe "We should be able to init event log, twice" {
    
	It "create same event log source twice" {
		EventlogSource-Create "test"
        EventlogSource-Create "test"
	}
    
}