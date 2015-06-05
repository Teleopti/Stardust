if not exists (select 1 from PurgeSetting where [Key]='DenyPendingRequestsAfterNDays')
	INSERT INTO [PurgeSetting] ([Key],[Value]) VALUES ('DenyPendingRequestsAfterNDays',14)