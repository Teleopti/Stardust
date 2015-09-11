if (select count(*) from tenant.tenant) = 1
begin

update tenant.tenant 
set RtaKey = '!#¤atAbgT%'
where RtaKey = null

end
else
begin

update tenant.tenant 
set RtaKey = LOWER(SUBSTRING(convert(nvarchar, cast(HashBytes('SHA1', Name) as varbinary),1), 3, 10))
where RtaKey = null

end
