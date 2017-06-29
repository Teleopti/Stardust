IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[StaffingOvertimeSuggestions]') AND type in (N'P', N'PC'))
DROP PROCEDURE [StaffingOvertimeSuggestions]
GO
-- [StaffingOvertimeSuggestions] '90CA6478-5AAC-45F4-B909-A60400F34929, 120b9155-8962-4de5-89f0-a1500098a882, DAA1A1EC-1A93-470F-85B5-A14E00F48588', '2016-08-10 04:00:00', '2016-08-10 18:00:00', 500
CREATE PROCEDURE [StaffingOvertimeSuggestions]
@SkillIds nvarchar(max),
@StartDateTime datetime,
@EndDateTime datetime,
@numToReturn int = 10
WITH EXECUTE AS OWNER

AS
SET NOCOUNT ON
CREATE TABLE #ids(skillids uniqueidentifier)
INSERT INTO #ids SELECT * FROM SplitStringString(@SkillIds) 

	select top(@numToReturn) pss.PersonId, pss.[End], 
	CASE 
	WHEN DATEDIFF(minute,pss.[End],@EndDateTime) > 0
		THEN DATEDIFF(minute,pss.[End],@EndDateTime)
	ELSE 
		0
	END
	 +
	CASE 
	WHEN DATEDIFF(minute,@StartDateTime,pss.[Start]) > 0
		THEN DATEDIFF(minute,@StartDateTime,pss.[Start])
	ELSE 
		0
	END
	 AS TimeToAdd


 --,pss2.Start AS NextDayStarts, DATEDIFF(n,  pss.[End], pss2.Start) AS MinutesToNextShifts
 from [ReadModel].[PersonScheduleDay] pss
 --INNER JOIN [ReadModel].[ScheduleDay] sd on pss.PersonId = sd.PersonId AND pss.BelongsToDate = sd.BelongsToDate
 LEFT JOIN [ReadModel].[PersonScheduleDay] pss2 on pss.PersonId = pss2.PersonId AND pss.BelongsToDate +1 = pss2.BelongsToDate
 WHERE ((pss.[End] >= @StartDateTime AND pss.[End] < @EndDateTime) OR (pss.[Start] > @StartDateTime AND pss.[Start] <= @EndDateTime))
 AND pss.PersonId in(SELECT pp.parent FROM PersonPeriod pp INNER JOIN PersonSkill ps ON pp.Id = ps.Parent
  inner join #ids on #ids.skillids = ps.skill

 -- skip persons that have absence on that day, we need to be smarter to just skip if it absence next to the time where we want to add overtime
  INNER JOIN [ReadModel].[ScheduleProjectionReadOnly] sp
 ON pss.BelongsToDate = sp.BelongsToDate and pss.PersonId = sp.PersonId
 and sp.PayloadId NOT IN(SELECT Id FROM Absence)

 WHERE pp.StartDate <= CONVERT(date, @StartDateTime) AND pp.EndDate >= CONVERT(date, @StartDateTime))
 
  -- AND pss.personid = '9CEDEEA4-69CF-43E6-BAE2-A14100F34EA1'
   --AND sd.Workday = 1
  --order by TimeToAdd desc
 
RETURN

GO