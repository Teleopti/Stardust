IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_hourly_availability_add_or_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_hourly_availability_add_or_update]
GO
-- =============================================
-- Description:	add or update fact_hourly_availability
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_hourly_availability_add_or_update] 
	   @date_id int
      ,@person_id int
      ,@scenario_id int
      ,@available_time_m int
      ,@available_days int
      ,@scheduled_time_m int
      ,@scheduled_days int
      ,@business_unit_id int
AS
declare @rows int

INSERT INTO mart.fact_hourly_availability
SELECT 
	@date_id
	,@person_id
	,@scenario_id
	,@available_time_m
	,@available_days
	,@scheduled_time_m
	,@scheduled_days
	,@business_unit_id
	,1
WHERE NOT EXISTS(SELECT 1 FROM mart.fact_hourly_availability WHERE date_id=@date_id AND person_id=@person_id AND scenario_id=@scenario_id)

SET @rows = (SELECT @@ROWCOUNT)
IF @rows = 0
BEGIN
	UPDATE mart.fact_hourly_availability
	SET
		available_time_m = @available_time_m
		,available_days = @available_days
		,scheduled_time_m = @scheduled_time_m
		,scheduled_days = @scheduled_days
		,business_unit_id = @business_unit_id
		,datasource_id = 1
	WHERE
	date_id=@date_id AND person_id=@person_id AND scenario_id=@scenario_id
END
GO



		
