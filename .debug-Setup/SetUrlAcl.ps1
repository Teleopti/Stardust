$global:value = (netsh http show urlacl url=http://+:14100/)

if(!($global:value  -like '*http://+:14100/*'))
{
    $global:value = (netsh http add urlacl url=http://+:14100/ user=Everyone listen=yes)

    if($global:value  -like '*Error: 5*')
    {
        Write-host 'You must run this as administrator to add permissions to listen on ports'
        start-sleep -seconds 5
        Exit   
    }
}



