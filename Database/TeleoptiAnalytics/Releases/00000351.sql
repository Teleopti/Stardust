----------------  
--Name: jonas + David
--Date: 2012-01-20
--Desc: Adding new control needed on two branches
----------------
insert into mart.report_control
select 38,'twolistAgentMultiTeam','mart.report_control_twolist_agent_get_multiple_teams',NULL

insert into mart.report_control
select 39,'twolistAgentMultiGroup','mart.report_control_twolist_group_page_agent_get_multiple_groups',NULL

----------------  
--Name: JN
--Date: 2012-01-27
--Desc: Multiple team/group selection on 8 reports.
----------------

INSERT INTO mart.report_control_collection
SELECT '345', '34', '1', '14', '0', 'ResScenarioColon', NULL, '@scenario_id', NULL, NULL, NULL, NULL UNION ALL
SELECT '346', '34', '2', '15', '-99', 'ResSkillColon', NULL, '@skill_set', NULL, NULL, NULL, NULL UNION ALL
SELECT '347', '34', '3', '10', '-99', 'ResWorkloadColon', NULL, '@workload_set', '346', NULL, NULL, NULL UNION ALL
SELECT '348', '34', '4', '11', '4', 'ResIntervalType', NULL, '@interval_type', NULL, NULL, NULL, NULL UNION ALL
SELECT '349', '34', '5', '1', '12:00', 'ResDateFromColon', NULL, '@date_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '350', '34', '6', '2', '12:00', 'ResDateToColon', NULL, '@date_to', '349', NULL, NULL, NULL UNION ALL
SELECT '351', '34', '7', '12', '0', 'ResIntervalFromColon', '1', '@interval_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '352', '34', '8', '13', '-99', 'ResIntervalToColon', '2', '@interval_to', NULL, NULL, NULL, NULL UNION ALL
SELECT '353', '34', '9', '29', '-2', 'ResGroupPageColon', NULL, '@group_page_code', NULL, NULL, NULL, NULL UNION ALL
SELECT '354', '34', '10', '35', '-99', 'ResGroupPageGroupColon', NULL, '@group_page_group_set', '353', NULL, NULL, NULL UNION ALL
SELECT '355', '34', '12', '3', '-2', 'ResSiteNameColon', NULL, '@site_id', '349', '350', '353', NULL UNION ALL
SELECT '356', '34', '13', '34', '-99', 'ResTeamNameColon', NULL, '@team_set', '349', '350', '353', '355' UNION ALL
SELECT '357', '34', '14', '19', '1', 'ResAdherenceCalculationColon', NULL, '@adherence_id', NULL, NULL, NULL, NULL UNION ALL
SELECT '358', '34', '15', '20', '1', 'ResServiceLevelCalcColon', NULL, '@sl_calc_id', NULL, NULL, NULL, NULL UNION ALL
SELECT '359', '34', '16', '22', '-1', 'ResTimeZoneColon', NULL, '@time_zone_id', NULL, NULL, NULL, NULL

INSERT INTO mart.report_control_collection
SELECT '360', '35', '1', '1', '12:00', 'ResDateFromColon', NULL, '@date_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '361', '35', '2', '2', '12:00', 'ResDateToColon', NULL, '@date_to', '360', NULL, NULL, NULL UNION ALL
SELECT '362', '35', '3', '12', '0', 'ResIntervalFromColon', '1', '@interval_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '363', '35', '4', '13', '-99', 'ResIntervalToColon', '2', '@interval_to', NULL, NULL, NULL, NULL UNION ALL
SELECT '364', '35', '5', '29', '-2', 'ResGroupPageColon', NULL, '@group_page_code', NULL, NULL, NULL, NULL UNION ALL
SELECT '365', '35', '6', '35', '-99', 'ResGroupPageGroupColon', NULL, '@group_page_group_set', '364', NULL, NULL, NULL UNION ALL
SELECT '366', '35', '8', '3', '-2', 'ResSiteNameColon', NULL, '@site_id', '360', '361', '364', NULL UNION ALL
SELECT '367', '35', '9', '34', '-99', 'ResTeamNameColon', NULL, '@team_set', '360', '361', '364', '366' UNION ALL
SELECT '368', '35', '10', '19', '1', 'ResAdherenceCalculationColon', NULL, '@adherence_id', NULL, NULL, NULL, NULL UNION ALL
SELECT '369', '35', '11', '22', '-1', 'ResTimeZoneColon', NULL, '@time_zone_id', NULL, NULL, NULL, NULL

