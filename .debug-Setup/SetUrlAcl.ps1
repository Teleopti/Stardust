
$global:value = (netsh http show urlacl url=http://+:9000/)

if(!($global:value  -like '*http://+:9000/*'))
{
    $global:value = (netsh http add urlacl url=http://+:9000/ user=Everyone listen=yes)

    if($global:value  -like '*5*')
    {
        Write-host 'You must run this as administrator to add permissions to listen on ports'
        pause
        Exit   
    }
    netsh http add urlacl url=http://+:9001/ user=Everyone listen=yes
}



