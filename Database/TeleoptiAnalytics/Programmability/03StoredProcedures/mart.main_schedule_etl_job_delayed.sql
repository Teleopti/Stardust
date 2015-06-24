IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[main_schedule_etl_job_delayed]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[main_schedule_etl_job_delayed]
GO

--[mart].[main_schedule_etl_job_delayed] 15
CREATE PROCEDURE [mart].[main_schedule_etl_job_delayed]
@months_back int = 18
AS
SET NOCOUNT ON
DECLARE @stored_procedure nvarchar(300)
SET @stored_procedure=N'mart.main_convert_fact_schedule_ccc8_run'

--data convert already happened
IF EXISTS (select shift_startdate_local_id FROM mart.fact_schedule)
RETURN

--fact_schedule_old exists
IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'mart' AND  TABLE_NAME = 'fact_schedule_old'))
RETURN

--already added to delayed job
IF EXISTS (select 1 FROM mart.etl_job_delayed WHERE stored_procedured = @stored_procedure)
RETURN

--chunk up the insert in 1 month/run
DECLARE @start_date_id int
DECLARE @min_date_in_new_schedule int
DECLARE @min_date_old_schedule int 
DECLARE @today_id int
DECLARE @today_date smalldatetime

SELECT @today_date=DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE()))
SELECT @start_date_id =  date_id from mart.dim_date where date_date=dateadd(month,-@months_back, @today_date)
SELECT @today_id = date_id from mart.dim_date where date_date=@today_date
SELECT @min_date_old_schedule = isnull(min(schedule_date_id),@today_id) from mart.fact_schedule_old
SELECT @min_date_in_new_schedule = isnull(min(shift_startdate_local_id),@today_id) from mart.fact_schedule

IF @min_date_old_schedule>@start_date_id 
	SET @start_date_id=@min_date_old_schedule

--debug
--SELECT @min_date_old_schedule,@min_date_in_new_schedule,@start_date_id

CREATE TABLE #data(	id int IDENTITY(1,1) NOT NULL,
					parameter_string nvarchar(1000))

WHILE @start_date_id < @min_date_in_new_schedule+1
BEGIN
	INSERT #data(parameter_string)
	SELECT '@start_date_id=' + convert(nvarchar(10),@start_date_id) + ',@end_date_id=' + convert(nvarchar(10),@start_date_id+30) 

	SET @start_date_id=@start_date_id+31
END
SET NOCOUNT OFF
INSERT mart.etl_job_delayed( stored_procedured, parameter_string, insert_date)
SELECT  @stored_procedure, parameter_string, getdate()
FROM #data
ORDER BY id desc
GO
