set quoted_identifier on
set arithabort off
set numeric_roundabort off
set ansi_warnings on
set ansi_padding on
set ansi_nulls on
set concat_null_yields_null on
set cursor_close_on_commit off

-- Delete data from bridge and other tables
DELETE FROM mart.bridge_queue_workload
DELETE FROM mart.bridge_skillset_skill
DELETE FROM mart.permission_report
DELETE FROM mart.scorecard_kpi
DELETE FROM dbo.aspnet_Membership
DELETE FROM dbo.aspnet_Users
DELETE FROM mart.bridge_acd_login_person
DELETE FROM mart.bridge_group_page_person
TRUNCATE TABLE mart.bridge_time_zone

-- Delete data from fact tables
TRUNCATE TABLE mart.fact_schedule
TRUNCATE TABLE mart.fact_queue
TRUNCATE TABLE mart.fact_forecast_workload
TRUNCATE TABLE mart.fact_schedule_forecast_skill
TRUNCATE TABLE mart.fact_agent
TRUNCATE TABLE mart.fact_agent_queue
TRUNCATE TABLE mart.fact_kpi_targets_team
TRUNCATE TABLE mart.fact_schedule_deviation
TRUNCATE TABLE mart.fact_schedule_day_count
TRUNCATE TABLE mart.fact_schedule_preference

--this unused time zone makes ETL to load bridge_time_zone over and over again
delete from mart.dim_time_zone where time_zone_code = 'GMT Standard Time'

--Insert ETL properties
DELETE [mart].[sys_configuration]
exec mart.sys_configuration_save @key=N'Culture',@value=1053
exec mart.sys_configuration_save @key=N'IntervalLengthMinutes',@value=15
exec mart.sys_configuration_save @key=N'TimeZoneCode',@value=N'W. Europe Standard Time'

--Delete All old schedules
EXEC mart.etl_job_delete_schedule_All

--Get Agg Data into Analytics
INSERT [TeleoptiAnalytics_Demo].[dbo].[acd_type]
SELECT [acd_type_id]
      ,[acd_type_desc]
  FROM [TeleoptiAgg_Demo].[dbo].[acd_type]

INSERT [TeleoptiAnalytics_Demo].[dbo].[log_object]
SELECT [log_object_id]
      ,[acd_type_id]
      ,[log_object_desc]
      ,[logDB_name]
      ,[intervals_per_day]
      ,[default_service_level_sec]
      ,[default_short_call_treshold]
  FROM [TeleoptiAgg_Demo].[dbo].[log_object]

SET IDENTITY_INSERT [TeleoptiAnalytics_Demo].dbo.agent_info ON
INSERT [TeleoptiAnalytics_Demo].[dbo].[agent_info]
([Agent_id]
      ,[Agent_name]
      ,[is_active]
      ,[log_object_id]
      ,[orig_agent_id])
SELECT [Agent_id]
      ,[Agent_name]
      ,[is_active]
      ,[log_object_id]
      ,[orig_agent_id]
  FROM [TeleoptiAgg_Demo].[dbo].[agent_info]
SET IDENTITY_INSERT [TeleoptiAnalytics_Demo].dbo.agent_info OFF

SET IDENTITY_INSERT [TeleoptiAnalytics_Demo].dbo.queues ON
INSERT [TeleoptiAnalytics_Demo].[dbo].[queues]
([queue]
      ,[orig_desc]
      ,[log_object_id]
      ,[orig_queue_id]
      ,[display_desc]
      )
SELECT [queue]
      ,[orig_desc]
      ,[log_object_id]
      ,[orig_queue_id]
      ,[display_desc]
  FROM [TeleoptiAgg_Demo].[dbo].[queues]
SET IDENTITY_INSERT [TeleoptiAnalytics_Demo].dbo.queues OFF

INSERT [TeleoptiAnalytics_Demo].[dbo].[queue_logg]
SELECT [queue]
      ,[date_from]
      ,[interval]
      ,[offd_direct_call_cnt]
      ,[overflow_in_call_cnt]
      ,[aband_call_cnt]
      ,[overflow_out_call_cnt]
      ,[answ_call_cnt]
      ,[queued_and_answ_call_dur]
      ,[queued_and_aband_call_dur]
      ,[talking_call_dur]
      ,[wrap_up_dur]
      ,[queued_answ_longest_que_dur]
      ,[queued_aband_longest_que_dur]
      ,[avg_avail_member_cnt]
      ,[ans_servicelevel_cnt]
      ,[wait_dur]
      ,[aband_short_call_cnt]
      ,[aband_within_sl_cnt]
  FROM [TeleoptiAgg_Demo].[dbo].[queue_logg]


INSERT [TeleoptiAnalytics_Demo].[dbo].[agent_logg]
SELECT [queue]
      ,[date_from]
      ,[interval]
      ,[agent_id]
      ,[agent_name]
      ,[avail_dur]
      ,[tot_work_dur]
      ,[talking_call_dur]
      ,[pause_dur]
      ,[wait_dur]
      ,[wrap_up_dur]
      ,[answ_call_cnt]
      ,[direct_out_call_cnt]
      ,[direct_out_call_dur]
      ,[direct_in_call_cnt]
      ,[direct_in_call_dur]
      ,[transfer_out_call_cnt]
      ,[admin_dur]
  FROM [TeleoptiAgg_Demo].[dbo].[agent_logg]
