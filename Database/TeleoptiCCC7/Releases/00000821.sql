create table dbo.PlanningPeriodJobResult(
	PlanningPeriod UNIQUEIDENTIFIER not null, 
	JobResult UNIQUEIDENTIFIER not null
)

alter table dbo.PlanningPeriodJobResult 
add constraint FK_PlanningPeriodJobResult_PlanningPeriod
foreign key (PlanningPeriod) references dbo.PlanningPeriod

alter table dbo.PlanningPeriodJobResult 
add constraint FK_PlanningPeriodJobResult_JobResult
foreign key (JobResult) references dbo.JobResult