declare @nu datetime
set @nu = GETUTCDATE()

update tenant.personinfo set 
LastPasswordChange = @nu,
InvalidAttemptsSequenceStart = @nu,
IsLocked = 0,
InvalidAttempts = 0

alter table tenant.personinfo
alter column LastPasswordChange datetime not null

go

alter table tenant.personinfo
alter column InvalidAttemptsSequenceStart datetime not null

go

alter table tenant.personinfo
alter column IsLocked bit not null

go

alter table tenant.personinfo
alter column InvalidAttempts int not null

