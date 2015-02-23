CREATE TABLE [dbo].[Tenant](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL
) ON [PRIMARY]
alter table dbo.Tenant add constraint PK_Tenant primary key clustered
(
 Id asc
)
alter table dbo.Tenant add constraint UQ_Tenant_Name unique nonclustered
(
 Name asc
)
GO

insert into dbo.Tenant (Name) values ('Teleopti WFM')

alter table [dbo].[Person]
add Tenant int null
GO

update dbo.Person set Tenant=1

alter table dbo.Person
alter column Tenant int not null

alter table dbo.Person add constraint FK_Person_Tenant
foreign key (Tenant) references Tenant

alter table dbo.Person add constraint DF_Tenant
default 1 FOR Tenant

GO
