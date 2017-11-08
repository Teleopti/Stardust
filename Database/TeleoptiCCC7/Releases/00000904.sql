ALTER TABLE Tenant.AdminAccessToken ALTER COLUMN [AccessToken] [varchar](100) NOT NULL
GO
ALTER TABLE Tenant.AdminUser DROP COLUMN AccessToken
GO