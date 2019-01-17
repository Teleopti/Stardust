if not exists(select * from Tenant.ServerConfiguration where [Key] = 'NotificationSubscriptionApiEndpoint')
begin
	insert into Tenant.ServerConfiguration values
        ('NotificationSubscriptionApiEndpoint', '', 'Link to teleopti subscription endpoint'),
        ('NotificationSubscriptionApiEndpointKey', '', 'Key to access the subscription endpoint')
end