INSERT INTO mart.report_control_collection
SELECT '381', '36', '1', '1', '12:00', 'ResDateFromColon', NULL, '@date_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '382', '36', '2', '2', '12:00', 'ResDateToColon', NULL, '@date_to', '381', NULL, NULL, NULL UNION ALL
SELECT '383', '36', '3', '12', '0', 'ResIntervalFromColon', '1', '@interval_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '384', '36', '4', '13', '-99', 'ResIntervalToColon', '2', '@interval_to', NULL, NULL, NULL, NULL UNION ALL
SELECT '385', '36', '5', '29', '-2', 'ResGroupPageColon', NULL, '@group_page_code', NULL, NULL, NULL, NULL UNION ALL
SELECT '386', '36', '6', '35', '-99', 'ResGroupPageGroupColon', NULL, '@group_page_group_set', '385', NULL, NULL, NULL UNION ALL
SELECT '387', '36', '7', '39', '-99', 'ResAgentColon', NULL, '@group_page_agent_set', '381', '382', '385', '386' UNION ALL
SELECT '388', '36', '8', '3', '-2', 'ResSiteNameColon', NULL, '@site_id', '381', '382', '385', NULL UNION ALL
SELECT '389', '36', '9', '34', '-99', 'ResTeamNameColon', NULL, '@team_set', '381', '382', '385', '388' UNION ALL
SELECT '390', '36', '12', '38', '-99', 'ResAgentsColon', NULL, '@agent_set', '381', '382', '388', '389' UNION ALL
SELECT '391', '36', '13', '19', '1', 'ResAdherenceCalculationColon', NULL, '@adherence_id', NULL, NULL, NULL, NULL UNION ALL
SELECT '392', '36', '14', '22', '-1', 'ResTimeZoneColon', NULL, '@time_zone_id', NULL, NULL, NULL, NULL

INSERT INTO  mart.report_control_collection
SELECT '393', '37', '1', '6', '12:00', 'ResDateColon', NULL, '@date_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '394', '37', '2', '29', '-2', 'ResGroupPageColon', NULL, '@group_page_code', NULL, NULL, NULL, NULL UNION ALL
SELECT '395', '37', '3', '35', '-99', 'ResGroupPageGroupColon', NULL, '@group_page_group_set', '394', NULL, NULL, NULL UNION ALL
SELECT '396', '37', '4', '37', '-2', 'ResAgentColon', NULL, '@group_page_agent_code', '393', '394', '395', NULL UNION ALL
SELECT '397', '37', '5', '3', '-2', 'ResSiteNameColon', NULL, '@site_id', '393', '394', NULL, NULL UNION ALL
SELECT '398', '37', '6', '34', '-99', 'ResTeamNameColon', NULL, '@team_set', '393', '394', '397', NULL UNION ALL
SELECT '399', '37', '7', '36', '-2', 'ResAgentsColon', NULL, '@agent_person_code', '393', '397', '398', NULL UNION ALL
SELECT '400', '37', '8', '19', '1', 'ResAdherenceCalculationColon', NULL, '@adherence_id', NULL, NULL, NULL, NULL UNION ALL
SELECT '401', '37', '9', '21', '1', 'ResSortByColon', NULL, '@sort_by', NULL, NULL, NULL, NULL UNION ALL
SELECT '402', '37', '10', '22', '-1', 'ResTimeZoneColon', NULL, '@time_zone_id', NULL, NULL, NULL, NULL

