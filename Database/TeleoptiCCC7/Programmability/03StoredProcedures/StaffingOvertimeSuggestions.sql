IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[StaffingOvertimeSuggestions]') AND type in (N'P', N'PC'))
DROP PROCEDURE [StaffingOvertimeSuggestions]
GO
-- [StaffingOvertimeSuggestions] '90CA6478-5AAC-45F4-B909-A60400F34929, 120b9155-8962-4de5-89f0-a1500098a882, DAA1A1EC-1A93-470F-85B5-A14E00F48588', '2016-08-10 04:00:00', '2016-08-10 18:00:00', 'E0D49526-3CCB-4A17-B7D2-A142010BBDB4', 5000

CREATE PROCEDURE [StaffingOvertimeSuggestions]
@SkillIds nvarchar(max),
@StartDateTime datetime,
@EndDateTime datetime,
@multiplikatorDefSet uniqueidentifier,
@numToReturn int = 10
WITH EXECUTE AS OWNER

AS
SET NOCOUNT ON
CREATE TABLE #ids(skillids uniqueidentifier)
INSERT INTO #ids SELECT * FROM SplitStringString(@SkillIds) 


CREATE TABLE #withAbsence (pid uniqueidentifier, startAbsence uniqueidentifier, endAbsence uniqueidentifier )

 
 INSERT INTO #withAbsence 
  select sp2.PersonId, sp2.PayloadId, null FROM [ReadModel].[PersonScheduleDay] sp
 INNER JOIN  [ReadModel].[ScheduleProjectionReadOnly] sp2 ON sp.BelongsToDate = sp2.BelongsToDate and sp.PersonId = sp2.PersonId
 AND sp2.StartDateTime = sp.Start
  WHERE ((sp.[End] >= @StartDateTime AND sp.[End] < @EndDateTime) OR (sp.[Start] > @StartDateTime AND sp.[Start] <= @EndDateTime))
 and sp2.PayloadId IN(SELECT Id FROM Absence)
  ORDER BY sp2.PersonId

  update #withAbsence 
  set endAbsence =  sp2.PayloadId FROM [ReadModel].[PersonScheduleDay] sp
  INNER JOIN #withAbsence ON #withAbsence.pid = sp.PersonId
 INNER JOIN  [ReadModel].[ScheduleProjectionReadOnly] sp2 ON sp.BelongsToDate = sp2.BelongsToDate and sp.PersonId = sp2.PersonId
 AND sp2.EndDateTime = sp.[End]
  WHERE ((sp.[End] >= @StartDateTime AND sp.[End] < @EndDateTime) OR (sp.[Start] > @StartDateTime AND sp.[Start] <= @EndDateTime))
 and sp2.PayloadId IN(SELECT Id FROM Absence)
  
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
 from [ReadModel].[PersonScheduleDay] pss
 --INNER JOIN [ReadModel].[ScheduleDay] sd on pss.PersonId = sd.PersonId AND pss.BelongsToDate = sd.BelongsToDate
 LEFT JOIN [ReadModel].[PersonScheduleDay] pss2 on pss.PersonId = pss2.PersonId AND pss.BelongsToDate +1 = pss2.BelongsToDate
 WHERE ((pss.[End] >= @StartDateTime AND pss.[End] < @EndDateTime) OR (pss.[Start] > @StartDateTime AND pss.[Start] <= @EndDateTime))
 AND pss.PersonId in(SELECT pp.parent FROM PersonPeriod pp INNER JOIN PersonSkill ps ON pp.Id = ps.Parent
  inner join #ids on #ids.skillids = ps.skill
 INNER JOIN MultiplicatorDefinitionSetCollection mdsc on pp.[Contract] = mdsc.[Contract] AND mdsc.MultiplicatorDefinitionSet = @multiplikatorDefSet
 
 WHERE pp.StartDate <= CONVERT(date, @StartDateTime) AND pp.EndDate >= CONVERT(date, @StartDateTime))
 -- skip persons that have absence on that day, we need to be smarter to just skip if it absence next to the time where we want to add overtime
 AND pss.PersonId not in (SELECT pid FROM #withAbsence WHERE endAbsence IS NOT NULL)

   --AND sd.Workday = 1
  --order by TimeToAdd desc
  
  
 DROP TABLE #withAbsence
RETURN

GO