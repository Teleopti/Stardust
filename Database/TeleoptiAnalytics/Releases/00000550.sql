IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'dim_day_off' AND jobstep_id=93)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(93,N'dim_day_off')
GO

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'stg_hourly_availability' AND jobstep_id=94)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(94,N'stg_hourly_availability')
GO

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'fact_hourly_availability' AND jobstep_id=95)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(95,N'fact_hourly_availability')
GO

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'fact_quality' AND jobstep_id=96)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(96,N'fact_quality')
GO

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'dim_person_windows_login' AND jobstep_id=97)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(97,N'dim_person_windows_login')
GO
