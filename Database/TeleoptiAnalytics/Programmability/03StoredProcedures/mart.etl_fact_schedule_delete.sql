IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_delete]
GO

-- =============================================
-- Author:		Jonas
-- Create date: 2014-12-08
-- Description:	Delete all schedule rows for shift starting on given date id
-- Ola 2014-12-10 And on just one person ;)
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_schedule_delete]
	@shift_startdate_local_id int,
	@person_code uniqueidentifier,
	@scenario_id int
AS
BEGIN
	SET NOCOUNT ON;
	

	DELETE FROM mart.fact_schedule
	WHERE shift_startdate_local_id = @shift_startdate_local_id
	AND scenario_id = @scenario_id
	AND person_id in (select person_id from mart.dim_person  WITH (NOLOCK) where person_code = @person_code)

	DELETE FROM mart.fact_schedule_day_count
	WHERE shift_startdate_local_id = @shift_startdate_local_id
	AND scenario_id = @scenario_id
	AND person_id in (select person_id from mart.dim_person  WITH (NOLOCK) where person_code = @person_code)
END

GO


