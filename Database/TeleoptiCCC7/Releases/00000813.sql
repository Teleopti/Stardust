alter table dbo.PlanningPeriod add AgentGroup uniqueidentifier null
 
alter table dbo.PlanningPeriod 
add constraint FK_PlanningPeriod_AgentGroup
foreign key (AgentGroup) references dbo.AgentGroup