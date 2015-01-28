IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[UpdateRowsOfSkillDaysWithinPeriod]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[UpdateRowsOfSkillDaysWithinPeriod]
GO

-- =============================================
-- exec [mart].[UpdateRowsOfSkillDaysWithinPeriod] @rows=100, @periodStart='2014-08-01', @periodEnd='2014-09-01',@reset=1
-- =============================================
CREATE PROCEDURE [mart].[UpdateRowsOfSkillDaysWithinPeriod]
@rows int,
@periodStart datetime = NULL,
@periodEnd datetime  = NULL,
@reset bit
AS

declare @LastUpdated	smalldatetime = '1999-12-31'
declare @EtlLastRun		smalldatetime = '2000-01-01'
declare @minStartPeriod	smalldatetime = '2001-01-02'
declare @maxStartPeriod	smalldatetime = '2020-12-31'

--====================
--set now() for period specified in SP call
--====================
--update selected rows
update sd
set UpdatedOn = GETUTCDATE()
FROM dbo.SkillDay sd
INNER JOIN 
(
select top (@rows)
	sd.Id
	from dbo.SkillDay sd
	inner join dbo.Scenario s
		on s.Id = sd.Scenario
	WHERE sd.SkillDayDate BETWEEN IsNull(@periodStart,@minStartPeriod) AND IsNull(@periodEnd,@maxStartPeriod)
	AND s.DefaultScenario = 1
	order by NEWID()
) p
ON p.Id = sd.Id
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[UpdateRowsOfScheduleDaysWithinPeriod]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[UpdateRowsOfScheduleDaysWithinPeriod]
GO

-- =============================================
-- exec [mart].[UpdateRowsOfScheduleDaysWithinPeriod] @rows=100, @periodStart='2000-01-01', @periodEnd='2013-06-30',@reset=1
-- =============================================
CREATE PROCEDURE [mart].[UpdateRowsOfScheduleDaysWithinPeriod]
@rows int,
@periodStart datetime = NULL,
@periodEnd datetime  = NULL,
@reset bit
AS

--reset?
if @reset = 1
	exec [mart].[resetIntradayTables]

declare @LastUpdated	smalldatetime = '1999-12-31'
declare @EtlLastRun		smalldatetime = '2000-01-01'
declare @minStartPeriod	smalldatetime = '2001-01-02'
declare @maxStartPeriod	smalldatetime = '2020-12-31'

--====================
--set now() for period specified in SP call
--====================
--update selected rows
SET NOCOUNT OFF
update s
set InsertedOn = GETUTCDATE()
FROM [ReadModel].[ScheduleDay] s
INNER JOIN 
(
select top (@rows)
	personId,
	BelongsToDate
	from [ReadModel].[ScheduleDay]
	WHERE BelongsToDate BETWEEN IsNull(@periodStart,@minStartPeriod) AND IsNull(@periodEnd,@maxStartPeriod)
	order by NEWID()
) p
ON p.BelongsToDate = s.BelongsToDate
AND p.PersonId = s.PersonId
GO

--====================================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[UpdateRowsOfScheduleDaysWithinPeriod]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[resetIntradayTables]
GO
CREATE PROCEDURE [mart].[resetIntradayTables]
AS
SET NOCOUNT ON
declare @LastUpdated	smalldatetime = '1999-12-31'
declare @EtlLastRun		smalldatetime = '2000-01-01'
declare @minStartPeriod	smalldatetime = '2001-01-02'
declare @maxStartPeriod	smalldatetime = '2020-12-31'

--update all tables we check via ETL
update ReadModel.ScheduleDay
set InsertedOn = @LastUpdated

update dbo.PreferenceDay
set UpdatedOn = @LastUpdated

update dbo.StudentAvailabilityDay
set UpdatedOn = @LastUpdated

update dbo.SkillDay
set UpdatedOn = @LastUpdated

update dbo.PersonRequest
set UpdatedOn = @LastUpdated

update dbo.PersonInApplicationRole
set InsertedOn = @LastUpdated

update dbo.ApplicationFunctionInRole
set InsertedOn = @LastUpdated

update dbo.AvailableData
set UpdatedOn = @LastUpdated

--Etl Mother
update mart.LastUpdatedPerStep
set [Date] = @EtlLastRun
