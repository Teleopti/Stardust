IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_log_save]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_log_save]
GO


--exec dbo.etl_log_save @schedule_id=-1,@start_datetime='2008-08-06 11:20:39:837',@end_datetime='2008-08-06 11:20:39:837',@name=N'fact_contract',@duration=500,01599999999974,@affected_rows=159,@error_msg=N''

CREATE PROCEDURE [mart].[etl_log_save]
  
  @schedule_id int,
  @start_datetime datetime,
  @end_datetime datetime,
  @name nvarchar(50),
@duration float,
@affected_rows int,
@error_msg nvarchar(50)

AS
BEGIN

	SET NOCOUNT ON;


--    UPDATE etl_job_execution
--	SET schedule_id = @schedule_id, 
--        job_start_time = @start_datetime,
--		job_end_time = @end_datetime,
--		duration_s = @duration,
--		affected_rows = @affected_rows
--	WHERE (job_execution_id = @job_execution_id)

END


GO

