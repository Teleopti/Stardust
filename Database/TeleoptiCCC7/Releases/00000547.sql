--needed to set this explicitly because in azure script it has set to off before
--and it need to be turned on to get filtered index to work.
--After discussion with Johan, we decided to leave it on (not end this script setting it to off)
--because it's the recommended Microsoft setting anyway.
set ansi_padding on

create unique index UQ_PersonInfo_ApplicationLogonName
on Tenant.PersonInfo (ApplicationLogonName)
where ApplicationLogonName is not null
go


create unique index UQ_PersonInfo_Identity
on Tenant.PersonInfo ([Identity])
where [Identity] is not null
go
