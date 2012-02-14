/* 
BuildTime is: 
2008-11-19 
16:38
*/ 
/* 
Trunk initiated: 
2008-11-19 
11:28
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Klas Mellbourn
--Date: 2008-11-19  
--Desc: Table containing license  
----------------  
create table dbo.License (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, XmlString NVARCHAR(255) not null)
go
ALTER TABLE [dbo].[License] ADD CONSTRAINT PK_License PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)

go
alter table dbo.License add constraint FK_License_Person_CreatedBy foreign key (CreatedBy) references Person
go
alter table dbo.License add constraint FK_License_Person_UpdatedBy foreign key (UpdatedBy) references Person
go

-------------------  
--Name: Sumeda Herah
--Date: 2008-11-19  
--Desc: move start and end time columns to request part and drop from request part tables
------------------- 

alter table dbo.AbsenceRequest  drop column StartDateTime
go
alter table dbo.AbsenceRequest drop column EndDateTime
go
alter table dbo.ShiftTradeRequest drop column StartDateTime
go
alter table dbo.ShiftTradeRequest drop column EndDateTime
go
alter table dbo.TextRequest drop column StartDateTime
go
alter table dbo.TextRequest drop column EndDateTime 
go
alter table dbo.RequestPart add StartDateTime  datetime not null
go
alter table dbo.RequestPart add EndDateTime datetime not null
go
PRINT 'Adding build number to database' 
INSERT INTO dbo.DatabaseVersion(BuildNumber, SystemVersion) VALUES (12,'7.0.12') 
