if not exists(select * from Tenant.ServerConfiguration where [Key] = 'GrantBotApiUrl')
begin
	insert into Tenant.ServerConfiguration values
        ('GrantBotApiUrl', '', 'Link to Grant api endpoint'),
        ('GrantBotDirectLineSecret', '', 'Key to access the DirectLine channel')
end
