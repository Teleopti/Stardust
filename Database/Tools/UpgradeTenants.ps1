$integrated = "false"

if($args[0] -eq "1")
{
    $integrated = "true"
}

$user=$args[1]
$pwd=$args[2]
$url=$args[3]
$url= $url + "UpgradeAllTenants"

$body = @{
    "Tenant" = ""
    "AdminUserName" = $user
    "AdminPassword" = $pwd
    "UseIntegratedSecurity" = $integrated
}

#write $integrated
Invoke-RestMethod -Method Post $url -Body ($body) -timeout 300

