IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Mart].[etl_job_save_schedule_period]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Mart].[etl_job_save_schedule_period]
GO


-- =============================================
-- Author:		JN
-- Create date: 2008-10-16
-- Description:	Insert/update a job schedule period
-- =============================================
CREATE PROCEDURE [mart].[etl_job_save_schedule_period]
@schedule_id int,
@job_name nvarchar(50),
@relative_period_start int,
@relative_period_end int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @job_id int
	SET @job_id = (SELECT job_id FROM Mart.etl_job WHERE job_name = @job_name)

	IF @job_id IS NULL
		RETURN

	-- Insert
	INSERT INTO Mart.[etl_job_schedule_period]
		   ([schedule_id]
		   ,[job_id]
		   ,[relative_period_start]
		   ,[relative_period_end])
	 VALUES
		   (@schedule_id
		   ,@job_id
		   ,@relative_period_start
		   ,@relative_period_end)
END


GO

