IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_queue_etl_job_delayed]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[fact_queue_etl_job_delayed]
GO

--[mart].[fact_queue_etl_job_delayed]
CREATE PROCEDURE [mart].[fact_queue_etl_job_delayed]

AS

--chunk up the insert by 6months/run
DECLARE @start_date datetime
DECLARE @max_date_agg datetime
DECLARE @chunkDays int
SET @chunkDays=180

SELECT @start_date = date_date
FROM mart.dim_date
WHERE date_id in (
	SELECT
	min(date_id)-1
	FROM mart.fact_queue_old --previous data
)

SELECT @max_date_agg = date_date --add an extra day
FROM mart.dim_date
WHERE date_date in 
(
	SELECT max(date_value)
	FROM mart.v_log_object_detail
	WHERE detail_id=2
)

CREATE TABLE #data(	id int IDENTITY(1,1) NOT NULL,
					parameter_string nvarchar(1000))

WHILE @start_date < @max_date_agg
BEGIN
	INSERT #data(parameter_string)
	SELECT '@start_date=''' + convert(nvarchar(10),@start_date,121) + ''',@end_date=''' + convert(nvarchar(10),dateadd(DAY,@chunkDays,@start_date),121) + ''',@datasource_id=-2'

	SET @start_date= dateadd(DAY,@chunkDays+1,@start_date)
END

INSERT mart.etl_job_delayed( stored_procedured, parameter_string, insert_date)
SELECT  'mart.etl_fact_queue_load', parameter_string, getdate()
FROM #data
ORDER BY id desc

GO

--truncate table mart.fact_queue
--run once upon deploy
IF (select count(*) FROM mart.fact_queue)=0
	EXEC [mart].[fact_queue_etl_job_delayed]
GO