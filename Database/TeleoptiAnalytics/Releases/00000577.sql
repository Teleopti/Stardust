PRINT 'Removing duplicates in dim_date if exists, and update related tables. Please be patient...'
SET NOCOUNT ON
declare @min_date_date smalldatetime
declare @min_date_id int

select top 1 @min_date_date = date_date
from mart.dim_date
group by date_date
having count(*) > 1
order by date_date

select top 1 @min_date_id = date_id
from mart.dim_date where date_date = @min_date_date order by date_id asc

-- save all rows with wrong dim date id in referenced table
SELECT f.date_id, f.interval_id, f.acd_login_id, d.date_date INTO #fact_agent_tmp 
	from mart.fact_agent f
        inner join mart.dim_date d on f.date_id = d.date_id
        where d.date_date >= @min_date_date 

SELECT f.date_id, f.interval_id, f.queue_id, f.acd_login_id, d.date_date INTO #fact_agent_queue_tmp
	from mart.fact_agent_queue f
		inner join mart.dim_date d on f.date_id = d.date_id
		where d.date_date >= @min_date_date

SELECT f.date_id, f.interval_id, f.start_time, f.workload_id, f.scenario_id, d.date_date INTO #fact_forecast_workload_tmp
	from mart.fact_forecast_workload f
		inner join mart.dim_date d on f.date_id = d.date_id
		where d.date_date >= @min_date_date

SELECT f.date_id, f.person_id, f.scenario_id, d.date_date INTO #fact_hourly_availability_tmp
	from mart.fact_hourly_availability f
		inner join mart.dim_date d on f.date_id = d.date_id
		where d.date_date >= @min_date_date

SELECT f.evaluation_id, f.datasource_id, d.date_date INTO #fact_quality_tmp
	from mart.fact_quality f
		inner join mart.dim_date d on f.date_id = d.date_id
		where d.date_date >= @min_date_date

SELECT f.date_id, f.interval_id, f.queue_id, d.date_date INTO #fact_queue_tmp
	from mart.fact_queue f
		inner join mart.dim_date d on f.date_id = d.date_id
		where d.date_date >= @min_date_date

SELECT f.request_code, d.date_date INTO #fact_request_tmp
	from mart.fact_request f
		inner join mart.dim_date d on f.request_start_date_id = d.date_id
		where d.date_date >= @min_date_date

SELECT f.request_code, f.request_date_id, d.date_date INTO #fact_requested_days_tmp
	from mart.fact_requested_days f
		inner join mart.dim_date d on f.request_date_id = d.date_id
		where d.date_date >= @min_date_date

--fact_schedule
SELECT f.shift_startdate_local_id, f.schedule_date_id, f.person_id, f.interval_id, f.activity_starttime, f.scenario_id, 
	d1.date_date as new_shift_startdate_local_id,
	d2.date_date as new_schedule_date_id,
	d3.date_date as new_activity_startdate_id,
	d4.date_date as new_activity_enddate_id,
	d5.date_date as new_shift_startdate_id,
	d6.date_date as new_shift_enddate_id
	INTO #fact_schedule_tmp
	from mart.fact_schedule f
		inner join mart.dim_date d1 on f.shift_startdate_local_id = d1.date_id
		inner join mart.dim_date d2 on f.schedule_date_id = d2.date_id
		inner join mart.dim_date d3 on f.activity_startdate_id = d3.date_id
		inner join mart.dim_date d4 on f.activity_enddate_id = d4.date_id
		inner join mart.dim_date d5 on f.shift_startdate_id = d5.date_id
		inner join mart.dim_date d6 on f.shift_enddate_id = d6.date_id
		where d1.date_date >= @min_date_date

--fact_schedule_convert
SELECT f.shift_startdate_local_id, f.schedule_date_id, f.person_id, f.interval_id, f.activity_starttime, f.scenario_id, 
	d1.date_date as new_shift_startdate_local_id,
	d2.date_date as new_schedule_date_id,
	d3.date_date as new_activity_startdate_id,
	d4.date_date as new_activity_enddate_id,
	d5.date_date as new_shift_startdate_id,
	d6.date_date as new_shift_enddate_id
	INTO #fact_schedule_convert_tmp
	from mart.fact_schedule_convert f
		inner join mart.dim_date d1 on f.shift_startdate_local_id = d1.date_id
		inner join mart.dim_date d2 on f.schedule_date_id = d2.date_id
		inner join mart.dim_date d3 on f.activity_startdate_id = d3.date_id
		inner join mart.dim_date d4 on f.activity_enddate_id = d4.date_id
		inner join mart.dim_date d5 on f.shift_startdate_id = d5.date_id
		inner join mart.dim_date d6 on f.shift_enddate_id = d6.date_id
		where d1.date_date >= @min_date_date


