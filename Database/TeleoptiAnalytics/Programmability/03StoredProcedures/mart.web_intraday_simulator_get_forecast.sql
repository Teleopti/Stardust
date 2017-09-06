IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[web_intraday_simulator_get_forecast]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[web_intraday_simulator_get_forecast]
GO

-- =============================================
-- Author:		Jonas & Maria
-- Create date: 2016-03-31
-- Description:	Load forecast data for given workload. Used by  for web intraday
-- =============================================
-- EXEC [mart].[web_intraday_simulator_get_forecast] 9, '2017-09-05', 88, '2017-09-05', 91
CREATE PROCEDURE [mart].[web_intraday_simulator_get_forecast]
@workload_id int,
@from_date smalldatetime,
@from_interval_id int,
@to_date smalldatetime,
@to_interval_id int

AS
BEGIN
	SET NOCOUNT ON;
            
	DECLARE @default_scenario_id int
	DECLARE @bu_id int
	
	SELECT @bu_id = business_unit_id FROM mart.dim_workload WHERE workload_id = @workload_id

	SELECT @default_scenario_id = scenario_id 
	FROM mart.dim_scenario 
	WHERE business_unit_id = @bu_id
		AND default_scenario = 1
	
	SELECT 
		fw.date_id,
		fw.interval_id,
		fw.forecasted_calls,		
		fw.forecasted_talk_time_s,
		fw.forecasted_after_call_work_s,
		fw.forecasted_handling_time_s
	FROM
		mart.fact_forecast_workload fw
		INNER JOIN mart.dim_date d ON fw.date_id = d.date_id
	WHERE
		fw.workload_id = @workload_id
		AND fw.scenario_id = @default_scenario_id
		AND 
			(
				(
					(@from_date <> @to_date)
					AND
					(
						(d.date_date = @from_date AND fw.interval_id >= @from_interval_id)
						OR
						(d.date_date = @to_date AND fw.interval_id < @to_interval_id)
					)
				)
				OR
				(
					(@from_date = @to_date)
					AND
					(d.date_date = @from_date AND (fw.interval_id >= @from_interval_id AND fw.interval_id < @to_interval_id))
				)
			)

	ORDER BY
		fw.date_id, fw.interval_id
END

GO

