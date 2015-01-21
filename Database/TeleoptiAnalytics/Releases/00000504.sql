----------------  
--Name: Karin and Maria
--Comment: Database toggle for index maintenance
----------------  
INSERT INTO mart.sys_configuration([key], [value])
SELECT  'RunIndexMaintenance','True'
GO
IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'AnalyticsIndexMaintenance' AND jobstep_id=88)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(88,N'AnalyticsIndexMaintenance')
GO
IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'AppIndexMaintenance' AND jobstep_id=89)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(89,N'AppIndexMaintenance')
GO
IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'AggIndexMaintenance' AND jobstep_id=90)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(90,N'AggIndexMaintenance')
GO



