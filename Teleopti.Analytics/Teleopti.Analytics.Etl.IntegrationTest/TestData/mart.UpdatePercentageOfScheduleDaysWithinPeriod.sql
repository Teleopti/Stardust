IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[UpdatePercentageOfScheduleDaysWithinPeriod]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[UpdatePercentageOfScheduleDaysWithinPeriod]
GO

-- =============================================
-- Author:		David
-- Create date: 2014-09-26
-- Description:	Updates parts of the Schedule in the readmodel so that ETL test will pickup new data 
-- =============================================
-- Date			Who	Description
-- =============================================
-- exec [mart].[UpdatePercentageOfScheduleDaysWithinPeriod] @percentage=0, @periodStart='2000-01-01', @periodEnd='2013-06-30'
CREATE PROCEDURE [mart].[UpdatePercentageOfScheduleDaysWithinPeriod]
@percentage int,
@periodStart datetime = NULL,
@periodEnd datetime  = NULL
AS

declare @rows int
declare @totRows int

--find number of schedule rows to update by percentage specified in SP call
SELECT
	@totRows = SUM(pa.rows)
FROM sys.tables ta
INNER JOIN sys.partitions pa
ON pa.OBJECT_ID = ta.OBJECT_ID
INNER JOIN sys.schemas sc
ON ta.schema_id = sc.schema_id
WHERE ta.is_ms_shipped = 0 AND pa.index_id IN (1,0)
AND sc.name='ReadModel'
AND ta.name='ScheduleDay'

select @rows = @totRows * @percentage/100

declare @LastUpdated	smalldatetime = '1999-12-31'
declare @EtlLastRun		smalldatetime = '2000-01-01'
declare @minStartPeriod	smalldatetime = '2001-01-02'
declare @maxStartPeriod	smalldatetime = '2020-12-31'

--====================
--reset
--====================
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

update dbo.AvailablePersonsInApplicationRole
set InsertedOn = @LastUpdated

update dbo.ApplicationFunctionInRole
set InsertedOn = @LastUpdated

update dbo.AvailableData
set UpdatedOn = @LastUpdated

--Etl Mother
update mart.LastUpdatedPerStep
set [Date] = @EtlLastRun

--====================
--set now() for period specified in SP call
--====================
--update selected rows
update s
set InsertedOn = GETUTCDATE()
FROM [ReadModel].[ScheduleDay] s
INNER JOIN 
(
select top (@rows)
	personId,
	BelongsToDate
	from [ReadModel].[ScheduleDay]
	WHERE BelongsToDate < IsNull(@periodEnd,@maxStartPeriod)
	AND BelongsToDate > IsNull(@periodStart,@minStartPeriod)
	order by NEWID()
) p
ON p.BelongsToDate = s.BelongsToDate
AND p.PersonId = s.PersonId

--finally
EXEC sp_executesql @statement=N'DBCC DROPCLEANBUFFERS'
GO