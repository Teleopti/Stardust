IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[GetNextActivityStartTime]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[GetNextActivityStartTime]
GO

-- exec sp_executesql N'exec ReadModel.GetNextActivityStartTime @PersonId=@p0, @UtcNow=@p1',N'@p0 uniqueidentifier,@p1 datetime',@p0='11610FE4-0130-4568-97DE-9B5E015B2564',@p1='2013-10-01 08:08:18'
CREATE PROC ReadModel.GetNextActivityStartTime
@PersonId uniqueidentifier,
@UtcNow datetime
AS

SELECT TOP 1 StartDateTime,EndDateTime
FROM [ReadModel].[ScheduleProjectionReadOnly]
WHERE EndDateTime > @UtcNow
AND PersonID = @personId
ORDER BY StartDateTime