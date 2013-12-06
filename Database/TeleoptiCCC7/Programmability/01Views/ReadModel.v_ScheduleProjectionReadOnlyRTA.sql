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
WHERE BelongsToDate
	BETWEEN
		DATEADD(
			DAY,-1, --yesterday
			DATEDIFF(DAY, 0, GETUTCDATE()) --Now().DateOnly
			)
		AND
		DATEADD(
			DAY,1, --tomorrow
			DATEDIFF(DAY, 0, GETUTCDATE()) --Now().DateOnly
			)
GO