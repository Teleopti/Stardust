-- Agent Group table -> PlanningGroup
exec sp_rename 'AgentGroup', 'PlanningGroup'
go
exec sp_rename 'dbo.PK_AgentGroup', 'PK_PlanningGroup'
go
exec sp_rename 'dbo.FK_AgentGroup_BusinessUnit', 'FK_PlanningGroup_BusinessUnit'
go
exec sp_rename 'dbo.FK_AgentGroup_Person_UpdatedBy', 'FK_PlanningGroup_Person_UpdatedBy'
go

-- Agent Group Filters table -> PlanningGroupFilters
exec sp_rename 'AgentGroupFilters', 'PlanningGroupFilters'
go
exec sp_rename 'PlanningGroupFilters.AgentGroup', 'PlanningGroup', 'COLUMN'
go
exec sp_rename 'dbo.PK_AgentGroupFilters', 'PK_PlanningGroupFilters'
go
exec sp_rename 'dbo.FK_AgentGroupFilters_AgentGroup', 'FK_PlanningGroupFilters_PlanningGroup'
go

-- PlanningPeriod FK references
exec sp_rename 'PlanningPeriod.AgentGroup', 'PlanningGroup', 'COLUMN'
go
exec sp_rename 'dbo.FK_PlanningPeriod_AgentGroup', 'FK_PlanningPeriod_PlanningGroup'
go

-- DayOffRules FK references
exec sp_rename 'DayOffRules.AgentGroup', 'PlanningGroup', 'COLUMN'
go
exec sp_rename 'dbo.FK_DayOffRules_AgentGroup', 'FK_DayOffRules_PlanningGroup'
go