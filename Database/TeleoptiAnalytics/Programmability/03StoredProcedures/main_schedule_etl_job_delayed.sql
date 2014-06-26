IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[main_schedule_etl_job_delayed]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[main_schedule_etl_job_delayed]
GO

--[mart].[main_schedule_etl_job_delayed] 15
CREATE PROCEDURE [mart].[main_schedule_etl_job_delayed]
@months_back int = 13
AS

--chunk up the insert in 1 month/run
--declare @months_back int
DECLARE @months_back_id int
DECLARE @min_date_in_new_schedule int
DECLARE @min_date_old_schedule int 
DECLARE @today_id int

SELECT @months_back_id =  date_id from mart.dim_date where date_date=dateadd(month,-@months_back, CONVERT(smalldatetime,CONVERT(nvarchar(30), getdate(), 112)) )
SELECT @today_id = date_id from mart.dim_date where date_date=CONVERT(smalldatetime,CONVERT(nvarchar(30), getdate(), 112)) 
SELECT @min_date_old_schedule = isnull(min(schedule_date_id),@today_id) from mart.fact_schedule_old
SELECT @min_date_in_new_schedule = isnull(min(schedule_date_id),@today_id) from mart.fact_schedule

IF @min_date_old_schedule>@months_back_id 
	SET @months_back_id=@min_date_old_schedule

--debug
--SELECT @min_date_old_schedule,@min_date_in_new_schedule,@months_back_id

CREATE TABLE #data(	id int IDENTITY(1,1) NOT NULL,
					parameter_string nvarchar(1000))

WHILE @months_back_id < @min_date_in_new_schedule
BEGIN
	INSERT #data(parameter_string)
	SELECT '@start_date_id=' + convert(nvarchar(10),@months_back_id) + ',@end_date_id=' + convert(nvarchar(10),@months_back_id+30) 

	SET @months_back_id=@months_back_id+31
END

INSERT mart.etl_job_delayed( stored_procedured, parameter_string, insert_date)
SELECT  'mart.main_convert_fact_schedule_ccc8_run', parameter_string, getdate()
FROM #data
ORDER BY id desc

SELECT * FROM mart.etl_job_delayed


GO