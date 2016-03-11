$url = $args[0]

if($url -like '*http://*')
{
    $global:value = (netsh http show urlacl url=https://+:14100/)
    if($global:value  -like '*https://+:14100/*')
    {
        $global:value = (netsh http delete urlacl url=https://+:14100/)
    }
    $global:value = (netsh http add urlacl url=http://+:14100/ user=Everyone listen=yes)
}

if($url -like '*https://*')
{
    $global:value = (netsh http show urlacl url=http://+:14100/)
    Write-Host $url;
    if($global:value  -like '*http://+:14100/*')
    {
        $global:value = (netsh http delete urlacl url=http://+:14100/)
    }
    $global:value = (netsh http add urlacl url=https://+:14100/ user=Everyone listen=yes)
}


