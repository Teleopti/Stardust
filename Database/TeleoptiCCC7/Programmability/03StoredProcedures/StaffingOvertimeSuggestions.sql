IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[StaffingOvertimeSuggestions]') AND type in (N'P', N'PC'))
DROP PROCEDURE [StaffingOvertimeSuggestions]
GO

CREATE PROCEDURE [StaffingOvertimeSuggestions]
@SkillIds nvarchar(max),
@StartDateTime datetime,
@EndDateTime datetime
WITH EXECUTE AS OWNER

AS
SET NOCOUNT ON
CREATE TABLE #ids(skillids uniqueidentifier)
INSERT INTO #ids SELECT * FROM SplitStringString(@SkillIds) 

	select pss.PersonId, pss.[End], DATEDIFF(minute,pss.[End],@EndDateTime)   AS TimeToAdd
 --,pss2.Start AS NextDayStarts, DATEDIFF(n,  pss.[End], pss2.Start) AS MinutesToNextShifts
 from [ReadModel].[PersonScheduleDay] pss
 --INNER JOIN [ReadModel].[ScheduleDay] sd on pss.PersonId = sd.PersonId AND pss.BelongsToDate = sd.BelongsToDate
 LEFT JOIN [ReadModel].[PersonScheduleDay] pss2 on pss.PersonId = pss2.PersonId AND pss.BelongsToDate +1 = pss2.BelongsToDate
 WHERE ((pss.[End] >= @StartDateTime AND pss.[End] < @EndDateTime) OR (pss.[Start] > @StartDateTime AND pss.[Start] <= @EndDateTime))
 AND pss.PersonId in(SELECT pp.parent FROM
PersonPeriod pp
 INNER JOIN PersonSkill ps ON pp.Id = ps.Parent
 inner join #ids on #ids.skillids = ps.skill
 WHERE pp.StartDate <= CONVERT(date, @StartDateTime) AND pp.EndDate >= CONVERT(date, @StartDateTime))
 --AND sd.Workday = 1
 order by pss.Start desc

RETURN

GO