create table dbo.AgentGroup (
	Id UNIQUEIDENTIFIER not null, 
	UpdatedBy UNIQUEIDENTIFIER null, 
	UpdatedOn DATETIME null,
	BusinessUnit uniqueidentifier not null,
	Name nvarchar(100) not null
)
alter table dbo.AgentGroup
add constraint PK_AgentGroup primary key clustered ([Id] asc)

alter table dbo.AgentGroup 
add constraint FK_AgentGroup_Person_UpdatedBy 
foreign key (UpdatedBy) references dbo.Person

alter table dbo.AgentGroup 
add constraint FK_AgentGroup_BusinessUnit
foreign key (BusinessUnit) references dbo.BusinessUnit


create table dbo.AgentGroupFilters (
  AgentGroup uniqueidentifier not null,
  FilterType varchar(50) not null,
  Filter uniqueidentifier not null
)
alter table dbo.AgentGroupFilters ADD CONSTRAINT PK_AgentGroupFilters PRIMARY KEY CLUSTERED 
(
	AgentGroup,
	FilterType,
	Filter
)
alter table dbo.AgentGroupFilters 
add constraint FK_AgentGroupFilters_AgentGroup
foreign key (AgentGroup) references dbo.AgentGroup


