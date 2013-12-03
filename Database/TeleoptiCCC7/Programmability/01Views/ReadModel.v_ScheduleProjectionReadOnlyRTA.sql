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
WHERE BelongsToDate BETWEEN DATEADD(DAY,-2,GETUTCDATE()) AND DATEADD(DAY,1,GETUTCDATE())
-- 2013-12-03 ErikS: Use between -2 since BelongsToDate is always on 00:00 but DATEADD(...) retuns the full datetime
GO