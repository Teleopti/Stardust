----------------  
--Name: Jonas n
--Date: 2011-12-13  
--Desc: Add dbo.log_object_detail table
----------------
CREATE TABLE [dbo].[log_object_detail](
	[log_object_id] [int] NOT NULL,
	[detail_id] [int] NOT NULL,
	[detail_desc] [varchar](50) NOT NULL,
	[proc_name] [varchar](50) NULL,
	[int_value] [int] NULL,
	[date_value] [smalldatetime] NULL,
 CONSTRAINT [PK_log_object_detail] PRIMARY KEY CLUSTERED 
(
	[log_object_id] ASC,
	[detail_id] ASC
)
)

GO

ALTER TABLE [dbo].[log_object_detail]  WITH CHECK ADD  CONSTRAINT [FK_log_object_detail_log_object] FOREIGN KEY([log_object_id])
REFERENCES [dbo].[log_object] ([log_object_id])
GO

ALTER TABLE [dbo].[log_object_detail] CHECK CONSTRAINT [FK_log_object_detail_log_object]
GO

----------------  
--Name: Jonas n
--Date: 2011-12-19  
--Desc: Change to multiple group selection for some reports.
----------------
INSERT INTO mart.report_control_collection
SELECT '287', '28', '1', '14', '0', 'ResScenarioColon', NULL, '@scenario_id', NULL, NULL, NULL, NULL UNION ALL
SELECT '288', '28', '2', '1', '12:00', 'ResDateFromColon', NULL, '@date_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '289', '28', '3', '2', '12:00', 'ResDateToColon', NULL, '@date_to', '288', NULL, NULL, NULL UNION ALL
SELECT '290', '28', '4', '29', '-2', 'ResGroupPageColon', NULL, '@group_page_code', NULL, NULL, NULL, NULL UNION ALL
SELECT '291', '28', '5', '35', '-99', 'ResGroupPageGroupColon', NULL, '@group_page_group_set', '290', NULL, NULL, NULL UNION ALL
SELECT '292', '28', '6', '37', '-2', 'ResAgentColon', NULL, '@group_page_agent_code', '288', '289', '290', '291' UNION ALL
SELECT '293', '28', '7', '3', '-2', 'ResSiteNameColon', NULL, '@site_id', '288', '289', '290', NULL UNION ALL
SELECT '294', '28', '8', '34', '-99', 'ResTeamNameColon', NULL, '@team_set', '288', '289', '293', NULL UNION ALL
SELECT '295', '28', '9', '36', '-2', 'ResAgentsColon', NULL, '@agent_code', '288', '289', '293', '294' UNION ALL
SELECT '296', '28', '10', '22', '-1', 'ResTimeZoneColon', NULL, '@time_zone_id', NULL, NULL, NULL, NULL

INSERT INTO mart.report_control_collection
SELECT '297', '29', '1', '14', '0', 'ResScenarioColon', NULL, '@scenario_id', NULL, NULL, NULL, NULL UNION ALL
SELECT '298', '29', '2', '1', '12:00', 'ResDateFromColon', NULL, '@date_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '299', '29', '3', '2', '12:00', 'ResDateToColon', NULL, '@date_to', '298', NULL, NULL, NULL UNION ALL
SELECT '300', '29', '4', '12', '0', 'ResIntervalFromColon', '1', '@interval_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '301', '29', '5', '13', '-99', 'ResIntervalToColon', '2', '@interval_to', NULL, NULL, NULL, NULL UNION ALL
SELECT '302', '29', '6', '29', '-2', 'ResGroupPageColon', NULL, '@group_page_code', NULL, NULL, NULL, NULL UNION ALL
SELECT '303', '29', '7', '35', '-99', 'ResGroupPageGroupColon', NULL, '@group_page_group_set', '302', NULL, NULL, NULL UNION ALL
SELECT '304', '29', '8', '37', '-2', 'ResAgentColon', NULL, '@group_page_agent_code', '298', '299', '302', '303' UNION ALL
SELECT '305', '29', '9', '3', '-2', 'ResSiteNameColon', NULL, '@site_id', '298', '299', '302', NULL UNION ALL
SELECT '306', '29', '10', '34', '-99', 'ResTeamNameColon', NULL, '@team_set', '298', '299', '305', NULL UNION ALL
SELECT '307', '29', '11', '36', '-2', 'ResAgentsColon', NULL, '@agent_code', '298', '299', '305', '306' UNION ALL
SELECT '308', '29', '12', '24', '-99', 'ResActivityColon', NULL, '@activity_set', NULL, NULL, NULL, NULL UNION ALL
SELECT '309', '29', '13', '25', '-1', 'ResAbsenceColon', NULL, '@absence_set', NULL, NULL, NULL, NULL UNION ALL
SELECT '310', '29', '13', '22', '-1', 'ResTimeZoneColon', NULL, '@time_zone_id', NULL, NULL, NULL, NULL

