if (select count(*) from tenant.tenant) = 1
begin

update tenant.tenant 
set RtaKey = '!#¤atAbgT%'
where RtaKey = null

end
else
begin

update tenant.tenant 
set RtaKey = SUBSTRING(master.dbo.fn_varbintohexstr(HashBytes('SHA1', Name)), 3, 10)
where RtaKey = null

end
