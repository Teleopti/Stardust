alter table tenant.personinfo
add LastPasswordChange datetime null
alter table tenant.personinfo
add InvalidAttemptsSequenceStart datetime null
alter table tenant.personinfo
add IsLocked bit null
alter table tenant.personinfo
add InvalidAttempts int null

go

drop table Tenant.ApplicationLogonInfo