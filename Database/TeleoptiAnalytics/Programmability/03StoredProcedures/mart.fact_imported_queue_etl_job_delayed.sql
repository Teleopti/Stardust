IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_imported_queue_etl_job_delayed]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[fact_imported_queue_etl_job_delayed]
GO

--[mart].[fact_imported_queue_etl_job_delayed]
CREATE PROCEDURE [mart].[fact_imported_queue_etl_job_delayed]

AS
SET NOCOUNT ON
DECLARE @stored_procedure nvarchar(300)
SET @stored_procedure=N'mart.etl_imported_queues_fact_queue_reload'

CREATE TABLE #queues(queue_id int)
INSERT #queues
SELECT queue_id 
FROM mart.dim_queue q 
WHERE log_object_name='Teleopti CCC - File import'
and datasource_id=1

--already added to delayed job
IF EXISTS (select 1 FROM mart.etl_job_delayed WHERE stored_procedured = @stored_procedure)
RETURN

--chunk up the insert by 6months/run
DECLARE @start_date_id int
DECLARE @max_date_id int
DECLARE @start_date datetime
DECLARE @max_date datetime
DECLARE @chunkDays int
SET @chunkDays=180

SELECT @start_date_id = MIN(date_id)-1
	FROM mart.fact_queue_old 
	WHERE queue_id in (SELECT queue_id FROM #queues)--previous data

SELECT @start_date = date_date FROM mart.dim_date WHERE date_id= @start_date_id

SELECT @max_date_id = MAX(date_id)
	FROM mart.fact_queue_old 
	WHERE queue_id in (SELECT queue_id FROM #queues)--previous data


SELECT @max_date =  date_date FROM mart.dim_date WHERE date_id = @max_date_id

--data convert already happened-->exit
IF EXISTS (select date_id FROM mart.fact_queue where queue_id in (select queue_id from #queues) AND date_id between @start_date_id and @max_date_id)
RETURN


CREATE TABLE #data(	id int IDENTITY(1,1) NOT NULL,
					parameter_string nvarchar(1000))

WHILE @start_date < @max_date
BEGIN
	INSERT #data(parameter_string)
	SELECT '@start_date=''' + convert(nvarchar(10),@start_date,121) + ''',@end_date=''' + convert(nvarchar(10),dateadd(DAY,@chunkDays,@start_date),121) + ''''

	SET @start_date= dateadd(DAY,@chunkDays+1,@start_date)
END
SET NOCOUNT OFF
INSERT mart.etl_job_delayed( stored_procedured, parameter_string, insert_date)
SELECT  @stored_procedure, parameter_string, getdate()
FROM #data
ORDER BY id desc

GO