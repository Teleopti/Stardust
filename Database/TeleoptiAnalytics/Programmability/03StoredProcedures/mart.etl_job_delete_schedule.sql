IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Mart].[etl_job_delete_schedule]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Mart].[etl_job_delete_schedule]
GO


-- =============================================
-- Author:		JN
-- Create date: 2008-06-10
-- Description:	Delete a job schedule
-- Create date: 2008-06-10
-- Update date: 
-- 2009-04-24 Transaction handling added by JN
-- 2009-04-27 Begin/Try instead of labels DJ
-- =============================================
CREATE PROCEDURE [mart].[etl_job_delete_schedule]
@schedule_id int
AS
BEGIN
SET NOCOUNT ON

BEGIN TRY

	BEGIN TRANSACTION

	-- First find out which job step errors that later needs to be deleted
	CREATE TABLE #jobstep_errors(error_id INT NOT NULL)

	INSERT INTO	#jobstep_errors(error_id)
		SELECT jobstep_error_id FROM 
			[mart].[etl_jobstep_execution] jse
		INNER JOIN
			[mart].[etl_job_execution] je
		ON
			jse.job_execution_id = je.job_execution_id
		WHERE je.schedule_id = @schedule_id
			AND jobstep_error_id IS NOT NULL

	--  ...then delete job step execution logs...
	DELETE jse FROM [mart].[etl_jobstep_execution] jse
	INNER JOIN
		[mart].[etl_job_execution] je
	ON
		jse.job_execution_id = je.job_execution_id
	WHERE je.schedule_id = @schedule_id

	-- ...then delete job step execution error logs...
	DELETE jser FROM [mart].[etl_jobstep_error] jser
	INNER JOIN 
		#jobstep_errors jse
	ON
		jser.jobstep_error_id = jse.error_id

	--  ...then delete job execution logs...
	DELETE FROM [mart].[etl_job_execution]
	WHERE schedule_id = @schedule_id

	-- ...then delete job schedules periods...
	DELETE FROM [mart].[etl_job_schedule_period]
	WHERE schedule_id = @schedule_id

	-- ...then delete the job schedule it self.
	DELETE FROM [mart].[etl_job_schedule]
	WHERE schedule_id = @schedule_id
		
	--If we got this far; commit
	COMMIT TRAN -- Transaction Success!
	RETURN
END TRY

BEGIN CATCH
	DECLARE @ErrorMsg nvarchar(4000)
	SELECT @ErrorMsg  = ERROR_MESSAGE()
	RAISERROR (@ErrorMsg,16,1)
	ROLLBACK TRANSACTION
	RETURN
END CATCH

END
GO
