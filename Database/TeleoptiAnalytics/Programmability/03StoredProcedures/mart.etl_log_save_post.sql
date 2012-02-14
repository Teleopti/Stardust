IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_log_save_post]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_log_save_post]
GO


CREATE PROCEDURE [mart].[etl_log_save_post]
  
	@job_execution_id int,
	@job_name varchar(50),
	@schedule_id int,
	@business_unit_code uniqueidentifier,
	@business_unit_name nvarchar(100),
	@start_datetime datetime,
	@end_datetime datetime,
	@duration float,
	@affected_rows int,
	@error_msg varchar(200)
  
AS
BEGIN

	SET NOCOUNT ON;

	DECLARE @job_id as int

	SELECT @job_id=job_id FROM Mart.etl_job WHERE job_name= @job_name


    UPDATE Mart.etl_job_execution
	SET job_id = @job_id,
		schedule_id = @schedule_id, 
		business_unit_code = @business_unit_code,
		business_unit_name = @business_unit_name,
        job_start_time = @start_datetime,
		job_end_time = @end_datetime,
		duration_s = @duration,
		affected_rows = @affected_rows
	WHERE (job_execution_id = @job_execution_id)

	SELECT @job_id

END


GO

