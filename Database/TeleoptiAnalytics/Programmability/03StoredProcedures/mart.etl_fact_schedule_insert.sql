IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_insert]
GO

-- =============================================
-- Author:		Jonas
-- Create date: 2014-12-08
-- Description:	Insert schedule row from schedule change event in client
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_schedule_insert]
	@shift_startdate_local_id int,
	@schedule_date_id int,
	@person_id int,
	@interval_id smallint,
	@activity_starttime smalldatetime,
	@scenario_id int,
	@activity_id int,
	@absence_id int,
	@activity_startdate_id int,
	@activity_enddate_id int,
	@activity_endtime smalldatetime,
	@shift_startdate_id int,
	@shift_starttime smalldatetime,
	@shift_enddate_id int,
	@shift_endtime smalldatetime,
	@shift_startinterval_id smallint,
	@shift_endinterval_id smallint,
	@shift_category_id int,
	@shift_length_m int,
	@scheduled_time_m int,
	@scheduled_time_absence_m int,
	@scheduled_time_activity_m int,
	@scheduled_contract_time_m int,
	@scheduled_contract_time_activity_m int,
	@scheduled_contract_time_absence_m int,
	@scheduled_work_time_m int,
	@scheduled_work_time_activity_m int,
	@scheduled_work_time_absence_m int,
	@scheduled_over_time_m int,
	@scheduled_ready_time_m int,
	@scheduled_paid_time_m int,
	@scheduled_paid_time_activity_m int,
	@scheduled_paid_time_absence_m int,
	@business_unit_id int,
	@datasource_update_date smalldatetime,
	@overtime_id int
AS
BEGIN
	SET NOCOUNT ON;

	-- Get or create shift_length_id from @shift_length_m
	DECLARE @shift_length_id int

	SELECT @shift_length_id = shift_length_id
	FROM mart.dim_shift_length
	WHERE shift_length_m = @shift_length_m

	IF @shift_length_id IS NULL
	BEGIN
		INSERT INTO mart.dim_shift_length (shift_length_m, shift_length_h)
		VALUES (@shift_length_m, CONVERT(decimal(10,2), @shift_length_m / 60))

		SET @shift_length_id = @@IDENTITY
	END


	INSERT INTO mart.fact_schedule
           (shift_startdate_local_id
           ,schedule_date_id
           ,person_id
           ,interval_id
           ,activity_starttime
           ,scenario_id
           ,activity_id
           ,absence_id
           ,activity_startdate_id
           ,activity_enddate_id
           ,activity_endtime
           ,shift_startdate_id
           ,shift_starttime
           ,shift_enddate_id
           ,shift_endtime
           ,shift_startinterval_id
           ,shift_endinterval_id
           ,shift_category_id
           ,shift_length_id
           ,scheduled_time_m
           ,scheduled_time_absence_m
           ,scheduled_time_activity_m
           ,scheduled_contract_time_m
           ,scheduled_contract_time_activity_m
           ,scheduled_contract_time_absence_m
           ,scheduled_work_time_m
           ,scheduled_work_time_activity_m
           ,scheduled_work_time_absence_m
           ,scheduled_over_time_m
           ,scheduled_ready_time_m
           ,scheduled_paid_time_m
           ,scheduled_paid_time_activity_m
           ,scheduled_paid_time_absence_m
           ,business_unit_id
           ,datasource_update_date
           ,overtime_id)
     VALUES
           (@shift_startdate_local_id,
			@schedule_date_id,
			@person_id,
			@interval_id,
			@activity_starttime,
			@scenario_id,
			@activity_id,
			@absence_id,
			@activity_startdate_id,
			@activity_enddate_id,
			@activity_endtime,
			@shift_startdate_id,
			@shift_starttime,
			@shift_enddate_id,
			@shift_endtime,
			@shift_startinterval_id,
			@shift_endinterval_id,
			@shift_category_id,
			@shift_length_id,
			@scheduled_time_m,
			@scheduled_time_absence_m,
			@scheduled_time_activity_m,
			@scheduled_contract_time_m,
			@scheduled_contract_time_activity_m,
			@scheduled_contract_time_absence_m,
			@scheduled_work_time_m,
			@scheduled_work_time_activity_m,
			@scheduled_work_time_absence_m,
			@scheduled_over_time_m,
			@scheduled_ready_time_m,
			@scheduled_paid_time_m,
			@scheduled_paid_time_activity_m,
			@scheduled_paid_time_absence_m,
			@business_unit_id,
			@datasource_update_date,
			@overtime_id)
END

GO


