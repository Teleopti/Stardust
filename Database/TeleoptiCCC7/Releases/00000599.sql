alter table Tenant.Tenant
add ApplicationConnectionString nvarchar(500) null

alter table Tenant.Tenant
add AnalyticsConnectionString nvarchar(500) null

go

update Tenant.Tenant set
ApplicationConnectionString='',
AnalyticsConnectionString=''

alter table Tenant.Tenant
alter column ApplicationConnectionString nvarchar(500) not null

alter table Tenant.Tenant
alter column AnalyticsConnectionString nvarchar(500) not null
