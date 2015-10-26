create table dbo.DayOffSettings (
	Id UNIQUEIDENTIFIER not null, 
	UpdatedBy UNIQUEIDENTIFIER null, 
	UpdatedOn DATETIME null, 
	MinDayOffsPerWeek int not null,
	MaxDayOffsPerWeek int not null,
	MinConsecutiveWorkDays int not null,
	MaxConsecutiveWorkDays int not null,
	MinConsecutiveDayOffs int not null,
	MaxConsecutiveDayOffs int not null,
	DefaultSettings bit not null
)
alter table dbo.DayOffSettings
add constraint PK_DayOffSettings primary key clustered ([Id] asc)

alter table dbo.DayOffSettings 
add constraint FK_DayOffSettings_Person_UpdatedBy 
foreign key (UpdatedBy) references dbo.Person

go


insert into dbo.DayOffSettings
select newid(), '3f0886ab-7b25-4e95-856a-0d726edc2a67', getdate(), 1, 3, 2, 6, 1, 3, 1

