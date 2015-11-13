-- =============================================
-- Author: Xinfeng
-- Create date: 2015-11-12
-- Description: This sp implemented load person schedule include over night shift.
--              @take means return how many person's schedule, so when you set @take=2, it may return 2-4 records
--              since over night shift will returned as a related record.
--              You need merge the schedule by yourself
-- Example:
--     EXEC [ReadModel].[LoadPersonScheduleIncludeOverNightShift]
--          @PersonIds= '11610FE4-0130-4568-97DE-9B5E015B2564,826F2A46-93BB-4B04-8D5E-9B5E015B2577',
--          @scheduleDate='2015-11-12',
--          @dateStart = '2015-11-11 23:00:00',
--          @dateEnd = '2015-11-12 23:00:00',
--          @skip = 1,
--          @take = 2
-- =============================================

CREATE PROCEDURE [ReadModel].[LoadPersonScheduleIncludeOverNightShift]
    @PersonIds nvarchar(max),
    @scheduleDate DateTime,
    @dateStart datetime,
    @dateEnd datetime,
    @skip int,
    @take int
AS
BEGIN
DECLARE @PersonIdList table
(
    PersonId uniqueidentifier
)

INSERT INTO @PersonIdList
SELECT * FROM dbo.SplitStringString(@PersonIds)

CREATE TABLE #PersonScheduleInfo (
    PersonId uniqueidentifier,
    ScheduleStartInMinute smallint,
    ScheduleEndInMinute smallint,
    IsDayOff bit,
    IsFullDayAbsence bit,
    IsEmptySchedule bit,
    SortValue smallint,
)

INSERT INTO #PersonScheduleInfo
SELECT DISTINCT
       p.PersonId AS PersonId,
       DATEDIFF(MINUTE, @scheduleDate, psd.Start) AS ScheduleStartInMinute,
       DATEDIFF(MINUTE, @scheduleDate, psd.[End]) AS ScheduleEndInMinute,
       psd.IsDayOff,
       CASE WHEN Start IS NOT NULL AND pa.Id IS NOT NULL THEN 1 ELSE 0 END AS IsFullDayAbsence,
       CASE WHEN Start IS NULL THEN 1 ELSE 0 END AS IsEmptySchedule,
       0 AS SortValue
  FROM @PersonIdList p
  JOIN dbo.Person person ON p.PersonId = person.Id
  LEFT JOIN ReadModel.PersonScheduleDay AS psd ON psd.PersonId = p.PersonId AND psd.BelongsToDate = @scheduleDate
  LEFT JOIN dbo.PersonAbsence pa ON p.PersonId = pa.Person AND psd.Start >= pa.Minimum AND psd.[End] <= pa.Maximum

UPDATE #PersonScheduleInfo
   SET SortValue = CASE
      WHEN IsEmptySchedule = 1 THEN 20000
      WHEN IsDayOff = 1 THEN 10000
      WHEN IsFullDayAbsence = 1 THEN 5000 + ScheduleStartInMinute
      ELSE ScheduleStartInMinute
   END

SELECT s.*, psd.PersonId, TeamId, SiteId, BusinessUnitId, psd.BelongsToDate AS Date, Start, [End], Model
  FROM ReadModel.PersonScheduleDay psd
 RIGHT JOIN (SELECT *, ROW_NUMBER() OVER (ORDER BY SortValue) AS RowNum FROM #PersonScheduleInfo) AS s
    ON s.PersonId = psd.PersonId
 WHERE Start IS NOT NULL
   AND Start < @dateEnd
   AND [End] > @dateStart
   AND s.RowNum > @skip
   AND s.RowNum <= @skip + @take
 ORDER BY psd.BelongsToDate DESC, s.SortValue ASC

END
