if not exists(select * from Tenant.ServerConfiguration where [Key] = 'NotificationExtProviderEnabled')
begin
	insert into Tenant.ServerConfiguration values
        ('NotificationExtProviderEnabled', 'false', 'Enable or disable the external notification provider')
end
