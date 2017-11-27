#Searching for the words fit & fdescribe in all spec.js files under web project. 
#To be sure that all tests is beeing run and no one accidentally commited it

$fit = gci "$env:WorkingDirectory\Teleopti.Ccc.Web\Teleopti.Ccc.Web\WFM\app\*.spec.js" -recurse | Select-String -pattern "\bfit\s*\(" 
$describe = gci "$env:WorkingDirectory\Teleopti.Ccc.Web\Teleopti.Ccc.Web\WFM\app\*.spec.js" -recurse | Select-String -pattern "\bfdescribe\b" 

if ($fit.count -gt "0") { 

    Write-Output "The word 'fit' has been found in the following file(s):`n "$fit.Path""
    exit 1
}

if ($describe.count -gt "0") { 

    Write-Output "The word 'fdescribe' has been found in the following file(s):`n "$describe.name""
    exit 1
}