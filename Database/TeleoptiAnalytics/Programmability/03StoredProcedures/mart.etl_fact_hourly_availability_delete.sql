IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_hourly_availability_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_hourly_availability_delete]
GO
-- =============================================
-- Description:	delete fact_request and fact_requested_day
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_hourly_availability_delete] 
	   @date_id int
      ,@person_code uniqueidentifier
      ,@scenario_id int
AS
BEGIN

DELETE a
  FROM mart.fact_hourly_availability a
 INNER JOIN mart.dim_person p ON a.person_id = p.person_id
 WHERE a.date_id = @date_id
   AND p.person_code = @person_code
   AND a.scenario_id = @scenario_id

END
GO