SELECT f.shift_startdate_local_id, f.person_id, f.scenario_id, d.date_date INTO #fact_schedule_day_count_tmp
	from mart.fact_schedule_day_count f
		inner join mart.dim_date d on f.shift_startdate_local_id = d.date_id
		where d.date_date >= @min_date_date

--fact_schedule_deviation
SELECT f.shift_startdate_local_id, f.date_id, f.interval_id, f.person_id,
	d1.date_date as new_shift_startdate_local_id,
	d2.date_date as new_date_id,
	d3.date_date as new_shift_startdate_id
	INTO #fact_schedule_deviation_tmp
	from mart.fact_schedule_deviation f
		inner join mart.dim_date d1 on f.shift_startdate_local_id = d1.date_id
		inner join mart.dim_date d2 on f.date_id = d2.date_id
		inner join mart.dim_date d3 on f.shift_startdate_id = d3.date_id
		where d1.date_date >= @min_date_date


SELECT f.date_id, f.interval_id, f.skill_id, f.scenario_id, d.date_date INTO #fact_schedule_forecast_skill_tmp
	from mart.fact_schedule_forecast_skill f
		inner join mart.dim_date d on f.date_id = d.date_id
		where d.date_date >= @min_date_date

SELECT f.date_id, f.interval_id, f.person_id, f.scenario_id, f.preference_type_id, f.shift_category_id, f.day_off_id, f.absence_id, d.date_date INTO #fact_schedule_preference_tmp
	from mart.fact_schedule_preference f
		inner join mart.dim_date d on f.date_id = d.date_id
		where d.date_date >= @min_date_date

--bridge_time_zone
SELECT f.date_id, f.interval_Id, f.time_zone_id, 
	d1.date_date as new_date_id,
	d2.date_date as new_local_date_id
	INTO #bridge_time_zone_tmp
	from mart.bridge_time_zone f
		inner join mart.dim_date d1 on f.date_id = d1.date_id
		inner join mart.dim_date d2 on f.local_date_id = d2.date_id
		where d1.date_date >= @min_date_date


--Disable FK's temporary
ALTER TABLE [mart].[fact_agent] NOCHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_agent_queue] NOCHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_forecast_workload] NOCHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_hourly_availability] NOCHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_quality] NOCHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_queue] NOCHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_request] NOCHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_requested_days] NOCHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_schedule] NOCHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_schedule_convert] NOCHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_schedule_day_count] NOCHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_schedule_deviation] NOCHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_schedule_forecast_skill] NOCHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_schedule_preference] NOCHECK CONSTRAINT ALL;
ALTER TABLE [mart].[bridge_time_zone] NOCHECK CONSTRAINT ALL;


--Delete unwanted dates
SELECT dd.date_date, date_id AS old_id, new_id 
INTO #duplicates
FROM [mart].[dim_date] dd
JOIN (SELECT date_date, MIN(date_id) AS new_id
  FROM [mart].[dim_date]
  GROUP BY date_date HAVING COUNT(date_date) > 1) err ON dd.date_date = err.date_date
WHERE date_id <> new_id

