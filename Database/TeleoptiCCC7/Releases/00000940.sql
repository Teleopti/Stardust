----------------  
--Name: Mingdi
--Date: 2018-01-30
--Desc: add more columns to identity businessUnit
----------------  

alter table dbo.ExternalPerformanceData
add BusinessUnit uniqueidentifier not null
alter table dbo.ExternalPerformanceData
add UpdatedBy uniqueidentifier not null
alter table dbo.ExternalPerformanceData
add UpdatedOn datetime not null
go

alter table dbo.ExternalPerformanceData
with check add constraint [FK_ExternalPerformanceData_BusinessUnit] foreign key ([BusinessUnit])
references dbo.BusinessUnit ([Id])
go

alter table dbo.ExternalPerformanceData
with check add constraint [FK_ExternalPerformanceData_UpdatedBy] foreign key ([UpdatedBy])
references dbo.Person ([Id])
go
