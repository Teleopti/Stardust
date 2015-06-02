declare @tenantId int
set @tenantId=1

truncate table tenant.personinfo
insert into tenant.personinfo (Id, Tenant, [Identity], ApplicationLogonName, ApplicationLogonPassword, TenantPassword)
select p.Id, @tenantId, ai.[Identity], aai.ApplicationLogOnName, aai.[Password], replace(newId(),'-','') from person p
left outer join AuthenticationInfo ai on ai.Person=p.Id
left outer join ApplicationAuthenticationInfo aai on aai.Person = p.Id
where IsDeleted=0 and (ai.[Identity] is not null or aai.ApplicationLogOnName is not null)

drop table AuthenticationInfo
drop table ApplicationAuthenticationInfo
drop table UserDetail
