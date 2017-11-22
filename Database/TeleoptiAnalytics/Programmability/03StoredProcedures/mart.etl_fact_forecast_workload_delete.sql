IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_forecast_workload_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_forecast_workload_delete] 
GO

-- =============================================
-- Author:		FelixBj
-- Create date: 2008-02-21
-- Description:	Removes forecast workload intervals 
--
-- ChangeLog
-- Date			By	Description
-- ==============================================
-- ==============================================
-- exec [mart].[etl_fact_forecast_workload_delete] 13422, 32, 74, 8, '2017-09-26 08:00:00'
CREATE PROCEDURE [mart].[etl_fact_forecast_workload_delete] 
		@date_id int
		,@interval_id int
		,@workload_id int
		,@scenario_id int
		,@start_time smalldatetime
AS
BEGIN
	DELETE FROM mart.fact_forecast_workload 
	WHERE date_id = @date_id
	and interval_id = @interval_id
	and workload_id = @workload_id
	and scenario_id = @scenario_id 
	and start_time = @start_time
END
GO



