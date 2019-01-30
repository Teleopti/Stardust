-- New ETL Job and job step
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (17, 'Insights data refresh');
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (100, 'Trigger Insights data refresh');
