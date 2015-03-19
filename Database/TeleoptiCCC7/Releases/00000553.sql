update dbo.person set isdeleted=1 where builtin=1
go

alter table dbo.person
drop column builtin
go

