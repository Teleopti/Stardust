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
-- exec [mart].[UpdatePercentageOfScheduleDaysWithinPeriod] @percentage=1, @periodStart='2000-01-01', @periodEnd='2013-06-30'
CREATE PROCEDURE [mart].[UpdatePercentageOfScheduleDaysWithinPeriod]
@percentage int,
@periodStart datetime = NULL,
@periodEnd datetime  = NULL
AS

declare @rows int
declare @totRows int

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

update s
set InsertedOn = GETUTCDATE()
FROM [ReadModel].[ScheduleDay] s
INNER JOIN 
(
select top (@rows)
	personId,
	BelongsToDate
	from [ReadModel].[ScheduleDay]
	WHERE BelongsToDate < IsNull(@periodEnd,'2020-12-31')
	AND BelongsToDate > IsNull(@periodStart,'2000-01-01')
	order by NEWID()
) p
ON p.BelongsToDate = s.BelongsToDate
AND p.PersonId = s.PersonId

GO