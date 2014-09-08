param([string]$ScriptPath)
write-host $ScriptPath
powershell -File "$ScriptPath" 2>&1 | more
	Add-Content "$fullPathsettingsFile" "`$(URL)|https://$DataSourceName.teleopticloud.com/Web/"
    Add-Content "$fullPathsettingsFile" "`$(DEFAULT_IDENTITY_PROVIDER)|Teleopti"
    Add-Content "$fullPathsettingsFile" "`$(WindowsClaimProvider)|"
    Add-Content "$fullPathsettingsFile" "`$(TeleoptiClaimProvider)|<add identifier=`"urn:Teleopti`" displayName=`"Teleopti`" url=`"https://$DataSourceName.teleopticloud.com/Web/sso/`" protocolHandler=`"OpenIdHandler`" />"
    Add-Content "$fullPathsettingsFile" "`$(UrlAuthenticationBridge)|https://$DataSourceName.teleopticloud.com/AuthenticationBridge/"