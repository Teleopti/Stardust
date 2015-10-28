drop table dbo.DayOffSettings

create table dbo.DayOffRules (
	Id UNIQUEIDENTIFIER not null, 
	UpdatedBy UNIQUEIDENTIFIER null, 
	UpdatedOn DATETIME null,
	BusinessUnit uniqueidentifier not null,
	MinDayOffsPerWeek int not null,
	MaxDayOffsPerWeek int not null,
	MinConsecutiveWorkDays int not null,
	MaxConsecutiveWorkDays int not null,
	MinConsecutiveDayOffs int not null,
	MaxConsecutiveDayOffs int not null,
	DefaultSettings bit not null
)
alter table dbo.DayOffRules
add constraint PK_DayOffRules primary key clustered ([Id] asc)

alter table dbo.DayOffRules 
add constraint FK_DayOffRules_Person_UpdatedBy 
foreign key (UpdatedBy) references dbo.Person

alter table dbo.DayOffRules 
add constraint FK_DayOffRules_BusinessUnit
foreign key (BusinessUnit) references dbo.BusinessUnit


--insert into dbo.DayOffRules
--select newid(), '3f0886ab-7b25-4e95-856a-0d726edc2a67', getdate(), bu.Id, 1, 3, 2, 6, 1, 3, 1
--from dbo.BusinessUnit bu
--where bu.IsDeleted=0

