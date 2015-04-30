alter table [Tenant].[Tenant] ADD CONSTRAINT UQ_Tenant_Name UNIQUE NONCLUSTERED 
(
	[Name] ASC
)
