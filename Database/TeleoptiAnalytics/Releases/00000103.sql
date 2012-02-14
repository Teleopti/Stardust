/* 
BuildTime is: 
2009-05-28 
13:37
*/ 
/* 
Trunk initiated: 
2009-05-13 
16:46
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 

----------------  
--Name: JN  
--Date: 2009-05-18  
--Desc: Increase column length of msg.UpdateType.ChangedBy
----------------  
ALTER TABLE msg.UpdateType
ALTER COLUMN ChangedBy nvarchar(1000)
GO

----------------  
--Name: David Jonsson 
--Date: 2009-05-27
--Desc: Default data for msg broker  
----------------  
IF NOT EXISTS (SELECT 1 FROM msg.UpdateType WHERE UpdateType = 0)
	INSERT INTO msg.UpdateType SELECT 0,'Insert','Wise',GETDATE()

IF NOT EXISTS (SELECT 1 FROM msg.UpdateType WHERE UpdateType = 1)
	INSERT INTO msg.UpdateType SELECT 1,'Update','Wise',GETDATE()

IF NOT EXISTS (SELECT 1 FROM msg.UpdateType WHERE UpdateType = 2)
	INSERT INTO msg.UpdateType SELECT 2,'Delete','Wise',GETDATE()
	
IF NOT EXISTS (SELECT 1 FROM msg.UpdateType WHERE UpdateType = 3)
	INSERT INTO msg.UpdateType SELECT 3,'NotApplicable','Wise',GETDATE()

GO
----------------  
--Name: David Jonsson 
--Date: 2009-05-27
--Desc: New report: Absence Time Per Agent
----------------  
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (179, 22, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (180, 22, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (181, 22, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 180, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (182, 22, 4, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 180, 181, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (183, 22, 5, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 180, 181, 182, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (184, 22, 6, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_id', 180, 181, 182, 183)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (185, 22, 7, 25, N'-1', N'ResAbsenceColon', NULL, N'@absence_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (186, 22, 8, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
GO

INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (4, 22, 1, N'~/Selection.aspx?ReportID=4', N'_self', N'Absence per Agent', N'ResReportAbsenceTimePerAgent', 1, N'~/Reports/CCC/report_absence_time_per_agent.rdlc', 1000, N'mart.report_data_absence_time_per_agent', N'f01_Report_AbsenceTimeperAgent.html', N'', N'', N'', N'')
GO

----------------  
--Name: David Jonsson 
--Date: 2009-05-27
--Desc: Wrong Wiki link
----------------  
UPDATE mart.report
SET help_key = 'f01_Report_ImproveReport.html'
WHERE report_id=3
GO 
GO 
 
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (103,'7.0.103') 
