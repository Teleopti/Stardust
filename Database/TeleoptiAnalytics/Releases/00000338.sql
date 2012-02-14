--AndersF 20111021 cope with unicode agent names

alter table dbo.agent_info alter column agent_name nvarchar(50) not null

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[agent_info]') AND name = N'uix_orig_agent_id')
DROP INDEX [uix_orig_agent_id] ON [dbo].[agent_info]
GO
alter table dbo.agent_info alter column orig_agent_id nvarchar(50) not null
GO
CREATE UNIQUE NONCLUSTERED INDEX [uix_orig_agent_id] ON [dbo].[agent_info] 
(
	[log_object_id] ASC,
	[orig_agent_id] ASC
)
GO

alter table dbo.queues alter column orig_desc nvarchar(50) null
GO
alter table dbo.queues alter column display_desc nvarchar(50) null
GO
----------------  
--Name: Jonas n
--Date: 2011-10-24  
--Desc: Change parameters for five reports so that you can choose multiple team or groups
----------------  
INSERT INTO mart.report_control
	SELECT 34, 'twolistTeam', 'mart.report_control_twolist_team_get', NULL
INSERT INTO mart.report_control
	SELECT 35, 'twolistGroupPageGroup', 'mart.report_control_twolist_group_page_group_get', NULL
INSERT INTO mart.report_control
	SELECT 36, 'cboAgentMultiTeam', 'mart.report_control_agent_get_multiple_teams', NULL
INSERT INTO mart.report_control
	SELECT 37, 'cboGroupPageAgentMultiGroup', 'mart.report_control_group_page_agent_get_multiple_groups', NULL
	

INSERT INTO mart.report_control_collection
SELECT '250', '25', '1', '14', '0', 'ResScenarioColon', NULL, '@scenario_id', NULL, NULL, NULL, NULL UNION ALL
SELECT '251', '25', '2', '1', '12:00', 'ResDateFromColon', NULL, '@date_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '252', '25', '3', '2', '12:00', 'ResDateToColon', NULL, '@date_to', '251', NULL, NULL, NULL UNION ALL
SELECT '253', '25', '4', '29', '-2', 'ResGroupPageColon', NULL, '@group_page_code', NULL, NULL, NULL, NULL UNION ALL
SELECT '254', '25', '5', '35', '-99', 'ResGroupPageGroupColon', NULL, '@group_page_group_set', '253', NULL, NULL, NULL UNION ALL
SELECT '255', '25', '6', '37', '-2', 'ResAgentColon', NULL, '@group_page_agent_code', '251', '252', '253', '254' UNION ALL
SELECT '256', '25', '7', '3', '-2', 'ResSiteNameColon', NULL, '@site_id', '251', '252', '253', NULL UNION ALL
SELECT '257', '25', '8', '34', '-99', 'ResTeamNameColon', NULL, '@team_set', '251', '252', '256', NULL UNION ALL
SELECT '258', '25', '9', '36', '-2', 'ResAgentsColon', NULL, '@agent_code', '251', '252', '256', '257' UNION ALL
SELECT '259', '25', '10', '25', '-1', 'ResAbsenceColon', NULL, '@absence_set', NULL, NULL, NULL, NULL UNION ALL
SELECT '260', '25', '11', '22', '-1', 'ResTimeZoneColon', NULL, '@time_zone_id', NULL, NULL, NULL, NULL

