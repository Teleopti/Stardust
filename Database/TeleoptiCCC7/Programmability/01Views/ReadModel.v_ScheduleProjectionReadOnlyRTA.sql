IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[ReadModel].[v_ScheduleProjectionReadOnlyRTA]'))
DROP VIEW [ReadModel].[v_ScheduleProjectionReadOnlyRTA]
GO
CREATE VIEW [ReadModel].[v_ScheduleProjectionReadOnlyRTA]
AS
SELECT [Id]
      ,[ScenarioId]
      ,[PersonId]
      ,[BelongsToDate]
      ,[PayloadId]
      ,[StartDateTime]
      ,[EndDateTime]
      ,[WorkTime]
      ,[Name]
      ,[ShortName]
      ,[DisplayColor]
FROM [ReadModel].[ScheduleProjectionReadOnly]
WHERE BelongsToDate between dateadd(day,-1,GETUTCDATE()) and dateadd(day,3,GETUTCDATE())

GO
