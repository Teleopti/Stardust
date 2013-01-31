	$here = Split-Path -Parent $MyInvocation.MyCommand.Path
    $sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
    . "$here\$sut"

Describe "Clean" {

	It "does something useful" {
		$true.should.be($false)
	}
}
	
Describe "FindAndReplace" {

	It "this should fail" {
		$temp = 1;
		$temp.should.be(2)
	}
	
}
