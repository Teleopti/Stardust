ALTER TABLE [dbo].[PlanningGroupSettings] ADD MinFullWeekendsOff int not null default 0;
ALTER TABLE [dbo].[PlanningGroupSettings] ADD MaxFullWeekendsOff int not null default 8;
ALTER TABLE [dbo].[PlanningGroupSettings] ADD MinWeekendDaysOff int not null default 0;
ALTER TABLE [dbo].[PlanningGroupSettings] ADD MaxWeekendDaysOff int not null default 16;