exec sp_rename 'DayOffRulesFilters', 'PlanningGroupSettingsFilters';
GO
exec sp_rename 'PlanningGroupSettingsFilters.DayOffRules', 'PlanningGroupSettings', 'COLUMN'
go
exec sp_rename 'dbo.PK_DayOffRulesFilters', 'PK_PlanningGroupSettingsFilters'
go
exec sp_rename 'dbo.FK_DayOffRulesFilters_DayOffRules', 'FK_PlanningGroupSettingsFilters_PlanningGroupSettings'
go


exec sp_rename 'DayOffRules', 'PlanningGroupSettings';
GO
exec sp_rename 'dbo.PK_DayOffRules', 'PK_PlanningGroupSettings'
go
exec sp_rename 'dbo.FK_DayOffRules_BusinessUnit', 'FK_PlanningGroupSettings_BusinessUnit'
go
exec sp_rename 'dbo.FK_DayOffRules_Person_UpdatedBy', 'FK_PlanningGroupSettings_Person_UpdatedBy'
go
exec sp_rename 'dbo.FK_DayOffRules_PlanningGroup', 'FK_PlanningGroupSettings_PlanningGroup'
go
exec sp_rename 'PlanningGroupSettings.UQ_DayOffRules_DefaultSettings', 'UQ_PlanningGroupSettings_DefaultSettings', 'INDEX'
go


ALTER TABLE [dbo].[PlanningGroupSettings] ADD BlockFinderType int not null default 0;

ALTER TABLE [dbo].[PlanningGroupSettings] ADD BlockSameStartTime bit not null default 0;

ALTER TABLE [dbo].[PlanningGroupSettings] ADD BlockSameShiftCategory bit not null default 0;

ALTER TABLE [dbo].[PlanningGroupSettings] ADD BlockSameShift bit not null default 0;

GO
