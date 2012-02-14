/* 
Trunk initiated: 
2009-08-19 
14:02
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 

----------------  
--Name: Zoë Trender 
--Date: 2009-08-20  
--Desc: Adding a new control to a selection page
----------------  

DELETE FROM mart.report_control_collection
WHERE collection_id = 10

DECLARE @maxId int
SET @maxId = (SELECT MAX(control_Collection_id) FROM mart.report_control_collection)
SET @maxId = @maxId + 1

INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (63, 10, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (64, 10, 2, 6, N'12:00', N'ResDateColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (65, 10, 3, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 64, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (66, 10, 4, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 64, 65, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (@maxId, 10, 5, 5, -2, 'ResAgentsColon', NULL, '@agent_person_id', 64, 65, 66, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (67, 10, 6, 19, N'1', N'ResAdherenceCalculationColon', NULL, N'@adherence_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (68, 10, 7, 21, N'1', N'ResSortByColon', NULL, N'@sort_by', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (78, 10, 8, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
GO

----------------  
--Name: David Jonsson
--Date: 2009-08-26 
--Desc: Deleting obsoltet data
----------------  
delete from mart.dim_activity
where activity_code is null
and activity_id <> -1
 
GO 
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (142,'7.0.142') 
