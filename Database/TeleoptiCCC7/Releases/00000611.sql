
CREATE TABLE [Tenant].[AdminUser](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[Password] [nvarchar](100) NOT NULL,
	[AccessToken]  [nvarchar](500)  NOT NULL,
 CONSTRAINT [PK_TenantUser] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
),
 CONSTRAINT [UQ_TenantUser_Email] UNIQUE NONCLUSTERED 
(
	[Email] ASC
)
)

GO

insert into Tenant.AdminUser (Name,Email, [Password],AccessToken)
values( 'Teleopti Tenant admin', 'admin@company.com', '###C1F2BF74658F78E6928A2E8A2426BE6360720989###', 'C1F2BF74658F78E6928A2E8A2426BE6360720989')