INSERT INTO mart.report_control_collection
SELECT '261', '26', '1', '14', '0', 'ResScenarioColon', NULL, '@scenario_id', NULL, NULL, NULL, NULL UNION ALL
SELECT '262', '26', '2', '1', '12:00', 'ResDateFromColon', NULL, '@date_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '263', '26', '3', '2', '12:00', 'ResDateToColon', NULL, '@date_to', '262', NULL, NULL, NULL UNION ALL
SELECT '264', '26', '4', '29', '-2', 'ResGroupPageColon', NULL, '@group_page_code', NULL, NULL, NULL, NULL UNION ALL
SELECT '265', '26', '5', '35', '-99', 'ResGroupPageGroupColon', NULL, '@group_page_group_set', '264', NULL, NULL, NULL UNION ALL
SELECT '266', '26', '6', '37', '-2', 'ResAgentColon', NULL, '@group_page_agent_code', '262', '263', '264', '265' UNION ALL
SELECT '267', '26', '7', '3', '-2', 'ResSiteNameColon', NULL, '@site_id', '262', '263', '264', NULL UNION ALL
SELECT '268', '26', '8', '34', '-99', 'ResTeamNameColon', NULL, '@team_set', '262', '263', '267', NULL UNION ALL
SELECT '269', '26', '9', '36', '-2', 'ResAgentsColon', NULL, '@agent_code', '262', '263', '267', '268' UNION ALL
SELECT '270', '26', '10', '28', '-99', 'ResShiftCategoryColon', NULL, '@shift_category_set', NULL, NULL, NULL, NULL UNION ALL
SELECT '271', '26', '11', '27', '-1', 'ResDayOffColon', NULL, '@day_off_set', NULL, NULL, NULL, NULL UNION ALL
SELECT '272', '26', '12', '25', '-1', 'ResAbsenceColon', NULL, '@absence_set', NULL, NULL, NULL, NULL UNION ALL
SELECT '273', '26', '13', '22', '-1', 'ResTimeZoneColon', NULL, '@time_zone_id', NULL, NULL, NULL, NULL

INSERT INTO mart.report_control_collection
SELECT '274', '27', '1', '15', '-99', 'ResSkillColon', NULL, '@skill_set', NULL, NULL, NULL, NULL UNION ALL
SELECT '275', '27', '2', '10', '-99', 'ResWorkloadColon', NULL, '@workload_set', '274', NULL, NULL, NULL UNION ALL
SELECT '276', '27', '3', '1', '12:00', 'ResDateFromColon', NULL, '@date_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '277', '27', '4', '2', '12:00', 'ResDateToColon', NULL, '@date_to', '276', NULL, NULL, NULL UNION ALL
SELECT '278', '27', '5', '12', '0', 'ResIntervalFromColon', '1', '@interval_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '279', '27', '6', '13', '-99', 'ResIntervalToColon', '2', '@interval_to', NULL, NULL, NULL, NULL UNION ALL
SELECT '280', '27', '7', '29', '-2', 'ResGroupPageColon', NULL, '@group_page_code', NULL, NULL, NULL, NULL UNION ALL
SELECT '281', '27', '8', '35', '-99', 'ResGroupPageGroupColon', NULL, '@group_page_group_set', '280', NULL, NULL, NULL UNION ALL
SELECT '282', '27', '9', '37', '-2', 'ResAgentColon', NULL, '@group_page_agent_code', '276', '277', '280', '281' UNION ALL
SELECT '283', '27', '10', '3', '-2', 'ResSiteNameColon', NULL, '@site_id', '276', '277', '280', NULL UNION ALL
SELECT '284', '27', '11', '34', '-99', 'ResTeamNameColon', NULL, '@team_set', '276', '277', '283', NULL UNION ALL
SELECT '285', '27', '12', '36', '-2', 'ResAgentsColon', NULL, '@agent_code', '276', '277', '283', '284' UNION ALL
SELECT '286', '27', '13', '22', '-1', 'ResTimeZoneColon', NULL, '@time_zone_id', NULL, NULL, NULL, NULL

UPDATE mart.report
SET control_collection_id = 25
WHERE report_id in (4, 22)

UPDATE mart.report
SET control_collection_id = 26
WHERE report_id in (20, 26)

UPDATE mart.report
SET control_collection_id = 27
WHERE report_id = 15

--Remove unused control collections
DELETE FROM mart.report_control_collection where collection_id IN (2, 3, 7, 12, 16, 22)

----------------  
--Name: DavidJ
--Date: 2011-10-27
--Desc: #16704 - Be able to select specific activities and absences in Scheduled Time Per Agent report
----------------
--use if exist since we have already shipped this to customer (ahead of this release)
if not exists (select * from mart.report_control_collection where control_collection_id=109)
insert into mart.report_control_collection
select '109','14','12','24','-99','ResActivityColon',NULL,'@activity_set',NULL,NULL,NULL,NULL

if not exists (select * from mart.report_control_collection where control_collection_id=110)
insert into mart.report_control_collection
select '110','14','13','25','-1','ResAbsenceColon',NULL,'@absence_set',NULL,NULL,NULL,NULL
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (338,'7.1.338') 
