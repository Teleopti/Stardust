CREATE TABLE [Tenant].[Tenant](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	Name nvarchar(255) not null
)
go 

alter table [Tenant].[Tenant]
add constraint PK_Tenant
primary key clustered
(
  [Id] asc
)
go

CREATE TABLE [Tenant].[PersonInfo](
	[Id] [uniqueidentifier] NOT NULL,
	TerminalDate datetime null,
	Tenant int not null,
	[Identity] nvarchar(100) null,
	ApplicationLogonName nvarchar(50) null,
	Password nvarchar(50) null)
go

alter table [Tenant].[PersonInfo]
add constraint PK_PersonInfo
primary key clustered
(
  [Id] asc
)
go

alter table [Tenant].[PersonInfo]
add constraint FK_PersonInfo_Tenant
foreign key (Tenant) references Tenant.Tenant
go

create unique index UQ_PersonInfo_ApplicationLogonName
on Tenant.PersonInfo (ApplicationLogonName)
where ApplicationLogonName is not null
go


create unique index UQ_PersonInfo_Identity
on Tenant.PersonInfo ([Identity])
where [Identity] is not null
go
