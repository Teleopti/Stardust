create table Tenant.TenantApplicationNhibernateConfig
(
	TenantId int not null,
	ConfigKey nvarchar(255) not null,
	ConfigValue nvarchar(255) not null
)
go
alter table Tenant.TenantApplicationNhibernateConfig add constraint PK_TenantApplicationNhibernateConfig primary key clustered 
(
	TenantId asc,
	ConfigKey asc
)
