create table dbo.ContractFilter (
  Id uniqueidentifier not null,
  Contract uniqueidentifier not null
)
alter table dbo.ContractFilter ADD CONSTRAINT PK_ContractFilter PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
alter table dbo.ContractFilter 
add constraint FK_ContractFilter_Contract
foreign key (Contract) references dbo.Contract


create table dbo.TeamFilter (
  Id uniqueidentifier not null,
  Team uniqueidentifier not null
)
alter table dbo.TeamFilter ADD CONSTRAINT PK_TeamFilter PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
alter table dbo.TeamFilter 
add constraint FK_TeamFilter_Team
foreign key (Team) references dbo.Team

create table dbo.SiteFilter (
  Id uniqueidentifier not null,
  Site uniqueidentifier not null
)
alter table dbo.SiteFilter ADD CONSTRAINT PK_SiteFilter PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
alter table dbo.SiteFilter 
add constraint FK_SiteFilter_Team
foreign key (Site) references dbo.Site



create table dbo.DayOffRulesFilters (
  DayOffRules uniqueidentifier not null,
  FilterType varchar(50) not null,
  Filter uniqueidentifier not null
)
alter table dbo.DayOffRulesFilters ADD CONSTRAINT PK_DayOffRulesFilters PRIMARY KEY CLUSTERED 
(
	DayOffRules,
	FilterType,
	Filter
)
alter table dbo.DayOffRulesFilters 
add constraint FK_DayOffRulesFilters_DayOffRules
foreign key (DayOffRules) references dbo.DayOffRules