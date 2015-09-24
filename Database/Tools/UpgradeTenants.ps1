$user=$args[0]
$pwd=$args[1]
$url=$args[2]
$url= $url + "UpgradeAllTenants"

$body = @{
    "Tenant" = ""
    "AdminUserName" = $user
    "AdminPassword" = $pwd
}
Invoke-RestMethod -Method Post $url -Body ($body) -timeout 300

