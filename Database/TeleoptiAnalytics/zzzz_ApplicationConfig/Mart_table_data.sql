/****** Object:  Table [mart].[etl_job]    Script Date: 02/09/2009 14:14:54 ******/
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (1, N'Initial' )
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (2, N'Schedule')
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (3, N'Queue Statistics')
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (4, N'Forecast')
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (5, N'Agent Statistics')
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (6, N'KPI')
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (7, N'Permission')
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (8, N'Person Skill')
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (9, N'Main')
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (10, N'Workload Queues')

/****** Object:  Table [mart].[etl_jobstep]    Script Date: 02/09/2009 14:14:54 ******/
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (1, N'stg_date')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (2, N'stg_person')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (3, N'stg_agent_skill')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (4, N'stg_activity')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (5, N'stg_absence')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (6, N'stg_scenario')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (7, N'stg_shift_category')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (8, N'stg_schedule, stg_contract, stg_schedule_day_absence_count')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (9, N'stg_workload, stg_queue_workload')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (10, N'stg_forecast_workload')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (11, N'stg_kpi')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (12, N'stg_scorecard')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (13, N'stg_scorecard_kpi')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (14, N'stg_kpi_targets_team')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (15, N'stg_permission')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (16, N'dim_business_unit')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (17, N'dim_date')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (18, N'dim_site')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (19, N'dim_team')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (20, N'dim_skill')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (21, N'dim_skillset')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (22, N'dim_person')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (23, N'dim_activity')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (24, N'dim_absence')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (25, N'dim_scenario')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (26, N'dim_shift_category')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (27, N'dim_shift_length')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (28, N'dim_workload')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (29, N'dim_queue')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (30, N'dim_acd_login')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (31, N'dim_kpi')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (32, N'dim_scorecard')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (33, N'scorecard_kpi')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (34, N'bridge_skillset_skill')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (35, N'bridge_acd_login_person')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (36, N'bridge_queue_workload')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (37, N'fact_schedule')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (38, N'fact_contract')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (39, N'fact_queue')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (40, N'fact_forecast_workload')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (41, N'fact_agent')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (42, N'fact_agent_queue')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (43, N'fact_kpi_targets_team')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (44, N'permission_report')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (45, N'stg_business_unit')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (46, N'fact_schedule_day_count')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (47, N'bridge_time_zone')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (48, N'dim_interval')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (49, N'dim_time_zone')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (50, N'fact_schedule_deviation')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (51, N'fact_schedule_forecast_skill')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (52, N'stg_time_zone_bridge')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (53, N'stg_time_zone')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (54, N'fact_schedule_day_count')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (55, N'dim_day_off')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (56, N'stg_schedule_day_off_count')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (57, N'stg_schedule_forecast_skill')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (58, N'stg_user')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (59, N'aspnet_Users, aspnet_Membership')

/****** Object:  Table [mart].[etl_job_schedule]    Script Date: 02/09/2009 14:14:54 ******/
SET IDENTITY_INSERT [mart].[etl_job_schedule] ON
INSERT [mart].[etl_job_schedule] ([schedule_id], [schedule_name], [enabled], [schedule_type], [occurs_daily_at], [occurs_every_minute], [recurring_starttime], [recurring_endtime], [etl_job_name], [etl_datasource_id], [description])
VALUES (-1, N'Manual', 1, 0, 0, 0, 0, 0, N'Not Defined', -1, NULL)
SET IDENTITY_INSERT [mart].[etl_job_schedule] OFF

