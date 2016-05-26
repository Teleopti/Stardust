IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'CalculateBadges' AND jobstep_id=92)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(92,N'CalculateBadges')
GO