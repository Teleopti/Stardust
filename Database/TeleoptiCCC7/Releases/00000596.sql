ALTER TABLE person DROP CONSTRAINT [DF_Tenant]
go
ALTER TABLE person DROP CONSTRAINT [FK_Person_Tenant]
go
alter table person drop column tenant
go
drop table dbo.Tenant
go
