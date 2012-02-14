IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Mart].[etl_job_delete_schedule_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Mart].[etl_job_delete_schedule_All]
GO


-- =============================================
-- Author:		DJ
-- Create date: 2009-05-06
-- Description:	Delete a all info on all ETL schedules
-- Update date: 
-- 2009-xx-xx Comment
-- =============================================
CREATE PROCEDURE [mart].[etl_job_delete_schedule_All]
AS
BEGIN
SET NOCOUNT ON

BEGIN TRY

	BEGIN TRANSACTION

	DELETE FROM [mart].[etl_jobstep_execution]

	DELETE FROM [mart].[etl_jobstep_error]

	DELETE FROM [mart].[etl_job_execution]

	DELETE FROM [mart].[etl_job_schedule_period]

	DELETE FROM [mart].[etl_job_schedule]
	WHERE schedule_id <> -1 --Manual Job = Defult data
		
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
