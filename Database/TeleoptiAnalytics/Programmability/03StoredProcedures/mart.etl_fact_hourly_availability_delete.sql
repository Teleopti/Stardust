IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_hourly_availability_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_hourly_availability_delete]
GO
-- =============================================
-- Description:	delete fact_request and fact_requested_day
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_hourly_availability_delete] 
	   @date_id int
      ,@person_id int
      ,@scenario_id int
AS
BEGIN

DELETE FROM mart.fact_hourly_availability
WHERE date_id=@date_id AND person_id=@person_id AND scenario_id=@scenario_id

END
GO
