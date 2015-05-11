alter table tenant.personinfo
add TenantPassword nvarchar(50) null

go

update Tenant.PersonInfo
set TenantPassword = replace(newId(),'-','')

go

alter table tenant.personinfo
alter column TenantPassword nvarchar(50) not null