INSERT INTO mart.report_control_collection
SELECT '403', '38', '1', '1', '12:00', 'ResDateFromColon', NULL, '@date_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '404', '38', '2', '2', '12:00', 'ResDateToColon', NULL, '@date_to', '403', NULL, NULL, NULL UNION ALL
SELECT '405', '38', '3', '12', '0', 'ResIntervalFromColon', '1', '@interval_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '406', '38', '4', '13', '-99', 'ResIntervalToColon', '2', '@interval_to', NULL, NULL, NULL, NULL UNION ALL
SELECT '407', '38', '5', '29', '-2', 'ResGroupPageColon', NULL, '@group_page_code', NULL, NULL, NULL, NULL UNION ALL
SELECT '408', '38', '6', '35', '-99', 'ResGroupPageGroupColon', NULL, '@group_page_group_set', '407', NULL, NULL, NULL UNION ALL
SELECT '409', '38', '7', '37', '-2', 'ResAgentColon', NULL, '@group_page_agent_code', '403', '404', '407', '408' UNION ALL
SELECT '410', '38', '8', '3', '-2', 'ResSiteNameColon', NULL, '@site_id', '403', '404', '407', NULL UNION ALL
SELECT '411', '38', '9', '34', '-99', 'ResTeamNameColon', NULL, '@team_set', '403', '404', '407', '410' UNION ALL
SELECT '412', '38', '10', '36', '-2', 'ResAgentsColon', NULL, '@agent_code', '403', '404', '410', '411' UNION ALL
SELECT '413', '38', '11', '22', '-1', 'ResTimeZoneColon', NULL, '@time_zone_id', NULL, NULL, NULL, NULL

INSERT INTO mart.report_control_collection
SELECT '414', '39', '1', '14', '0', 'ResScenarioColon', NULL, '@scenario_id', NULL, NULL, NULL, NULL UNION ALL
SELECT '415', '39', '2', '1', '12:00', 'ResDateFromColon', NULL, '@date_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '416', '39', '3', '2', '12:00', 'ResDateToColon', NULL, '@date_to', '415', NULL, NULL, NULL UNION ALL
SELECT '417', '39', '4', '12', '0', 'ResIntervalFromColon', '1', '@interval_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '418', '39', '5', '13', '-99', 'ResIntervalToColon', '2', '@interval_to', NULL, NULL, NULL, NULL UNION ALL
SELECT '419', '39', '6', '29', '-2', 'ResGroupPageColon', NULL, '@group_page_code', NULL, NULL, NULL, NULL UNION ALL
SELECT '420', '39', '7', '35', '-99', 'ResGroupPageGroupColon', NULL, '@group_page_group_set', '419', NULL, NULL, NULL UNION ALL
SELECT '421', '39', '8', '37', '-2', 'ResAgentColon', NULL, '@group_page_agent_code', '415', '416', '419', '420' UNION ALL
SELECT '422', '39', '9', '3', '-2', 'ResSiteNameColon', NULL, '@site_id', '415', '416', '419', NULL UNION ALL
SELECT '423', '39', '10', '34', '-99', 'ResTeamNameColon', NULL, '@team_set', '415', '416', '419', '422' UNION ALL
SELECT '424', '39', '11', '36', '-2', 'ResAgentsColon', NULL, '@agent_code', '415', '416', '422', '423' UNION ALL
SELECT '425', '39', '12', '33', '-1', 'ResOvertimeColon', NULL, '@overtime_set', NULL, NULL, NULL, NULL UNION ALL
SELECT '426', '39', '13', '22', '-1', 'ResTimeZoneColon', NULL, '@time_zone_id', NULL, NULL, NULL, NULL