delete from [mart].[dim_date] 
where date_id in (select old_id from #duplicates)

drop table #duplicates

-- save non-sequentialed id of dim dates into tmp table
select [date_date]
      ,[year]
      ,[year_month]
      ,[month]
      ,[month_name]
      ,[month_resource_key]
      ,[day_in_month]
      ,[weekday_number]
      ,[weekday_name]
      ,[weekday_resource_key]
      ,[week_number]
      ,[year_week]
      ,[quarter]
      ,[insert_date]
      INTO #dim_date_temp
FROM [mart].[dim_date]
WHERE [date_date] > @min_date_date

DELETE FROM [mart].[dim_date]
WHERE [date_date] > @min_date_date


-- save dim dates back with sequential id
SET IDENTITY_INSERT [mart].[dim_date] ON

INSERT INTO [mart].[dim_date]
 ([date_id]
      ,[date_date]
      ,[year]
      ,[year_month]
      ,[month]
      ,[month_name]
      ,[month_resource_key]
      ,[day_in_month]
      ,[weekday_number]
      ,[weekday_name]
      ,[weekday_resource_key]
      ,[week_number]
      ,[year_week]
      ,[quarter]
      ,[insert_date])
SELECT @min_date_id + datediff(day,@min_date_date, date_date)
      ,[date_date]
      ,[year]
      ,[year_month]
      ,[month]
      ,[month_name]
      ,[month_resource_key]
      ,[day_in_month]
      ,[weekday_number]
      ,[weekday_name]
      ,[weekday_resource_key]
      ,[week_number]
      ,[year_week]
      ,[quarter]
      ,[insert_date]
FROM #dim_date_temp ORDER BY date_date asc

drop table #dim_date_temp

SET IDENTITY_INSERT [mart].[dim_date] OFF

-- update reference table with correct dim date id
update f set f.date_id = d.date_id from mart.fact_agent f
	inner join #fact_agent_tmp ft on ft.date_id = f.date_id and ft.interval_id = f.interval_id and ft.acd_login_id = f.acd_login_id
	inner join mart.dim_date d on ft.date_date = d.date_date

update f set f.date_id = d.date_id from mart.fact_agent_queue f
	inner join #fact_agent_queue_tmp ft on ft.date_id = f.date_id and ft.interval_id = f.interval_id and ft.acd_login_id = f.acd_login_id
	inner join mart.dim_date d on ft.date_date = d.date_date

update f set f.date_id = d.date_id from mart.fact_forecast_workload f
	inner join #fact_forecast_workload_tmp ft on ft.date_id = f.date_id and ft.interval_id = f.interval_id and ft.start_time = f.start_time and ft.workload_id = f.workload_id and ft.scenario_id = f.scenario_id
	inner join mart.dim_date d on ft.date_date = d.date_date

update f set f.date_id = d.date_id from mart.fact_hourly_availability f
	inner join #fact_hourly_availability_tmp ft on ft.date_id = f.date_id and ft.person_id = f.person_id and ft.scenario_id = f.scenario_id
	inner join mart.dim_date d on ft.date_date = d.date_date

update f set f.date_id = d.date_id from mart.fact_quality f
	inner join #fact_quality_tmp ft on ft.evaluation_id = f.evaluation_id and ft.datasource_id = f.datasource_id
	inner join mart.dim_date d on ft.date_date = d.date_date

update f set f.date_id = d.date_id from mart.fact_queue f
	inner join #fact_queue_tmp ft on ft.date_id = f.date_id and ft.interval_id = f.interval_id and ft.queue_id = f.queue_id
	inner join mart.dim_date d on ft.date_date = d.date_date

update f set f.request_start_date_id = d.date_id from mart.fact_request f
	inner join #fact_request_tmp ft on ft.request_code = f.request_code
	inner join mart.dim_date d on ft.date_date = d.date_date

update f set f.request_date_id = d.date_id from mart.fact_requested_days f
	inner join #fact_requested_days_tmp ft on ft.request_code = f.request_code and ft.request_date_id = f.request_date_id
	inner join mart.dim_date d on ft.date_date = d.date_date

--fact_schedule
update f set f.shift_startdate_local_id = d1.date_id, 
			f.schedule_date_id = d2.date_id,
			f.activity_startdate_id = d3.date_id,
			f.activity_enddate_id = d4.date_id,
			f.shift_startdate_id = d5.date_id,
			f.shift_enddate_id = d6.date_id
	from mart.fact_schedule f
	inner join #fact_schedule_tmp ft on ft.shift_startdate_local_id = f.shift_startdate_local_id and ft.schedule_date_id = f.schedule_date_id and ft.person_id = f.person_id and ft.interval_id = f.interval_id and ft.activity_starttime = f.activity_starttime and ft.scenario_id = f.scenario_id
	inner join mart.dim_date d1 on ft.new_shift_startdate_local_id = d1.date_date
	inner join mart.dim_date d2 on ft.new_schedule_date_id = d2.date_date
	inner join mart.dim_date d3 on ft.new_activity_startdate_id = d3.date_date
	inner join mart.dim_date d4 on ft.new_activity_enddate_id = d4.date_date
	inner join mart.dim_date d5 on ft.new_shift_startdate_id = d5.date_date
	inner join mart.dim_date d6 on ft.new_shift_enddate_id = d6.date_date

--fact_schedule_convert
update f set f.shift_startdate_local_id = d1.date_id,
			f.schedule_date_id = d2.date_id,
			f.activity_startdate_id = d3.date_id,
			f.activity_enddate_id = d4.date_id,
			f.shift_startdate_id = d5.date_id,
			f.shift_enddate_id = d6.date_id
	from mart.fact_schedule_convert f
	inner join #fact_schedule_convert_tmp ft on ft.shift_startdate_local_id = f.shift_startdate_local_id and ft.schedule_date_id = f.schedule_date_id and ft.person_id = f.person_id and ft.interval_id = f.interval_id and ft.activity_starttime = f.activity_starttime and ft.scenario_id = f.scenario_id
	inner join mart.dim_date d1 on ft.new_shift_startdate_local_id = d1.date_date
	inner join mart.dim_date d2 on ft.new_schedule_date_id = d2.date_date
	inner join mart.dim_date d3 on ft.new_activity_startdate_id = d3.date_date
	inner join mart.dim_date d4 on ft.new_activity_enddate_id = d4.date_date
	inner join mart.dim_date d5 on ft.new_shift_startdate_id = d5.date_date
	inner join mart.dim_date d6 on ft.new_shift_enddate_id = d6.date_date


update f set f.shift_startdate_local_id = d.date_id 
	from mart.fact_schedule_day_count f
	inner join #fact_schedule_day_count_tmp ft on ft.shift_startdate_local_id = f.shift_startdate_local_id and ft.person_id = f.person_id and ft.scenario_id = f.scenario_id
	inner join mart.dim_date d on ft.date_date = d.date_date


--fact_schedule_deviation
update f set f.shift_startdate_local_id = d1.date_id,
			f.date_id = d2.date_id,
			f.shift_startdate_id = d3.date_id
	from mart.fact_schedule_deviation f
	inner join #fact_schedule_deviation_tmp ft on ft.shift_startdate_local_id = f.shift_startdate_local_id and ft.date_id = f.date_id and ft.interval_id = f.interval_id and ft.person_id = f.person_id
	inner join mart.dim_date d1 on ft.new_shift_startdate_local_id = d1.date_date
	inner join mart.dim_date d2 on ft.new_date_id = d2.date_date
	inner join mart.dim_date d3 on ft.new_shift_startdate_id = d3.date_date


update f set f.date_id = d.date_id
	from mart.fact_schedule_forecast_skill f
	inner join #fact_schedule_forecast_skill_tmp ft on ft.date_id = f.date_id and ft.interval_id = f.interval_id and ft.skill_id = f.skill_id and ft.scenario_id = f.scenario_id
	inner join mart.dim_date d on ft.date_date = d.date_date

update f set f.date_id = d.date_id 
	from mart.fact_schedule_preference f
	inner join #fact_schedule_preference_tmp ft on ft.date_id = f.date_id and ft.interval_id = f.interval_id and ft.person_id = f.person_id and ft.scenario_id = f.scenario_id and ft.preference_type_id = f.preference_type_id and ft.shift_category_id = f.shift_category_id and ft.day_off_id = f.day_off_id and ft.absence_id = f.absence_id
	inner join mart.dim_date d on ft.date_date = d.date_date


--bridge_time_zone
update f set f.date_id = d1.date_id,
			f.local_date_id = d2.date_id
	from mart.bridge_time_zone f
	inner join #bridge_time_zone_tmp ft on ft.date_id =f.date_id and ft.interval_id = f.interval_id and ft.time_zone_id = f.time_zone_id
	inner join mart.dim_date d1 on ft.new_date_id = d1.date_date
	inner join mart.dim_date d2 on ft.new_local_date_id = d2.date_date

--re-enable FK's
ALTER TABLE [mart].[fact_agent] WITH NOCHECK CHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_agent_queue] WITH NOCHECK CHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_forecast_workload] WITH NOCHECK CHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_hourly_availability] WITH NOCHECK CHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_quality] WITH NOCHECK CHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_queue] WITH NOCHECK CHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_request] WITH NOCHECK CHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_requested_days] WITH NOCHECK CHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_schedule] WITH NOCHECK CHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_schedule_convert] WITH NOCHECK CHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_schedule_day_count] WITH NOCHECK CHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_schedule_deviation] WITH NOCHECK CHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_schedule_forecast_skill] WITH NOCHECK CHECK CONSTRAINT ALL;
ALTER TABLE [mart].[fact_schedule_preference] WITH NOCHECK CHECK CONSTRAINT ALL;
ALTER TABLE [mart].[bridge_time_zone] WITH NOCHECK CHECK CONSTRAINT ALL;



drop table #fact_agent_tmp
drop table #fact_agent_queue_tmp
drop table #fact_forecast_workload_tmp
drop table #fact_hourly_availability_tmp
drop table #fact_quality_tmp
drop table #fact_queue_tmp
drop table #fact_request_tmp
drop table #fact_requested_days_tmp
drop table #fact_schedule_tmp
drop table #fact_schedule_convert_tmp
drop table #fact_schedule_day_count_tmp
drop table #fact_schedule_deviation_tmp
drop table #fact_schedule_forecast_skill_tmp
drop table #fact_schedule_preference_tmp
drop table #bridge_time_zone_tmp

SET NOCOUNT OFF

--#remove the temp fix added for some customers
IF EXISTS(SELECT * 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='UC_dim_date_date_date')
BEGIN
	ALTER TABLE [mart].[dim_date] DROP CONSTRAINT [UC_dim_date_date_date]
END

ALTER TABLE [mart].[dim_date] ADD CONSTRAINT [UC_dim_date_date_date] UNIQUE (date_date)
GO