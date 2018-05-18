$Username = 'toptinet\tfsintegration'
$Password = 'm8kemew0rk'
$pass = ConvertTo-SecureString -AsPlainText $Password -Force
$Cred = New-Object System.Management.Automation.PSCredential -ArgumentList $Username,$pass

invoke-command -ComputerName WFMSmoke -Credential $cred -ScriptBlock {

Set-WebConfigurationProperty -Filter "/system.webServer/security/authentication/AnonymousAuthentication" `
                             -Name Enabled `
                             -Value True `
                             -PSPath "IIS:\Sites\Default Web Site\TeleoptiWFM\Client" }