alter table dbo.DayOffRules
add Name nvarchar(100) null
Go

update dbo.DayOffRules
set Name = ''

alter table dbo.DayOffRules
alter column Name nvarchar(100) not null


