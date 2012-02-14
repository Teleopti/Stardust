#get input
param($PhysicalPath,$DefaultSite,$SiteName,$AppPoolName)

$PathExist = Test-Path $PhysicalPath
If ($PathExist -eq $True) {
	import-module webadministration

	Remove-Item IIS:\Sites\$DefaultSite\$SiteName -recurse
	Remove-Item IIS:\AppPools\$AppPoolName -recurse
	Remove-Item $PhysicalPath -recurse
}