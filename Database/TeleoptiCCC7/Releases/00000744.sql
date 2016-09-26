ALTER TABLE [ReadModel].[ScheduleForecastSkill]
ADD CalculatedOn [datetime] 

GO

update [ReadModel].[ScheduleForecastSkill]
set CalculatedOn = GETDATE()

