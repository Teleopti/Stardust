if COL_LENGTH('Tenant.ServerConfiguration', 'columnName') is null
begin
    alter table Tenant.ServerConfiguration
    add	[Description] nvarchar(1024) not null
    default ''
    with values
end

go

begin
    update Tenant.ServerConfiguration
        set [Description] = 'Add urls to let mytime or ASM widget work in an iframe.'
        where [Key] = 'FrameAncestors'
    update Tenant.ServerConfiguration
        set [Description] = 'Add Application Insights instrumentation key for log analytics.'
        where [Key] = 'InstrumentationKey'
    update Tenant.ServerConfiguration
        set [Description] = 'Analysis database for PM service.'
        where [Key] = 'AS_DATABASE'
    update Tenant.ServerConfiguration
        set [Description] = 'Analysis database server for PM service.'
        where [Key] = 'AS_SERVER_NAME'
    update Tenant.ServerConfiguration
        set [Description] = 'Indicate whether PM service is installed.'
        where [Key] = 'PM_INSTALL'
    update Tenant.ServerConfiguration
        set [Description] = 'How many days the logon attempts are saved.'
        where [Key] = 'PreserveLogonAttempts'
    update Tenant.ServerConfiguration
        set [Description] = 'OBS! This is not used in an Azure installation. The path to where the payroll files are placed. Leave blank if you want to use the the default path Payroll.DeployNew\[TENANTNAME] under the Service Bus.'
        where [Key] = 'PayrollSourcePath'
end

if not exists(select * from Tenant.ServerConfiguration where [Key] = 'NotificationApiEndpoint')
begin
    -- Now we can presume a bunch of fields are not already migrated
	insert into Tenant.ServerConfiguration values
        ('NotificationApiEndpoint', 'https://api.teleopticloud.com/TeleoptiNotifications/Notify', ''),
        ('NotificationExtProviderEnabled', 'false', ''),
        ('NotificationExtProviderApiId', '', ''),
        ('NotificationExtProviderAssembly', 'Teleopti.Ccc.Domain', ''),
        ('NotificationExtProviderClass', 'Teleopti.Ccc.Domain.Notification.ClickatellNotificationSender', ''),
        ('NotificationExtProviderData', '', ''),
        ('NotificationExtProviderErrorCode', 'fault', ''),
        ('NotificationExtProviderFindSuccessOnError', 'Error', ''),
        ('NotificationExtProviderFrom', '', ''),
        ('NotificationExtProviderPassword', '', ''),
        ('NotificationExtProviderSkipSearch', 'false', ''),
        ('NotificationExtProviderSuccessCode', '', ''),
        ('NotificationExtProviderUrl', 'http://api.clickatell.com/xml/xml?data=', ''),
        ('NotificationExtProviderUsername', '', ''),
        ('NotificationSmtpEnabled', 'false', ''),
        ('NotificationSmtpHost', '', ''),
        ('NotificationSmtpPassword', '', ''),
        ('NotificationSmtpPort', '', ''),
        ('NotificationSmtpUser', '', ''),
        ('NotificationSmtpUseRelay', '', '')
end
