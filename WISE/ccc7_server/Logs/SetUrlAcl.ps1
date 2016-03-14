$global:value = (netsh http show urlacl url=http://+:14100/)

if(!($global:value  -like '*http://+:14100/*'))
{
    $global:value = (netsh http add urlacl url=http://+:14100/ user=Everyone listen=yes)
}
    

