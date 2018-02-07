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
	DECLARE @confMinDate smalldatetime
	DECLARE @minDimDate smalldatetime
	DECLARE @daysToKeepETLError INT
	DECLARE @daysToKeepETLExecution INT
	DECLARE @daysToKeepRTAEvents INT
	DECLARE @permissionReportMinDate smalldatetime
	
	--Set up (i.e. skip migration scripts just for the purpose of populating the config table)
	--All set up has now also been removed from migration scripts to have this list as one source of truth
	--Default purge values have been changed to reflect the new Teleopti Data Retention Policy
	/* Part of Teleopti Data Retention Policy */
	/* Talk to Anders before changing anything in this section */
	IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 4)
		INSERT INTO [mart].[etl_maintenance_configuration] VALUES(4,'YearsToKeepFactAgent',2)
	IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 5)
		INSERT INTO [mart].[etl_maintenance_configuration] VALUES(5,'YearsToKeepFactAgentQueue',1)
	IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 6)
		INSERT INTO [mart].[etl_maintenance_configuration] VALUES(6,'YearsToKeepFactForecastWorkload',3)
	IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 7)
		INSERT INTO [mart].[etl_maintenance_configuration] VALUES(7,'YearsToKeepFactQueue',10)
	IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 8)
		INSERT INTO [mart].[etl_maintenance_configuration] VALUES(8,'YearsToKeepFactRequest',2)
	IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 9)
		INSERT INTO [mart].[etl_maintenance_configuration] VALUES(9,'YearsToKeepFactSchedule',3)
	IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 10)
		INSERT INTO [mart].[etl_maintenance_configuration] VALUES(10,'YearsToKeepFactScheduleDayCount',3)
	IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 11)
		INSERT INTO [mart].[etl_maintenance_configuration] VALUES(11,'YearsToKeepFactScheduleDeviation',2)
	IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 12)
		INSERT INTO [mart].[etl_maintenance_configuration] VALUES(12,'YearsToKeepFactScheduleForecastSkill',3)
	IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 13)
		INSERT INTO [mart].[etl_maintenance_configuration] VALUES(13,'YearsToKeepFactSchedulePreferences',2)



	/* Not part of Teleopti Data Retention Policy */
	if not exists (select 1 from mart.etl_maintenance_configuration where configuration_id = 1)
		insert into mart.etl_maintenance_configuration VALUES (1, N'daysToKeepETLError', 60)
	if not exists (select 1 from mart.etl_maintenance_configuration where configuration_id = 2)
		insert into mart.etl_maintenance_configuration VALUES (2, N'daysToKeepETLExecution', 45)
	if not exists (select 1 from mart.etl_maintenance_configuration where configuration_id = 3)
		insert into mart.etl_maintenance_configuration VALUES (3, N'daysToKeepRTAEvents', 2)
	if not exists(select 1 from [mart].[etl_maintenance_configuration] where configuration_id = 14)
		insert into [mart].[etl_maintenance_configuration] values(14,'YearsToKeepAggQueueStats',50)
	if not exists(select 1 from [mart].[etl_maintenance_configuration] where configuration_id = 15)
		insert into [mart].[etl_maintenance_configuration] values(15,'YearsToKeepAggAgentStats',50)
	if not exists (select 1 from mart.etl_maintenance_configuration where configuration_id = 16)
		insert into mart.etl_maintenance_configuration values(16,'DaysToKeepMessagesPurged',60)
	IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 17)
		insert into mart.etl_maintenance_configuration VALUES(17,'DaysToKeepUnchangedPermissionReport',90)



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
	
	--------------- Fact tables ---------------------
	SELECT @minDimDate =  MIN(date_date) FROM mart.dim_date WHERE date_id>-1

	--fact_agent
	SET @confMinDate = dateadd(year,-1*isnull((select isnull(configuration_value,100) FROM [mart].[etl_maintenance_configuration] 
			WHERE configuration_id = 4 AND configuration_name = 'YearsToKeepFactAgent'),100),getdate())
	IF @confMinDate >= @minDimDate
	BEGIN
		delete mart.fact_agent
		from mart.fact_agent f
		inner join mart.dim_date d on f.date_id = d.date_id
		where 1=1
		and d.date_date < @confMinDate
		and d.date_date < (select dateadd(day,10,min(d2.date_date))
							from mart.fact_agent f2 inner join mart.dim_date d2 on f2.date_id = d2.date_id)
	END

	--fact_agent_queue
	SET @confMinDate = dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 5 
						and configuration_name = 'YearsToKeepFactAgentQueue'),100),getdate())
	IF @confMinDate >= @minDimDate
	BEGIN
		delete mart.fact_agent_queue
		from mart.fact_agent_queue f
		inner join mart.dim_date d on f.date_id = d.date_id
		where 1=1
		and d.date_date < @confMinDate
		and d.date_date < (select dateadd(day,10,min(d2.date_date))
							from mart.fact_agent_queue f2 inner join mart.dim_date d2 on f2.date_id = d2.date_id)
	END

	--fact_forecast_workload
	SET @confMinDate = dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 6
						and configuration_name = 'YearsToKeepFactForecastWorkload'),100),getdate())
	IF @confMinDate >= @minDimDate
	BEGIN
		delete mart.fact_forecast_workload
		from mart.fact_forecast_workload f
		inner join mart.dim_date d on f.date_id = d.date_id
		where 1=1
		and d.date_date < @confMinDate
		and d.date_date < (select dateadd(day,10,min(d2.date_date))
							from mart.fact_forecast_workload f2 inner join mart.dim_date d2 on f2.date_id = d2.date_id)
	END

	--fact_queue
	SET @confMinDate = dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 7
						and configuration_name = 'YearsToKeepFactQueue'),100),getdate())
	IF @confMinDate >= @minDimDate
	BEGIN
		delete mart.fact_queue
		from mart.fact_queue f
		inner join mart.dim_date d on f.date_id = d.date_id
		where 1=1
		and d.date_date < @confMinDate
		and d.date_date < (select dateadd(day,10,min(d2.date_date))
							from mart.fact_queue f2 inner join mart.dim_date d2 on f2.date_id = d2.date_id)
	END

	--fact_request
	SET @confMinDate = dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 8
							and configuration_name = 'YearsToKeepFactRequest'),100),getdate())
	IF @confMinDate >= @minDimDate
	BEGIN
		delete mart.fact_request
		from mart.fact_request f
		inner join mart.dim_date d on f.request_start_date_id = d.date_id
		where 1=1
		and d.date_date < @confMinDate
		and d.date_date < (select dateadd(day,10,min(d2.date_date))
							from mart.fact_request f2 inner join mart.dim_date d2 on f2.request_start_date_id = d2.date_id)
	END
			
	--fact_schedule
	SET @confMinDate = dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 9
							and configuration_name = 'YearsToKeepFactSchedule'),100),getdate())
	IF @confMinDate >= @minDimDate
	BEGIN
		delete mart.fact_schedule
		from mart.fact_schedule f
		inner join mart.dim_date d on f.schedule_date_id = d.date_id
		where 1=1
		and d.date_date < @confMinDate
		and d.date_date < (select dateadd(day,10,min(d2.date_date))
							from mart.fact_schedule f2 inner join mart.dim_date d2 on f2.shift_startdate_local_id = d2.date_id)
	END

	--fact_schedule_day_count
	SET @confMinDate = dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 10
							and configuration_name = 'YearsToKeepFactScheduleDayCount'),100),getdate())
	IF @confMinDate >= @minDimDate
	BEGIN
		delete mart.fact_schedule_day_count
		from mart.fact_schedule_day_count f
		inner join mart.dim_date d on f.shift_startdate_local_id = d.date_id
		where 1=1
		and d.date_date < @confMinDate
		and d.date_date < (select dateadd(day,10,min(d2.date_date))
							from mart.fact_schedule_day_count f2 inner join mart.dim_date d2 on f2.shift_startdate_local_id = d2.date_id)
	END

	--fact_schedule_deviation
	SET @confMinDate = dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 11
						and configuration_name = 'YearsToKeepFactScheduleDeviation'),100),getdate())
	IF @confMinDate >= @minDimDate
	BEGIN
		delete mart.fact_schedule_deviation
		from mart.fact_schedule_deviation f
		inner join mart.dim_date d on f.shift_startdate_local_id = d.date_id
		where 1=1
		and d.date_date < @confMinDate
		and d.date_date < (select dateadd(day,10,min(d2.date_date))
							from mart.fact_schedule_deviation f2 inner join mart.dim_date d2 on f2.shift_startdate_local_id = d2.date_id)
	END

	--fact_schedule_forecast_skill
	SET @confMinDate = dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 12
						and configuration_name = 'YearsToKeepFactScheduleForecastSkill'),100),getdate())
	IF @confMinDate >= @minDimDate
	BEGIN
		delete mart.fact_schedule_forecast_skill
		from mart.fact_schedule_forecast_skill f
		inner join mart.dim_date d on f.date_id = d.date_id
		where 1=1
		and d.date_date < @confMinDate
		and d.date_date < (select dateadd(day,10,min(d2.date_date))
							from mart.fact_schedule_forecast_skill f2 inner join mart.dim_date d2 on f2.date_id = d2.date_id)
	END

	--fact_schedule_preference
	SET @confMinDate = dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 13
							and configuration_name = 'YearsToKeepFactSchedulePreferences'),100),getdate())
	IF @confMinDate >= @minDimDate
	BEGIN
		delete mart.fact_schedule_preference
		from mart.fact_schedule_preference f
		inner join mart.dim_date d on f.date_id = d.date_id
		where 1=1
		and d.date_date < @confMinDate
		and d.date_date < (select dateadd(day,10,min(d2.date_date))
							from mart.fact_schedule_preference f2 inner join mart.dim_date d2 on f2.date_id = d2.date_id)
	END
	--External Agg tables, but only if mapped 1:1 via crossDb-views. If any custom views exists, then skip it
	IF not exists (
		select 1
		from mart.sys_crossdatabaseview_custom
		)
	exec mart.etl_data_mart_maintenance_aggTables

	--Internal Agg Queue_Logg
	SET @confMinDate = dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 14
							and configuration_name = 'YearsToKeepAggQueueStats'),100),getdate())
	IF @confMinDate >= @minDimDate
	BEGIN
		delete dbo.queue_logg
		from dbo.queue_logg f
		where 1=1
		and f.date_from < @confMinDate
		and f.date_from < (select dateadd(day,10,min(f2.date_from))
							from dbo.queue_logg f2)
	END
	--Internal Agg Agent_Logg
	SET @confMinDate = dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 15
						and configuration_name = 'YearsToKeepAggAgentStats'),100),getdate())
	IF @confMinDate >= @minDimDate
	BEGIN
		delete dbo.agent_logg
		from dbo.agent_logg f
		where 1=1
		and f.date_from < @confMinDate
		and f.date_from < (select dateadd(day,10,min(f2.date_from))
							from dbo.agent_logg f2)
	END

	--Queue
	SET @confMinDate = dateadd(day,-1*(select isnull(configuration_value,60) from [mart].[etl_maintenance_configuration] where configuration_id = 16),getdate())

	delete top(10000) from Queue.MessagesPurged
	where PurgedAt < @confMinDate
	
	--Permission Report, delete unchanged old report permissions
	SET @permissionReportMinDate = dateadd(day,-1*(select isnull(configuration_value,90) from [mart].[etl_maintenance_configuration] where configuration_id = 17 
		AND configuration_name = 'DaysToKeepUnchangedPermissionReport'),getdate())

	delete from mart.permission_report
	where datasource_update_date < @permissionReportMinDate

	-- Delete group pages without persons
	DELETE from [mart].[dim_group_page]
	WHERE group_page_id NOT IN (SELECT group_page_id FROM mart.bridge_group_page_person)
END

GO