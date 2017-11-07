CREATE TABLE [Tenant].[AdminAccessToken](
	[AdminUserId] [int] NOT NULL,
	[AccessToken] [varchar](30) NOT NULL,
	[Expires] [datetime] NOT NULL,
 CONSTRAINT [PK_Tenant.AdminAccessToken] PRIMARY KEY CLUSTERED 
(
	[AdminUserId] ASC,
	[AccessToken] ASC
))

ALTER TABLE [Tenant].[AdminAccessToken]  WITH CHECK ADD  CONSTRAINT [FK_AdminAccessToken_AdminUser] FOREIGN KEY([AdminUserId])
REFERENCES [Tenant].[AdminUser] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [Tenant].[AdminAccessToken] CHECK CONSTRAINT [FK_AdminAccessToken_AdminUser]
GO


