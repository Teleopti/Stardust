
----------------
--Name: KJ 
--Date: 2012-09-05
--Desc: New report for Schedule Adherence and collection
----------------

--ADD REPORT CONTROL COLLECTION
IF NOT EXISTS(SELECT 1 FROM mart.report_control_collection where CollectionId='22AF82E4-E0E0-44DE-8AD5-82768F74E6C1')
BEGIN
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '7D02E87F-FC1C-4386-993C-5252526F6201',mart.report_control.Id,'22AF82E4-E0E0-44DE-8AD5-82768F74E6C1',451,42,1,1,'12:00','ResDateFromColon',NULL,'@date_from',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=1
	
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '5B67EF62-5211-49D3-AB35-F9B40CAA67DD',mart.report_control.Id,'22AF82E4-E0E0-44DE-8AD5-82768F74E6C1',452,42,2,2,'12:00','ResDateToColon',NULL,'@date_to',451,NULL,NULL,NULL,'7D02E87F-FC1C-4386-993C-5252526F6201',NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=2

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '3C2F6E27-751C-409E-830C-C229BDABB199',mart.report_control.Id,'22AF82E4-E0E0-44DE-8AD5-82768F74E6C1',453,42,3,29,'-2','ResGroupPageColon',NULL,'@group_page_code',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=29

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '0643EBA1-7A14-4014-8BE5-230A45224E40',mart.report_control.Id,'22AF82E4-E0E0-44DE-8AD5-82768F74E6C1',454,42,4,35,'-99','ResGroupPageGroupColon',NULL,'@group_page_group_set',453,NULL,NULL,NULL,'3C2F6E27-751C-409E-830C-C229BDABB199',NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=35
	
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '2B02F3AE-6DD2-4EE5-8F48-834AEF9A5C00',mart.report_control.Id,'22AF82E4-E0E0-44DE-8AD5-82768F74E6C1',461,42,5,37,'0','ResAgentsColon',NULL,'@group_page_agent_code',451,453,454,NULL,'7D02E87F-FC1C-4386-993C-5252526F6201','3C2F6E27-751C-409E-830C-C229BDABB199','0643EBA1-7A14-4014-8BE5-230A45224E40',NULL
	FROM mart.report_control WHERE control_id=37

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'ED0014EB-CF18-4262-A044-CF05BD693D28',mart.report_control.Id,'22AF82E4-E0E0-44DE-8AD5-82768F74E6C1',455,42,6,3,'-2','ResSiteNameColon',NULL,'@site_id',451,452,453,NULL,'7D02E87F-FC1C-4386-993C-5252526F6201','5B67EF62-5211-49D3-AB35-F9B40CAA67DD','3C2F6E27-751C-409E-830C-C229BDABB199',NULL
	FROM mart.report_control WHERE control_id=3

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '0E8E1C3D-43C5-41A4-9EF7-0357F7AB0D60',mart.report_control.Id,'22AF82E4-E0E0-44DE-8AD5-82768F74E6C1',456,42,7,34,'-99','ResTeamNameColon',NULL,'@team_set',451,452,453,455,'7D02E87F-FC1C-4386-993C-5252526F6201','5B67EF62-5211-49D3-AB35-F9B40CAA67DD','3C2F6E27-751C-409E-830C-C229BDABB199','ED0014EB-CF18-4262-A044-CF05BD693D28'
	FROM mart.report_control WHERE control_id=34

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'F2DACAEB-5596-4839-B7BA-6258F27D83CF',mart.report_control.Id,'22AF82E4-E0E0-44DE-8AD5-82768F74E6C1',457,42,8,36,'-2','ResAgentsColon',NULL,'@agent_person_code',451,452,455,456,'7D02E87F-FC1C-4386-993C-5252526F6201','5B67EF62-5211-49D3-AB35-F9B40CAA67DD','ED0014EB-CF18-4262-A044-CF05BD693D28','0E8E1C3D-43C5-41A4-9EF7-0357F7AB0D60'
	FROM mart.report_control WHERE control_id=36

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '87A5D253-60F4-4843-AA11-B9DEA29D32C4',mart.report_control.Id,'22AF82E4-E0E0-44DE-8AD5-82768F74E6C1',460,42,9,19,1,'ResAdherenceCalculationColon',NULL,'@adherence_id',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=19
	
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'D980BE85-A32D-42C4-87D4-9852EE2ED41F',mart.report_control.Id,'22AF82E4-E0E0-44DE-8AD5-82768F74E6C1',458,42,10,19,6,'ResSortByColon',NULL,'@sort_by',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=21
	
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'F0AC9AAC-D6F0-4D9E-B88B-6AD54610EFF2',mart.report_control.Id,'22AF82E4-E0E0-44DE-8AD5-82768F74E6C1',459,42,11,22,'-95','ResTimeZoneColon',NULL,'@time_zone_id',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=22

END	

--ADD REPORT
IF NOT EXISTS(SELECT 1 FROM mart.Report WHERE Id='6A3EB69B-690E-4605-B80E-46D5710B28AF')
BEGIN
	INSERT INTO mart.Report(Id, report_id, control_collection_id, url, target, report_name, report_name_resource_key, visible, rpt_file_name, proc_name, help_key, sub1_name, sub1_proc_name, sub2_name, sub2_proc_name, ControlCollectionId)
	VALUES ('6A3EB69B-690E-4605-B80E-46D5710B28AF',28,42,'~/Selection.aspx?ReportId=6a3eb69b-690e-4605-b80e-46d5710b28af','_blank','Adherence per Agent','ResReportAdherencePerAgent',1,'~/Reports/CCC/report_agent_schedule_adherence.aspx','mart.report_data_agent_schedule_adherence','f01_Report_AdherencePerAgent.html','','','','','22AF82E4-E0E0-44DE-8AD5-82768F74E6C1')
END	

--Change name of old report
UPDATE mart.Report
SET report_name = 'Adherence per Day',
	report_name_resource_key='ResReportAdherencePerDay',
	help_key = 'f01_Report_AdherencePerDay.html'
WHERE report_id=13
	

	
	
	
	