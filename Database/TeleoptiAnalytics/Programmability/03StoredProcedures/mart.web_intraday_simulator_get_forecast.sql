IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[web_intraday_simulator_get_forecast]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[web_intraday_simulator_get_forecast]
GO

-- =============================================
-- Author:		Jonas & Maria
-- Create date: 2016-03-31
-- Description:	Load forecast data for given workload. Used by  for web intraday
-- =============================================
-- EXEC [mart].[web_intraday_simulator_get_forecast] 8, 'W. Europe Standard Time', '2016-04-07', 48
CREATE PROCEDURE [mart].[web_intraday_simulator_get_forecast]
@workload_id int,
@time_zone_code nvarchar(100),
@today smalldatetime,
@interval_id int

AS
BEGIN
	SET NOCOUNT ON;
            
	DECLARE @time_zone_id as int
	DECLARE @default_scenario_id int
	
	SELECT @default_scenario_id = scenario_id FROM mart.dim_scenario WHERE default_scenario = 1
	SELECT @time_zone_id = time_zone_id FROM mart.dim_time_zone WHERE time_zone_code = @time_zone_code

	SELECT
		fw.date_id,
		fw.interval_id,
		fw.forecasted_calls,
		fw.forecasted_handling_time_s
	FROM
		mart.fact_forecast_workload fw
		INNER JOIN mart.bridge_time_zone bz ON fw.date_id = bz.date_id AND fw.interval_id = bz.interval_id
		INNER JOIN mart.dim_date d ON bz.local_date_id = d.date_id
		INNER JOIN mart.dim_interval i ON bz.local_interval_id = i.interval_id
	WHERE
		fw.workload_id = @workload_id
		AND fw.scenario_id = @default_scenario_id
		AND bz.time_zone_id = @time_zone_id
		AND d.date_date = @today
		AND i.interval_id < @interval_id
	ORDER BY
		fw.date_id, fw.interval_id
END

GO

