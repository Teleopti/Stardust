IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_log_save_init]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_log_save_init]
GO


CREATE PROCEDURE [mart].[etl_log_save_init]
@schedule_id int  
AS
BEGIN

	SET NOCOUNT ON;

	IF NOT EXISTS(SELECT 1 FROM mart.etl_job_schedule WHERE schedule_id = @schedule_id)
	BEGIN
		SELECT -99
		RETURN
	END

    INSERT INTO Mart.etl_job_execution (insert_date)
    VALUES (default)

    declare @id as int;
	set @id = SCOPE_IDENTITY();

    select @id;

END


GO