INSERT INTO mart.report_control_collection
SELECT '427', '40', '1', '1', '12:00', 'ResDateFromColon', NULL, '@date_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '428', '40', '2', '2', '12:00', 'ResDateToColon', NULL, '@date_to', '427', NULL, NULL, NULL UNION ALL
SELECT '429', '40', '3', '12', '0', 'ResIntervalFromColon', '1', '@interval_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '430', '40', '4', '13', '-99', 'ResIntervalToColon', '2', '@interval_to', NULL, NULL, NULL, NULL UNION ALL
SELECT '431', '40', '5', '29', '-2', 'ResGroupPageColon', NULL, '@group_page_code', NULL, NULL, NULL, NULL UNION ALL
SELECT '432', '40', '6', '35', '-99', 'ResGroupPageGroupColon', NULL, '@group_page_group_set', '431', NULL, NULL, NULL UNION ALL
SELECT '433', '40', '7', '39', '-99', 'ResAgentColon', NULL, '@group_page_agent_set', '427', '428', '431', '432' UNION ALL
SELECT '434', '40', '8', '3', '-2', 'ResSiteNameColon', NULL, '@site_id', '427', '428', '431', NULL UNION ALL
SELECT '435', '40', '9', '34', '-99', 'ResTeamNameColon', NULL, '@team_set', '427', '428', '431', '434' UNION ALL
SELECT '436', '40', '10', '38', '-99', 'ResAgentsColon', NULL, '@agent_set', '427', '428', '434', '435' UNION ALL
SELECT '437', '40', '11', '22', '-1', 'ResTimeZoneColon', NULL, '@time_zone_id', NULL, NULL, NULL, NULL

INSERT INTO mart.report_control_collection
SELECT '438', '41', '1', '14', '0', 'ResScenarioColon', NULL, '@scenario_id', NULL, NULL, NULL, NULL UNION ALL
SELECT '439', '41', '2', '1', '12:00', 'ResDateFromColon', NULL, '@date_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '440', '41', '3', '2', '12:00', 'ResDateToColon', NULL, '@date_to', '439', NULL, NULL, NULL UNION ALL
SELECT '441', '41', '4', '12', '0', 'ResIntervalFromColon', '1', '@interval_from', NULL, NULL, NULL, NULL UNION ALL
SELECT '442', '41', '5', '13', '-99', 'ResIntervalToColon', '2', '@interval_to', NULL, NULL, NULL, NULL UNION ALL
SELECT '443', '41', '6', '29', '-2', 'ResGroupPageColon', NULL, '@group_page_code', NULL, NULL, NULL, NULL UNION ALL
SELECT '444', '41', '7', '35', '-99', 'ResGroupPageGroupColon', NULL, '@group_page_group_set', '443', NULL, NULL, NULL UNION ALL
SELECT '445', '41', '8', '37', '-2', 'ResAgentColon', NULL, '@group_page_agent_code', '439', '440', '443', '444' UNION ALL
SELECT '446', '41', '9', '3', '-2', 'ResSiteNameColon', NULL, '@site_id', '439', '440', '443', NULL UNION ALL
SELECT '447', '41', '10', '34', '-99', 'ResTeamNameColon', NULL, '@team_set', '439', '440', '443', '446' UNION ALL
SELECT '448', '41', '11', '36', '-2', 'ResAgentsColon', NULL, '@agent_code', '439', '440', '446', '447' UNION ALL
SELECT '449', '41', '12', '24', '-99', 'ResActivityColon', NULL, '@activity_set', NULL, NULL, NULL, NULL UNION ALL
SELECT '450', '41', '13', '22', '-1', 'ResTimeZoneColon', NULL, '@time_zone_id', NULL, NULL, NULL, NULL

UPDATE mart.report
SET control_collection_id = 34
WHERE report_id = 3

UPDATE mart.report
SET control_collection_id = 35
WHERE report_id = 11

UPDATE mart.report
SET control_collection_id = 36
WHERE report_id = 12

UPDATE mart.report
SET control_collection_id = 37
WHERE report_id = 13

UPDATE mart.report
SET control_collection_id = 38
WHERE report_id = 16

UPDATE mart.report
SET control_collection_id = 39
WHERE report_id = 23

UPDATE mart.report
SET control_collection_id = 40
WHERE report_id = 24

UPDATE mart.report
SET control_collection_id = 41
WHERE report_id = 25

--Remove unused control collections
DELETE FROM mart.report_control_collection where collection_id IN (21, 18, 8, 10, 13, 23, 24, 17)
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (351,'7.1.351') 
