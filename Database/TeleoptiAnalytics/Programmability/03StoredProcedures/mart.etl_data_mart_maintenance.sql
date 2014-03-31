IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_data_mart_maintenance]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_data_mart_maintenance]
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jonas n
-- Create date: 2010-06-16
-- Description:	A maintenance procedure that will ie remove old ETL execution logs and purge old mart data.
-- 20120207 AF: Now also prge of old mart fact data.
-- 20130325 AF: Now also Purge of old Agg data
-- =============================================
CREATE PROCEDURE [mart].[etl_data_mart_maintenance]
AS
BEGIN
	DECLARE @daysToKeepETLError INT
	DECLARE @daysToKeepETLExecution INT
	DECLARE @daysToKeepRTAEvents INT
	
	SELECT @daysToKeepETLExecution = configuration_value
	FROM mart.etl_maintenance_configuration
	WHERE configuration_name = 'daysToKeepETLExecution'
	
	SELECT @daysToKeepETLError = configuration_value
	FROM mart.etl_maintenance_configuration 
	WHERE configuration_name = 'daysToKeepETLError'
	
	SELECT @daysToKeepRTAEvents = configuration_value 
	FROM mart.etl_maintenance_configuration
	WHERE configuration_name = 'daysToKeepRTAEvents'
	
	-- Can´t keep more ETL execution days than ETL error days
	IF (@daysToKeepETLError < @daysToKeepETLExecution)
	BEGIN
		SET @daysToKeepETLError = @daysToKeepETLExecution
	END


	--Delete job and job step execution history older than x days
	DELETE FROM mart.etl_jobstep_execution
	WHERE insert_date < dateadd(day, -@daysToKeepETLExecution, getdate())
	
	DELETE FROM mart.etl_job_execution
	WHERE job_execution_id NOT IN (SELECT job_execution_id FROM mart.etl_jobstep_execution)
	
	-- Delete job step error history older than x days
	DELETE FROM mart.etl_jobstep_error
	WHERE insert_date < dateadd(day, -@daysToKeepETLError, getdate())
	
	-- Delete RTA events history older than x days
	DELETE FROM RTA.ExternalAgentState
	WHERE TimestampValue < dateadd(day, -@daysToKeepRTAEvents, getdate())

	--fact_agent
	delete mart.fact_agent
	from mart.fact_agent f
	inner join mart.dim_date d on f.date_id = d.date_id
	where 1=1
	and d.date_date < dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 4 
						and configuration_name = 'YearsToKeepFactAgent'),100),getdate())
	and d.date_date < (select dateadd(day,10,min(d2.date_date))
						from mart.fact_agent f2 inner join mart.dim_date d2 on f2.date_id = d2.date_id)

	--fact_agent_queue
	delete mart.fact_agent_queue
	from mart.fact_agent_queue f
	inner join mart.dim_date d on f.date_id = d.date_id
	where 1=1
	and d.date_date < dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 5 
						and configuration_name = 'YearsToKeepFactAgentQueue'),100),getdate())
	and d.date_date < (select dateadd(day,10,min(d2.date_date))
						from mart.fact_agent_queue f2 inner join mart.dim_date d2 on f2.date_id = d2.date_id)

	--fact_forecast_workload
	delete mart.fact_forecast_workload
	from mart.fact_forecast_workload f
	inner join mart.dim_date d on f.date_id = d.date_id
	where 1=1
	and d.date_date < dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 6
						and configuration_name = 'YearsToKeepFactForecastWorkload'),100),getdate())
	and d.date_date < (select dateadd(day,10,min(d2.date_date))
						from mart.fact_forecast_workload f2 inner join mart.dim_date d2 on f2.date_id = d2.date_id)

	--fact_queue
	delete mart.fact_queue
	from mart.fact_queue f
	inner join mart.dim_date d on f.date_id = d.date_id
	where 1=1
	and d.date_date < dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 7
						and configuration_name = 'YearsToKeepFactQueue'),100),getdate())
	and d.date_date < (select dateadd(day,10,min(d2.date_date))
						from mart.fact_queue f2 inner join mart.dim_date d2 on f2.date_id = d2.date_id)

	--fact_request
	delete mart.fact_request
	from mart.fact_request f
	inner join mart.dim_date d on f.request_start_date_id = d.date_id
	where 1=1
	and d.date_date < dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 8
						and configuration_name = 'YearsToKeepFactRequest'),100),getdate())
	and d.date_date < (select dateadd(day,10,min(d2.date_date))
						from mart.fact_request f2 inner join mart.dim_date d2 on f2.request_start_date_id = d2.date_id)
					
	--fact_schedule
	delete mart.fact_schedule
	from mart.fact_schedule f
	inner join mart.dim_date d on f.schedule_date_id = d.date_id
	where 1=1
	and d.date_date < dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 9
						and configuration_name = 'YearsToKeepFactSchedule'),100),getdate())
	and d.date_date < (select dateadd(day,10,min(d2.date_date))
						from mart.fact_schedule f2 inner join mart.dim_date d2 on f2.shift_startdate_local_id = d2.date_id)

	--fact_schedule_day_count
	delete mart.fact_schedule_day_count
	from mart.fact_schedule_day_count f
	inner join mart.dim_date d on f.shift_startdate_local_id = d.date_id
	where 1=1
	and d.date_date < dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 10
						and configuration_name = 'YearsToKeepFactScheduleDayCount'),100),getdate())
	and d.date_date < (select dateadd(day,10,min(d2.date_date))
						from mart.fact_schedule_day_count f2 inner join mart.dim_date d2 on f2.shift_startdate_local_id = d2.date_id)

	--fact_schedule_deviation
	delete mart.fact_schedule_deviation
	from mart.fact_schedule_deviation f
	inner join mart.dim_date d on f.shift_startdate_local_id = d.date_id
	where 1=1
	and d.date_date < dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 11
						and configuration_name = 'YearsToKeepFactScheduleDeviation'),100),getdate())
	and d.date_date < (select dateadd(day,10,min(d2.date_date))
						from mart.fact_schedule_deviation f2 inner join mart.dim_date d2 on f2.shift_startdate_local_id = d2.date_id)

	--fact_schedule_forecast_skill
	delete mart.fact_schedule_forecast_skill
	from mart.fact_schedule_forecast_skill f
	inner join mart.dim_date d on f.date_id = d.date_id
	where 1=1
	and d.date_date < dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 12
						and configuration_name = 'YearsToKeepFactScheduleForecastSkill'),100),getdate())
	and d.date_date < (select dateadd(day,10,min(d2.date_date))
						from mart.fact_schedule_forecast_skill f2 inner join mart.dim_date d2 on f2.date_id = d2.date_id)

	--fact_schedule_preference
	delete mart.fact_schedule_preference
	from mart.fact_schedule_preference f
	inner join mart.dim_date d on f.date_id = d.date_id
	where 1=1
	and d.date_date < dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 13
						and configuration_name = 'YearsToKeepFactSchedulePreferences'),100),getdate())
	and d.date_date < (select dateadd(day,10,min(d2.date_date))
						from mart.fact_schedule_preference f2 inner join mart.dim_date d2 on f2.date_id = d2.date_id)

	--External Agg Queue_Logg
	delete mart.v_queue_logg
	from mart.v_queue_logg f
	where 1=1
	and f.date_from < dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 14
						and configuration_name = 'YearsToKeepAggQueueStats'),100),getdate())
	and f.date_from < (select dateadd(day,10,min(f2.date_from))
						from mart.v_queue_logg f2)

	--Internal Agg Queue_Logg
	delete dbo.queue_logg
	from dbo.queue_logg f
	where 1=1
	and f.date_from < dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 14
						and configuration_name = 'YearsToKeepAggQueueStats'),100),getdate())
	and f.date_from < (select dateadd(day,10,min(f2.date_from))
						from dbo.queue_logg f2)

	--External Agg Agent_Logg
	delete mart.v_agent_logg
	from mart.v_agent_logg f
	where 1=1
	and f.date_from < dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 15
						and configuration_name = 'YearsToKeepAggAgentStats'),100),getdate())
	and f.date_from < (select dateadd(day,10,min(f2.date_from))
						from mart.v_agent_logg f2)

	--Internal Agg Agent_Logg
	delete dbo.agent_logg
	from dbo.agent_logg f
	where 1=1
	and f.date_from < dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 15
						and configuration_name = 'YearsToKeepAggAgentStats'),100),getdate())
	and f.date_from < (select dateadd(day,10,min(f2.date_from))
						from dbo.agent_logg f2)

END

GO