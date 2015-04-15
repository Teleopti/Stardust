declare @tenantId int
set @tenantId=1

truncate table tenant.personinfo
insert into tenant.personinfo (Id, TerminalDate, Tenant, [Identity], ApplicationLogonName, [Password])
select p.Id, p.TerminalDate, @tenantId, ai.[Identity], aai.ApplicationLogOnName, aai.[Password] from person p
left outer join AuthenticationInfo ai on ai.Person=p.Id
left outer join ApplicationAuthenticationInfo aai on aai.Person = p.Id
where IsDeleted=0

go