/****** Object:  Table [mart].[service_level_calculation]    Script Date: 02/11/2009 11:00:50 ******/
SET IDENTITY_INSERT [mart].[service_level_calculation] ON
INSERT [mart].[service_level_calculation] ([service_level_id], [service_level_name], [term_id]) VALUES (1, N'Answered Calls Within Service Level Threshold /Offered Calls ', 12)
INSERT [mart].[service_level_calculation] ([service_level_id], [service_level_name], [term_id]) VALUES (2, N'Answered and Abandoned Calls Within Service Level Threshold /Offered Calls', 13)
INSERT [mart].[service_level_calculation] ([service_level_id], [service_level_name], [term_id]) VALUES (3, N'Answered Calls Within Service Level Threshold / Answered Calls', 14)
SET IDENTITY_INSERT [mart].[service_level_calculation] OFF
/****** Object:  Table [mart].[adherence_calculation]    Script Date: 02/11/2009 11:00:50 ******/
SET IDENTITY_INSERT [mart].[adherence_calculation] ON
INSERT [mart].[adherence_calculation] ([adherence_id], [adherence_name], [term_id]) VALUES (1, N'Ready Time vs. Scheduled Ready Time', 9)
INSERT [mart].[adherence_calculation] ([adherence_id], [adherence_name], [term_id]) VALUES (2, N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)', 10)
INSERT [mart].[adherence_calculation] ([adherence_id], [adherence_name], [term_id]) VALUES (3, N'Ready Time vs. Contracted Schedule Time', 11)
SET IDENTITY_INSERT [mart].[adherence_calculation] OFF
/****** Object:  Table [mart].[report_group]    Script Date: 02/11/2009 11:00:50 ******/
INSERT [mart].[report_group] ([group_id], [group_name], [group_name_resource_key]) VALUES (1, N'Group 1', N'ResourceGroup1')
/****** Object:  Table [mart].[period_type]    Script Date: 02/11/2009 11:00:50 ******/
SET IDENTITY_INSERT [mart].[period_type] ON
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [term_id]) VALUES (1, N'Interval', 5)
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [term_id]) VALUES (2, N'Half hour', 4)
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [term_id]) VALUES (3, N'Hour', 3)
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [term_id]) VALUES (4, N'Day', 6)
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [term_id]) VALUES (5, N'Week', 7)
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [term_id]) VALUES (6, N'Month', 8)
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [term_id]) VALUES (7, N'Weekday', 31)
SET IDENTITY_INSERT [mart].[period_type] OFF
/****** Object:  Table [mart].[aspnet_applications]    Script Date: 02/11/2009 11:00:50 ******/
INSERT [dbo].[aspnet_applications] ([ApplicationName], [LoweredApplicationName], [ApplicationId], [Description]) VALUES (N'/', N'/', N'196a4451-8580-46bd-807a-1dbf027f970a', NULL)
/****** Object:  Table [mart].[report_control]    Script Date: 02/11/2009 11:00:50 ******/
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (1, N'dateFrom', N'1', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (2, N'dateTo', N'1', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (3, N'cboSite', N'mart.report_control_site_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (4, N'cboTeam', N'mart.report_control_team_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (5, N'cboAgent', N'mart.report_control_agent_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (6, N'dateDate', N'1', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (7, N'cboIntervalHourFrom', N'mart.report_control_hour_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (8, N'cboIntervalHourTo', N'mart.report_control_hour_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (9, N'cboSkill', N'mart.report_control_skill_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (10, N'twolistWorkload', N'mart.report_control_workload_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (11, N'cboIntervalType', N'mart.report_control_interval_type_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (12, N'cboIntervalFrom', N'mart.report_control_interval_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (13, N'cboIntervalTo', N'mart.report_control_interval_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (14, N'cboScenario', N'mart.report_control_scenario_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (15, N'twolistSkill', N'mart.report_control_twolist_skill_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (16, N'cboPersonCatType', N'mart.report_control_person_category_type_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (17, N'cboPersonCat', N'mart.report_control_person_category_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (18, N'twolistAgent', N'mart.report_control_twolist_agent_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (19, N'cboAdherenceCalc', N'mart.report_control_adherence_calculation_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (20, N'cboServiceLevelCalc', N'mart.report_control_service_level_calculation_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (21, N'cboSortAdhRep', N'mart.report_control_sort_by_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (22, N'cboTimeZone', N'mart.report_control_time_zone', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (23, N'twolistQueue', N'mart.report_control_twolist_queue_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (24, N'twolistoptActivity', N'mart.report_control_twolist_activity_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (25, N'twolistoptAbsence', N'mart.report_control_twolist_absence_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (26, N'twolistShiftCat', N'mart.report_control_twolist_shift_category_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (27, N'twolistoptDayOff', N'mart.report_control_twolist_day_off_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (28, N'twolistoptShiftCat', N'mart.report_control_twolist_shift_category_get', NULL)
/****** Object:  Table [mart].[aspnet_SchemaVersions]    Script Date: 02/11/2009 11:00:50 ******/
INSERT [dbo].[aspnet_SchemaVersions] ([Feature], [CompatibleSchemaVersion], [IsCurrentVersion]) VALUES (N'common', N'1', 1)
INSERT [dbo].[aspnet_SchemaVersions] ([Feature], [CompatibleSchemaVersion], [IsCurrentVersion]) VALUES (N'membership', N'1', 1)
/****** Object:  Table [mart].[language]    Script Date: 02/11/2009 11:00:50 ******/
INSERT [mart].[language] ([language_id], [language_name]) VALUES (1025, N'Arabic')
INSERT [mart].[language] ([language_id], [language_name]) VALUES (1033, N'English')
INSERT [mart].[language] ([language_id], [language_name]) VALUES (1040, N'Italian')
INSERT [mart].[language] ([language_id], [language_name]) VALUES (1053, N'Swedish')
/****** Object:  Table [mart].[language_term]    Script Date: 02/11/2009 11:00:50 ******/
SET IDENTITY_INSERT [mart].[language_term] ON
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (1, N'All')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (2, N'Not Defined')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (3, N'Hour')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (4, N'Half Hour')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (5, N'Interval')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (6, N'Day')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (7, N'Week')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (8, N'Month')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (9, N'Ready Time vs Scheduled Ready Time')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (10, N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (11, N'Ready Time vs. Contracted Schedule Time')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (12, N'Answered Calls Within Service Level Threshold /Offered Calls ')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (13, N'Answered and Abandoned Calls Within Service Level Threshold /Offered Calls')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (14, N'Answered Calls Within Service Level Threshold / Answered Calls')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (15, N'First Name')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (16, N'Last Name')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (17, N'Shift Start Time')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (18, N'Adherence')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (19, N'Monday')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (20, N'Tuesday')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (21, N'Wednesday')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (22, N'Thursday')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (23, N'Friday')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (24, N'Saturday')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (25, N'Sunday')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (26, N'Activity')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (27, N'Absence')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (28, N'Day Off')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (29, N'Shift Category')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (30, N'Extended Preference')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (31, N'Weekday')
SET IDENTITY_INSERT [mart].[language_term] OFF
/****** Object:  Table [mart].[language_translation]    Script Date: 02/11/2009 11:00:50 ******/
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 1, N'All', N'????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 2, N'Not Defined', N'??? ????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 3, N'Hour', N'????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 4, N'Half Hour', N'??? ????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 5, N'Interval', N'?????? ??????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 6, N'Day', N'???')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 7, N'Week', N'?????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 8, N'Month', N'???')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 9, N'Ready Time vs Scheduled Ready Time', N'??? ????????? ????? ??? ????????? ???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 10, N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)', N'??? ????????? ????? ????? ??????? (??????? ????? ??? ??? ???? ????? ?????)')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 11, N'Ready Time ˇvs. Scheduled Contract Time (i.e excluding lunch)', N'??? ????????? ????? ??? ????? ???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 12, N'Answered Calls Within Service Level Threshold / Offered Calls', N'???? ????? ?????? ???? ????????? ???? ?? ???? ????? / ????????? ???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 13, N'Answered and Abandoned Calls Within Service Level Threshold / Offered Calls', N'???? ????? ?????? ???? ????????? ???? ?? ???? ????? ?????????? ??? ???????? / ????????? ???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 14, N'Answered Calls Within Service Level Threshold / Answered Calls', N'???? ????? ?????? ???? ????????? ???? ?? ???? ????? / ????????? ???? ?? ???? ?????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 15, N'First Name', N'????? ?????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 16, N'Last Name', N'??? ???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 17, N'Shift Start Time', N'??? ??? ???? ???')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 18, N'Adherence', N'????????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 19, N'Monday', N'???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 20, N'Tuesday', N'????????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 21, N'Wednesday', N'????????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 22, N'Thursday', N'??????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 23, N'Friday', N'??????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 24, N'Saturday', N'?????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 25, N'Sunday', N'?????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 26, N'Activity', N'????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 27, N'Absence', N'????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 28, N'Day Off', N'????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 1, N'All', N'All')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 2, N'Not Defined', N'Not Defined')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 3, N'Hour', N'Hour')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 4, N'Half Hour', N'Half Hour')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 5, N'Interval', N'Interval')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 6, N'Day', N'Day')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 7, N'Week', N'Week')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 8, N'Month', N'Month')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 9, N'Ready Time vs Scheduled Ready Time', N'Ready Time vs Scheduled Ready Time')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 10, N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)', N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 11, N'Ready Time  vs. Scheduled Contract Time (i.e excluding lunch)', N'Ready Time vs. Contracted Schedule Time')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 12, N'Answered Calls Within Service Level Threshold / Offered Calls ', N'Answered Calls Within Service Level Threshold / Offered Calls ')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 13, N'Answered and Abandoned Calls Within Service Level Threshold / Offered Calls', N'Answered and Abandoned Calls Within Service Level Threshold / Offered Calls')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 14, N'Answered Calls Within Service Level Threshold / Answered Calls', N'Answered Calls Within Service Level Threshold / Answered Calls')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 15, N'First Name', N'First Name')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 16, N'Last Name', N'Last Name')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 17, N'Shift Start Time', N'Shift Start Time')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 18, N'Adherence', N'Adherence')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 19, N'Monday', N'Monday')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 20, N'Tuesday', N'Tuesday')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 21, N'Wednesday', N'Wednesday')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 22, N'Thursday', N'Thursday')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 23, N'Friday', N'Friday')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 24, N'Saturday', N'Saturday')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 25, N'Sunday', N'Sunday')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 26, N'Activity', N'Activity')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 27, N'Absence', N'Absence')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 28, N'Day Off', N'Day Off')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 29, N'Shift Category', N'Shift Category')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 30, N'Extended Preference', N'Extended Preference')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 31, N'Weekday', N'Weekday')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 1, N'All', N'Tutti')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 2, N'Not Defined', N'Non Definito')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 3, N'Hour', N'Ora')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 4, N'Half Hour', N'Mezz''ora')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 5, N'Interval', N'Intervallo')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 6, N'Day', N'Giorno')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 7, N'Week', N'Settimana')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 8, N'Month', N'Mese')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 9, N'Ready Time vs Scheduled Ready Time', N'Tempo Pronto / Tempo Pronto Programmato')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 10, N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)', N'Tempo Pronto / Tempo Programmato (incl. prima e dopo inizio turno)')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 11, N'Ready Time ˇvs. Scheduled Contract Time (i.e excluding lunch)', N'Tempo Pronto / Tempo Programmato')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 12, N'Answered Calls Within Service Level Threshold / Offered Calls', N'Chiamate Risposte entro Soglia Livello di Servizio / Chiamate Offerte')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 13, N'Answered and Abandoned Calls Within Service Level Threshold / Offered Calls', N'Chiamate Risposte e Abbandonate entro Soglia Livello di Servizio / Chiamate Offerte')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 14, N'Answered Calls Within Service Level Threshold / Answered Calls', N'Chiamate Risposte entro Soglia Livello di Servizio / Risposte')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 15, N'First Name', N'Nome')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 16, N'Last Name', N'Cognome')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 17, N'Shift Start Time', N'Ora Inizio Turno')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 18, N'Adherence', N'Presenza')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 19, N'Monday', N'Lunedç')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 20, N'Tuesday', N'Martedç')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 21, N'Wednesday', N'Mercoledç')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 22, N'Thursday', N'Giovedç')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 23, N'Friday', N'Venerdç')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 24, N'Saturday', N'Sabato')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 25, N'Sunday', N'Domenica')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 26, N'Activity', N'AttivitÖ')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 27, N'Absence', N'Assenza')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 28, N'Day Off', N'Giorno Libero')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 1, N'All', N'Alla')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 2, N'Not Defined', N'Ej Definierad')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 3, N'Hour', N'Timme')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 4, N'Half Hour', N'Halvtimme')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 5, N'Interval', N'Intervall')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 6, N'Day', N'Dag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 7, N'Week', N'Vecka')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 8, N'Month', N'MÜnad')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 9, N'Ready Time vs Scheduled Ready Time', N'Ready tid vs Schemalagd ready tid')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 10, N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)', N'Ready tid vs Schemalagd tid (inkl tid fîre och efter skiftstart)')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 11, N'Ready Time  vs. Scheduled Contract Time (i.e excluding lunch)', N'Ready tid vs Schemalagd konstraktstid')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 12, N'Answered Calls Within Service Level Threshold / Offered Calls ', N'Besvarade samtal inom svarsmÜl / Inkommande samtal')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 13, N'Answered and Abandoned Calls Within Service Level Threshold / Offered Calls', N'Besvarade och tappade samtal inom svarsmÜl / Inkommande samtal')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 14, N'Answered Calls Within Service Level Threshold / Answered Calls', N'Besvarade samtal inom svarsmÜl / Besvarade samtal')

INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 15, N'First Name', N'Fîrnamn')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 16, N'Last Name', N'Efternamn')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 17, N'Shift Start Time', N'Skiftstart')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 18, N'Adherence', N'Fîljsamhet')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 19, N'Monday', N'mÜndag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 20, N'Tuesday', N'tisdag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 21, N'Wednesday', N'onsdag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 22, N'Thursday', N'torsdag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 23, N'Friday', N'fredag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 24, N'Saturday', N'lîrdag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 25, N'Sunday', N'sîndag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 26, N'Activity', N'Aktivitet')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 27, N'Absence', N'FrÜnvaro')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 28, N'Day Off', N'Fridag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 29, N'Shift Category', N'Skiftkategori')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 30, N'Extended Preference', N'Utîkat înskemÜl')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 31, N'Weekday', N'Veckodag')
/****** Object:  Table [mart].[report]    Script Date: 02/11/2009 11:00:50 ******/
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (1, 19, 1, N'~/Selection.aspx?ReportID=1', N'_self', N'Preferences per Day', N'ResReportPreferencesPerDay', 1, N'~/Reports/CCC/report_schedule_preferences_per_day.rdlc', 1000, N'mart.report_data_schedule_preferences_per_day', N'f01_Report_PreferencesPerDay.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (2, 19, 1, N'~/Selection.aspx?ReportID=2', N'_self', N'Preferences Per Agent', N'ResReportPreferencesPerAgent', 1, N'~/Reports/CCC/report_schedule_preferences_per_agent.rdlc', 1000, N'mart.report_data_schedule_preferences_per_agent', N'f01_Report_PreferencesPerAgent.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (3, 21, 1, N'~/Selection.aspx?ReportID=3', N'_self', N'IMPROVE', N'ResReportImprove', 1, N'~/Reports/CCC/report_IMPROVE.rdlc', 1000, N'mart.report_data_IMPROVE', N'/f01:Report+ImproveReport', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (5, 1, 1, N'~/Reports/CCC/AgentScorecardDefault.aspx', NULL, N'Agent Scorecard Compact', N'ResreportAgentScoercard', 1, N'', 1000, N'', N'', N' ', N' ', N' ', N' ')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (6, 1, 1, N'~/Reports/CCC/AgentScorecard.aspx', N'main', N'Agent Scorecard', N'ResReportAgentScorecard', 1, N'', 1000, N'', N'', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (7, 5, 1, N'~/Selection.aspx?ReportID=7', N'_self', N'Forecast vs Scheduled Hours', N'ResReportForecastvsScheduledHours', 1, N'~/Reports/CCC/report_forecast_vs_scheduled_hours.rdlc', 1000, N'mart.report_data_forecast_vs_scheduled_hours', N'f01_Report_ForecastvsScheduledHours.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (8, 4, 1, N'~/Selection.aspx?ReportID=8', N'_self', N'Abandonment and Speed of Answer', N'ResReportAbandonmentAndSpeedOfAnswer', 1, N'~/Reports/CCC/report_queue_stat_abnd.rdlc', 1000, N'mart.report_data_queue_stat_abnd', N'f01_Report_AbandonmentAndSpeedOfAnswer.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (9, 9, 1, N'~/Selection.aspx?ReportID=9', N'_self', N'Service Level and Agents Ready', N'ResReportServiceLevelAndAgentsReady', 1, N'~/Reports/CCC/report_service_level_agents_ready.rdlc', 1000, N'mart.report_data_service_level_agents_ready', N'f01_Report_ServiceLevelAndAgentsReady.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (10, 6, 1, N'~/Selection.aspx?ReportID=10', N'_self', N'Forecast vs Actual Workload', N'ResReportForecastvsActualWorkload', 1, N'~/Reports/CCC/report_forecast_vs_actual_workload.rdlc', 1000, N'mart.report_data_forecast_vs_actual_workload', N'f01_Report_ForecastvsActualWorkload.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (11, 18, 1, N'~/Selection.aspx?ReportID=11', N'_self', N'Team Metrics', N'ResReportTeamMetrics', 1, N'~/Reports/CCC/report_team_metrics.rdlc', 1000, N'mart.report_data_team_metrics', N'f01_Report_TeamMetrics.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (12, 8, 1, N'~/Selection.aspx?ReportID=12', N'_self', N'Agent Metrics', N'ResReportAgentScheduleResult', 1, N'~/Reports/CCC/report_agent_schedule_result.rdlc', 1000, N'mart.report_data_agent_schedule_result', N'f01_Report_AgentScheduleResult.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (13, 10, 1, N'~/Selection.aspx?ReportID=13', N'_self', N'Agent Schedule Adherence', N'ResReportAgentScheduleAdherence', 1, N'~/Reports/CCC/report_agent_schedule_adherence.aspx', 1000, N'mart.report_data_agent_schedule_adherence', N'f01_Report_AgentScheduleAdherence.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (14, 11, 1, N'~/Selection.aspx?ReportID=14', N'_self', N'Queue Statistics', N'ResReportQueueStatistics', 1, N'~/Reports/CCC/report_queue_stat_raw.rdlc', 1000, N'mart.report_data_queue_stat_raw', N'f01_Report_QueueStatistics.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (15, 12, 1, N'~/Selection.aspx?reportID=15', N'_self', N'Agent Queue Statistics', N'ResReportAgentQueueStatistics', 1, N'~/Reports/CCC/report_agent_queue_stat_raw.rdlc', 1000, N'mart.report_data_agent_queue_stat_raw', N'f01_Report_AgentQueueStatistics.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (16, 13, 1, N'~/Selection.aspx?ReportID=16', N'_self', N'Agent Statistics', N'ResReportAgentStatistics', 1, N'~/Reports/CCC/report_agent_stat_raw.rdlc', 1000, N'mart.report_data_agent_stat_raw', N'f01_Report_AgentStatistics.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (17, 14, 1, N'~/Selection.aspx?ReportID=17', N'_self', N'Scheduled Time per Agent', N'ResReportScheduledTimePerAgent', 1, N'~/Reports/CCC/report_scheduled_time_per_agent.rdlc', 1000, N'mart.report_data_scheduled_time_per_agent', N'f01_Report_ScheduledTimePerAgent.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (18, 17, 1, N'~/Selection.aspx?ReportID=18', N'_self', N'Scheduled Time per Activity', N'ResReportScheduledTimePerActivity', 1, N'~/Reports/CCC/report_scheduled_time_per_activity.rdlc', 1000, N'mart.report_data_scheduled_time_per_activity', N'f01_Report_ScheduledTimePerActivity.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (19, 15, 1, N'~/Selection.aspx?ReportID=19', N'_self', N'Shift Category per Day', N'ResReportShiftCategoryPerDay', 1, N'~/Reports/CCC/report_shift_category_per_day.rdlc', 1000, N'mart.report_data_shift_category_per_day', N'f01_Report_ShiftCategoryPerDay.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (20, 16, 1, N'~/Selection.aspx?ReportID=20', N'_self', N'Shift Category and Full Day Absences per Agent', N'ResReportShiftCategoryAndDayAbsencePerAgent', 1, N'~/Reports/CCC/report_shift_category_and_day_absences_per_agent.rdlc', 1000, N'mart.report_data_shift_cat_and_day_abs_per_agent', N'f01_Report_ShiftCategoryAndFullDayAbsencePerAgent.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (21, 20, 1, N'~/Selection.aspx?ReportID=21', N'_self', N'Scheduled Agents per Interval and Team', N'ResReportScheduledAgentsPerIntervalAndTeam', 1, N'~/Reports/CCC/report_scheduled_agents_per_interval_and_team.rdlc', 1000, N'mart.report_data_scheduled_agents_per_interval_and_team', N'f01_Report_ScheduledAgentsPerIntervalAndTeam.html', N'', N'', N'', N'')
/****** Object:  Table [mart].[report_control_collection]    Script Date: 02/11/2009 11:00:50 ******/
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (1, 1, 0, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (2, 1, 1, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 1, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (3, 1, 2, 3, N'1', N'ResSiteNameColon', NULL, N'@site_id', 1, 2, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (4, 1, 3, 4, N'1', N'ResTeamNameColon', NULL, N'@team_id', 1, 2, 3, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (5, 2, 0, 6, N'12:00', N'ResDateColon', NULL, N'@date', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (6, 2, 1, 7, N'8', N'ResIntervalColon', NULL, N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (7, 2, 2, 8, N'17', N'ResIntervalColon', NULL, N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (8, 2, 3, 9, N'1', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (9, 2, 4, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 8, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (10, 3, 0, 6, N'12:00', N'ResDateColon', NULL, N'@date', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (11, 3, 1, 11, N'3', N'ResIntervalType', NULL, N'@interval_type', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (12, 3, 2, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (13, 3, 3, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (14, 3, 4, 9, N'-2', N'ResSkillColon', NULL, N'@skill_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (15, 3, 5, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 14, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (17, 4, 1, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (18, 4, 2, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 17, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (19, 4, 3, 11, N'4', N'ResIntervalType', NULL, N'@interval_type', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (20, 4, 4, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (21, 4, 5, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 20, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (22, 4, 6, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (23, 4, 7, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (24, 5, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (25, 5, 2, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (26, 5, 3, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (27, 5, 4, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 26, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (28, 5, 5, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (29, 5, 6, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (30, 6, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (31, 6, 2, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (32, 6, 3, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 31, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (33, 6, 4, 11, N'4', N'ResIntervalType', NULL, N'@interval_type', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (34, 6, 5, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (35, 6, 6, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 34, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (36, 6, 7, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (37, 6, 8, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (38, 7, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (39, 7, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (40, 7, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 39, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (41, 7, 4, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (42, 7, 5, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (43, 7, 6, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 39, 40, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (44, 7, 7, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 39, 40, 43, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (46, 8, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (47, 8, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (48, 8, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 47, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (49, 8, 4, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (50, 8, 5, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (51, 8, 6, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 47, 48, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (52, 8, 7, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 47, 48, 51, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (53, 8, 10, 18, N'-99', N'ResAgentsColon', NULL, N'@agent_set', 47, 48, 51, 52)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (54, 8, 11, 19, N'1', N'ResAdherenceCalculationColon', NULL, N'@adherence_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (55, 9, 1, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (56, 9, 2, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 55, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (57, 9, 3, 11, N'4', N'ResIntervalType', NULL, N'@interval_type', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (58, 9, 4, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (59, 9, 5, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 58, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (60, 9, 6, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (61, 9, 7, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (62, 9, 8, 20, N'1', N'ResServiceLevelCalcColon', NULL, N'@sl_calc_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (63, 10, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (64, 10, 2, 6, N'12:00', N'ResDateColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (65, 10, 3, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 64, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (66, 10, 4, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 64, 65, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (67, 10, 5, 19, N'1', N'ResAdherenceCalculationColon', NULL, N'@adherence_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (68, 10, 6, 21, N'1', N'ResSortByColon', NULL, N'@sort_by', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (69, 8, 12, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (70, 11, 1, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (71, 11, 2, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 70, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (72, 11, 3, 23, N'-99', N'ResQueueColon', NULL, N'@queue_set', 70, 71, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (73, 11, 4, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (74, 11, 5, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 73, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (75, 11, 6, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (76, 11, 7, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (77, 11, 8, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (78, 10, 7, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (79, 9, 9, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (80, 4, 8, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (81, 5, 7, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (82, 6, 9, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (83, 12, 1, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (84, 12, 2, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 83, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (85, 12, 3, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (86, 12, 4, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 85, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (87, 12, 5, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (88, 12, 6, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (89, 12, 7, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 85, 86, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (90, 12, 8, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 85, 86, 89, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (91, 12, 9, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_id', 85, 86, 89, 90)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (92, 12, 10, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (93, 13, 1, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (94, 13, 2, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 93, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (95, 13, 3, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (96, 13, 4, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (97, 13, 5, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 93, 94, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (98, 13, 6, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 93, 94, 97, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (99, 13, 7, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_id', 93, 94, 97, 98)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (100, 13, 8, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (101, 14, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (102, 14, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (103, 14, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 102, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (104, 14, 4, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (105, 14, 5, 13, N'287', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (106, 14, 6, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 102, 103, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (107, 14, 7, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 102, 103, 106, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (108, 14, 8, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_id', 102, 103, 106, 107)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (111, 14, 10, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (112, 15, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (113, 15, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (114, 15, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 113, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (115, 15, 4, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 113, 114, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (116, 15, 5, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 113, 114, 115, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (117, 15, 6, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_id', 113, 114, 115, 116)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (118, 15, 7, 26, N'-99', N'ResShiftCategoryColon', NULL, N'@shift_category_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (119, 15, 8, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (120, 16, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (121, 16, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (122, 16, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 121, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (123, 16, 4, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 121, 122, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (124, 16, 5, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 121, 122, 123, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (125, 16, 6, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_id', 121, 122, 123, 124)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (126, 16, 7, 28, N'-99', N'ResShiftCategoryColon', NULL, N'@shift_category_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (127, 16, 8, 27, N'-1', N'ResDayOffColon', NULL, N'@day_off_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (128, 16, 9, 25, N'-1', N'ResAbsenceColon', NULL, N'@absence_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (129, 16, 10, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (130, 17, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (131, 17, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 130, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (132, 17, 6, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 130, 131, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (133, 17, 7, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 130, 131, 132, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (134, 17, 8, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_id', 130, 131, 132, 133)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (135, 17, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (136, 17, 10, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (137, 17, 9, 24, N'-99', N'ResActivityColon', NULL, N'@activity_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (138, 17, 4, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (139, 17, 5, 13, N'287', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (140, 18, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (141, 18, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 140, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (142, 18, 6, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 140, 141, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (143, 18, 7, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 140, 141, 142, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (145, 18, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (146, 18, 9, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (147, 18, 8, 19, N'1', N'ResAdherenceCalculationColon', NULL, N'@adherence_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (148, 18, 4, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (149, 18, 5, 13, N'287', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (150, 19, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (151, 19, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (152, 19, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 151, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (153, 19, 4, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 151, 152, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (154, 19, 5, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 151, 152, 153, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (155, 19, 6, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_id', 151, 152, 153, 154)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (156, 19, 7, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (157, 20, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (158, 20, 2, 6, N'12:00', N'ResDateColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (159, 20, 3, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (160, 20, 4, 13, N'287', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (161, 20, 5, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 158, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (162, 20, 6, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 158, 161, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (164, 20, 8, 24, N'-99', N'ResActivityColon', NULL, N'@activity_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (165, 20, 9, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (166, 21, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (167, 21, 2, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (168, 21, 3, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 167, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (169, 21, 4, 11, N'4', N'ResIntervalType', NULL, N'@interval_type', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (170, 21, 5, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (171, 21, 6, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 170, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (172, 21, 7, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (173, 21, 8, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (174, 21, 9, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 170, 171, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (175, 21, 10, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 170, 171, 174, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (176, 21, 11, 19, N'1', N'ResAdherenceCalculationColon', NULL, N'@adherence_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (177, 21, 12, 20, N'1', N'ResServiceLevelCalcColon', NULL, N'@sl_calc_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (178, 21, 13, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
GO

--Crossdatabase view inserts
--KJ 20090212
INSERT INTO mart.sys_crossdatabaseview_target
VALUES (4,'TeleoptiCCCAgg','TeleoptiCCCAgg', 0)
GO
SET IDENTITY_INSERT [mart].[sys_crossdatabaseview] ON
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (1, N'v_log_object', N'SELECT * FROM $$$target$$$.dbo.log_object', 4)
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (2, N'v_agent_logg', N'SELECT * FROM $$$target$$$.dbo.agent_logg', 4)
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (3, N'v_agent_info', N'SELECT * FROM $$$target$$$.dbo.agent_info', 4)
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (4, N'v_queues', N'SELECT * FROM $$$target$$$.dbo.queues', 4)
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (5, N'v_queue_logg', N'SELECT * FROM $$$target$$$.dbo.queue_logg', 4)
SET IDENTITY_INSERT [mart].[sys_crossdatabaseview] OFF
GO

-- Not Defined data source or manual data source

SET IDENTITY_INSERT [mart].[sys_datasource] ON

INSERT INTO [mart].[sys_datasource]
	(
	datasource_id, 
	datasource_name, 
	log_object_id,
	log_object_name,
	datasource_database_id,
	datasource_database_name,
	datasource_type_name
	)
SELECT 
	datasource_id			=-1, 
	datasource_name			='Not Defined',
	log_object_id			= -1,
	log_object_name			='Not Defined',
	datasource_database_id	= -1,
	datasource_database_name= 'Not Defined',
	datasource_type_name	= 'Not Defined'
WHERE NOT EXISTS (SELECT * FROM [mart].[sys_datasource] where datasource_id = -1)

----------------------------------------------------------------------------
-- Insert TeleoptiCCC as a data source.
INSERT INTO [mart].[sys_datasource]
	(
	datasource_id, 
	datasource_name, 
	log_object_id,
	log_object_name,
	datasource_database_id,
	datasource_database_name,
	datasource_type_name
	)
SELECT 
	datasource_id			=1, 
	datasource_name			= 'TeleoptiCCC',
	log_object_id			= -1,
	log_object_name			= 'Not Defined',
	datasource_database_id	= 1,
	datasource_database_name= 'Raptor Default',
	datasource_type_name	= 'Raptor Default'
WHERE NOT EXISTS (SELECT * FROM [mart].[sys_datasource] where datasource_id = 1)

SET IDENTITY_INSERT [mart].[sys_datasource] OFF
GO

SET NOCOUNT ON
INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (1, N'TeleoptiBrokerService', N'Port', N'9080', N'System.Int32',suser_sname(), getdate())

INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (2, N'TeleoptiBrokerService', N'Server', N'127.0.0.1', N'System.String',suser_sname(),getdate())

INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (3, N'TeleoptiBrokerService', N'Threads', N'1', N'System.Int32',suser_sname(), getdate())

INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (4, N'TeleoptiBrokerService', N'Intervall', N'10000', N'System.Double',suser_sname(), getdate())

INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (5, N'TeleoptiBrokerService', N'ConnectionString', N'Data Source=server\instanceName;Initial Catalog=Messaging;Persist Security Info=True;User ID=sa;Password=somepassword', N'System.String',suser_sname(), getdate())

INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (6, N'TeleoptiBrokerService', N'GeneralThreadPoolThreads', N'3', N'System.Int32',suser_sname(), getdate())

INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (7, N'TeleoptiBrokerService', N'DatabaseThreadPoolThreads', N'3', N'System.Int32',suser_sname(), getdate())

INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (8, N'TeleoptiBrokerService', N'ReceiptThreadPoolThreads', N'3', N'System.Int32',suser_sname(), getdate())

INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (9, N'TeleoptiBrokerService', N'HeartbeatThreadPoolThreads', N'1', N'System.Int32',suser_sname(), getdate())

INSERT INTO [msg].[Address] ([MessageBrokerId],[MulticastAddress],[Port],[Direction])
VALUES (1,'235.235.235.234',9090,'Client')


GO
INSERT INTO [mart].[dim_preference_type]
           (
			   [preference_type_id],
			   [preference_type_name],
			   [resource_key]
           )
     VALUES
           (
				1,
				'Shift Category',
				'ResShiftCategory'
			)

INSERT INTO [mart].[dim_preference_type]
           (
			   [preference_type_id],
			   [preference_type_name],
			   [resource_key]
           )
     VALUES
           (
				2,
				'Day Off',
				'ResDayOff'
			)
INSERT INTO [mart].[dim_preference_type]
           (
			   [preference_type_id],
			   [preference_type_name],
			   [resource_key]
           )
     VALUES
           (
				3,
				'Extended',
				'ResExtendedPreference'
			)

GO
