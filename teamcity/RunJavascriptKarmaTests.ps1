Set-Location "$Env:WorkingDirectory\Teleopti.Ccc.Web\Teleopti.Ccc.Web\WFM"
. ..\.node\UseNodeEnv.ps1
#& npm ci

$output = & 'npm' ci

Write-Output $output
if ($LASTEXITCODE -ne 0)
{
    $err = $output.Where{$PSItem -match ' npm ERR'}
    Write-Output "NPM CI FAILED: $err" -ErrCode $LASTEXITCODE
    Exit 1
}

npm run test:teamcity