INSERT INTO mart.report_control_collection
SELECT '311', '30', '1', '14', '0', 'ResScenarioColon', NULL, '@scenario_id', NULL, NULL, NULL, NULL UNION ALL
SELECT '312', '30', '2', '1', '12:00', 'ResDateFromColon', NULL, '@date_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '313', '30', '3', '2', '12:00', 'ResDateToColon', NULL, '@date_to', '312', NULL, NULL, NULL UNION ALL
SELECT '314', '30', '4', '12', '0', 'ResIntervalFromColon', '1', '@interval_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '315', '30', '5', '13', '-99', 'ResIntervalToColon', '2', '@interval_to', NULL, NULL, NULL, NULL UNION ALL
SELECT '316', '30', '6', '29', '-2', 'ResGroupPageColon', NULL, '@group_page_code', NULL, NULL, NULL, NULL UNION ALL
SELECT '317', '30', '7', '35', '-99', 'ResGroupPageGroupColon', NULL, '@group_page_group_set', '316', NULL, NULL, NULL UNION ALL
SELECT '318', '30', '8', '37', '-2', 'ResAgentColon', NULL, '@group_page_agent_code', '312', '313', '316', '317' UNION ALL
SELECT '319', '30', '9', '3', '-2', 'ResSiteNameColon', NULL, '@site_id', '312', '313', '316', NULL UNION ALL
SELECT '320', '30', '10', '34', '-99', 'ResTeamNameColon', NULL, '@team_set', '312', '313', '319', NULL UNION ALL
SELECT '321', '30', '11', '36', '-2', 'ResAgentsColon', NULL, '@agent_code', '312', '313', '319', '320' UNION ALL
SELECT '322', '30', '12', '24', '-99', 'ResActivityColon', NULL, '@activity_set', NULL, NULL, NULL, NULL UNION ALL
SELECT '323', '30', '13', '22', '-1', 'ResTimeZoneColon', NULL, '@time_zone_id', NULL, NULL, NULL, NULL

INSERT INTO mart.report_control_collection
SELECT '324', '31', '1', '14', '0', 'ResScenarioColon', NULL, '@scenario_id', NULL, NULL, NULL, NULL UNION ALL
SELECT '325', '31', '2', '1', '12:00', 'ResDateFromColon', NULL, '@date_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '326', '31', '3', '2', '12:00', 'ResDateToColon', NULL, '@date_to', '325', NULL, NULL, NULL UNION ALL
SELECT '327', '31', '4', '29', '-2', 'ResGroupPageColon', NULL, '@group_page_code', NULL, NULL, NULL, NULL UNION ALL
SELECT '328', '31', '5', '35', '-99', 'ResGroupPageGroupColon', NULL, '@group_page_group_set', '327', NULL, NULL, NULL UNION ALL
SELECT '329', '31', '6', '37', '-2', 'ResAgentColon', NULL, '@group_page_agent_code', '325', '326', '327', '328' UNION ALL
SELECT '330', '31', '7', '3', '-2', 'ResSiteNameColon', NULL, '@site_id', '325', '326', '327', NULL UNION ALL
SELECT '331', '31', '8', '34', '-99', 'ResTeamNameColon', NULL, '@team_set', '325', '326', '330', NULL UNION ALL
SELECT '332', '31', '9', '36', '-2', 'ResAgentsColon', NULL, '@agent_code', '325', '326', '330', '331' UNION ALL
SELECT '333', '31', '10', '26', '-99', 'ResShiftCategoryColon', NULL, '@shift_category_set', NULL, NULL, NULL, NULL UNION ALL
SELECT '334', '31', '11', '22', '-1', 'ResTimeZoneColon', NULL, '@time_zone_id', NULL, NULL, NULL, NULL

INSERT INTO mart.report_control_collection
SELECT '335', '32', '1', '14', '0', 'ResScenarioColon', NULL, '@scenario_id', NULL, NULL, NULL, NULL UNION ALL
SELECT '336', '32', '2', '6', '12:00', 'ResDateColon', NULL, '@date_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '337', '32', '3', '12', '0', 'ResIntervalFromColon', '1', '@interval_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '338', '32', '4', '13', '-99', 'ResIntervalToColon', '2', '@interval_to', NULL, NULL, NULL, NULL UNION ALL
SELECT '339', '32', '5', '29', '-2', 'ResGroupPageColon', NULL, '@group_page_code', NULL, NULL, NULL, NULL UNION ALL
SELECT '340', '32', '6', '35', '-99', 'ResGroupPageGroupColon', NULL, '@group_page_group_set', '339', NULL, NULL, NULL UNION ALL
SELECT '341', '32', '8', '3', '-2', 'ResSiteNameColon', NULL, '@site_id', '336', '339', NULL, NULL UNION ALL
SELECT '342', '32', '9', '34', '-99', 'ResTeamNameColon', NULL, '@team_set', '336', '341', NULL, NULL UNION ALL
SELECT '343', '32', '11', '24', '-99', 'ResActivityColon', NULL, '@activity_set', NULL, NULL, NULL, NULL UNION ALL
SELECT '344', '32', '12', '22', '-1', 'ResTimeZoneColon', NULL, '@time_zone_id', NULL, NULL, NULL, NULL

UPDATE mart.report
SET control_collection_id = 28
WHERE report_id in (1, 2)

UPDATE mart.report
SET control_collection_id = 29
WHERE report_id = 17

UPDATE mart.report
SET control_collection_id = 30
WHERE report_id = 18

UPDATE mart.report
SET control_collection_id = 31
WHERE report_id = 19

UPDATE mart.report
SET control_collection_id = 32
WHERE report_id = 21

--Remove unused control collections
DELETE FROM mart.report_control_collection where collection_id IN (14, 15, 19, 20)
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (345,'7.1.345') 
