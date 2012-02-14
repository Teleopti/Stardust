/* 
Trunk initiated: 
2009-11-02 
08:07
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: JN
--Date: 2009-11-02  
--Desc: Clean up etl job and jobsteps.
----------------  
-- Delete all execution info and all job steps
DELETE FROM mart.etl_jobstep_execution
DELETE FROM mart.etl_jobstep_error
DELETE FROM mart.etl_jobstep
DELETE FROM mart.etl_job_execution

-- Update existing jobs
UPDATE mart.etl_job
SET job_name = 'Intraday',
	update_date = GETDATE()
WHERE job_id = 9

-- Update existing job schedules
UPDATE mart.etl_job_schedule
SET etl_job_name = 'Intraday'
WHERE etl_job_name = 'Main'

-- Insert new jobs
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (11, N'Nightly' )
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (12, N'Queue and Agent login synchronization' )
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (13, N'Cleanup' )
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (14, N'Process Cube' )

-- Insert all job steps
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (1, N'dim_interval')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (2, N'stg_date')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (3, N'dim_date')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (4, N'stg_time_zone')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (5, N'dim_time_zone')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (6, N'stg_time_zone_bridge')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (7, N'bridge_time_zone')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (8, N'stg_business_unit')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (9, N'dim_queue')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (10, N'dim_acd_login')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (11, N'TeleoptiCCC7.QueueSource')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (12, N'TeleoptiCCC7.ExternalLogOn')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (13, N'stg_person')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (14, N'stg_agent_skill')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (15, N'stg_activity')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (16, N'stg_absence')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (17, N'stg_scenario')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (18, N'stg_shift_category')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (19, N'stg_schedule, stg_schedule_day_absence_count')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (20, N'stg_schedule_forecast_skill')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (21, N'stg_schedule_day_off_count')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (22, N'stg_workload, stg_queue_workload')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (23, N'stg_forecast_workload')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (24, N'stg_kpi')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (25, N'stg_scorecard')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (26, N'stg_scorecard_kpi')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (27, N'stg_kpi_targets_team')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (28, N'stg_permission')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (29, N'stg_user')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (30, N'dim_business_unit')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (31, N'dim_site')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (32, N'dim_team')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (33, N'dim_skill')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (34, N'dim_skillset')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (35, N'dim_person')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (36, N'dim_activity')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (37, N'dim_absence')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (38, N'dim_scenario')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (39, N'dim_shift_category')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (40, N'dim_shift_length')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (41, N'dim_day_off')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (42, N'dim_workload')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (43, N'dim_kpi')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (44, N'dim_scorecard')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (45, N'scorecard_kpi')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (46, N'bridge_skillset_skill')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (47, N'bridge_acd_login_person')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (48, N'bridge_queue_workload')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (49, N'aspnet_Users, aspnet_Membership')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (50, N'fact_schedule')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (51, N'fact_schedule_day_count')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (52, N'fact_schedule_forecast_skill')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (53, N'fact_queue')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (54, N'fact_forecast_workload')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (55, N'fact_agent')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (56, N'fact_agent_queue')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (57, N'fact_schedule_deviation')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (58, N'fact_kpi_targets_team')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (59, N'permission_report')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (60, N'dim_person delete data')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (61, N'dim_person trim data')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (62, N'dim_scenario delete data')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (63, N'Performance Manager permissions')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (64, N'Process Cube')
 
GO 
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (169,'7.0.169') 
