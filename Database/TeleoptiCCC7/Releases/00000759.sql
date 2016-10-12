
DELETE FROM [Tenant].[TenantApplicationNhibernateConfig]
WHERE TenantId NOT IN (SELECT Id FROM Tenant.Tenant)
GO

ALTER TABLE [Tenant].[TenantApplicationNhibernateConfig]  WITH CHECK ADD  CONSTRAINT [FK_TenantApplicationNhibernateConfig_Tenant] FOREIGN KEY([TenantId])
REFERENCES [Tenant].[Tenant] ([Id])
ON DELETE CASCADE
GO




