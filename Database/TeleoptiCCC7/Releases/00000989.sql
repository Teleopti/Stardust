if not exists(select * from Tenant.ServerConfiguration where [Key] = 'PreserveLogonAttemptsDays')
	insert into Tenant.ServerConfiguration VALUES ('PreserveLogonAttemptsDays', 30)