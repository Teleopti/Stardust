IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Mart].[etl_job_save_schedule]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Mart].[etl_job_save_schedule]
GO


-- =============================================
-- Author:		JN
-- Create date: 2008-06-09
-- Description:	Insert/update a job schedule
-- =============================================
CREATE PROCEDURE [mart].[etl_job_save_schedule]
@schedule_id int,
@schedule_name nvarchar(150),
@enabled bit,
@schedule_type int,
@occurs_daily_at int,
@occurs_every_minute int,
@recurring_starttime int,
@recurring_endtime int,
@etl_job_name nvarchar(150),
@etl_relative_period_start int,
@etl_relative_period_end int,
@etl_datasource_id int,
@description nvarchar(500)
AS
BEGIN
	SET NOCOUNT ON;

	IF @schedule_id = -1
	BEGIN
		-- INSERT
		INSERT INTO Mart.[etl_job_schedule]
			([schedule_name]
			,[enabled]
			,[schedule_type]
			,[occurs_daily_at]
			,[occurs_every_minute]
			,[recurring_starttime]
			,[recurring_endtime]
			,[etl_job_name]
			,[etl_relative_period_start]
			,[etl_relative_period_end]
			,[etl_datasource_id]
			,[description])
		VALUES
			(@schedule_name
			,@enabled
			,@schedule_type
			,@occurs_daily_at
			,@occurs_every_minute
			,@recurring_starttime
			,@recurring_endtime
			,@etl_job_name
			,@etl_relative_period_start
			,@etl_relative_period_end
			,@etl_datasource_id
			,@description)

			SET @schedule_id = @@IDENTITY
	END
	ELSE
	BEGIN
		-- UPDATE
		UPDATE Mart.[etl_job_schedule]
		SET [schedule_name] = @schedule_name
			,[enabled] = @enabled
			,[schedule_type] = @schedule_type
			,[occurs_daily_at] = @occurs_daily_at
			,[occurs_every_minute] = @occurs_every_minute
			,[recurring_starttime] = @recurring_starttime
			,[recurring_endtime] = @recurring_endtime
			,[etl_job_name] = @etl_job_name
			,[etl_relative_period_start] = @etl_relative_period_start
			,[etl_relative_period_end] = @etl_relative_period_end
			,[etl_datasource_id] = @etl_datasource_id
			,[description] = @description
		WHERE schedule_id = @schedule_id

		-- Clear its schedule periods since new will be inserted after this procedure
		DELETE
		FROM
			Mart.etl_job_schedule_period
		WHERE
			schedule_id = @schedule_id
	END

	-- Return inserted/updated schedule id
	SELECT @schedule_id as schedule_id
END


GO

