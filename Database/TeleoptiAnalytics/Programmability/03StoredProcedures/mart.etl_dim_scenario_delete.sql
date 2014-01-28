/****** Object:  StoredProcedure [mart].[etl_dim_scenario_delete]    Script Date: 10/29/2009 13:36:28 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_scenario_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_scenario_delete]
GO


/****** Object:  StoredProcedure [mart].[etl_dim_scenario_delete]    Script Date: 10/29/2009 13:36:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		JN
-- Create date: 2009-10-29
-- Description:	Clean scenarios in datamart without any data.
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_scenario_delete] 
AS
BEGIN
    DELETE FROM mart.dim_scenario
	WHERE scenario_id NOT IN (SELECT scenario_id FROM mart.fact_schedule)
		AND scenario_id NOT IN (SELECT scenario_id FROM mart.fact_schedule_day_count)
		AND scenario_id NOT IN (SELECT scenario_id FROM mart.fact_schedule_forecast_skill)
		AND scenario_id NOT IN (SELECT scenario_id FROM mart.fact_schedule_preference)
		AND scenario_id NOT IN (SELECT scenario_id FROM mart.fact_forecast_workload)
		AND scenario_id NOT IN (SELECT scenario_id FROM mart.fact_hourly_availability)
		AND scenario_id <> -1
END

GO