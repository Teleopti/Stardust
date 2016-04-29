----------------  
--Name: Karin
--Date: 2016-04-18
--Desc: #xxxxx New report_control and added to collection
-----------------
--ADD NEW CONTROL
IF NOT EXISTS(SELECT 1 FROM mart.report_control where control_id=44)
BEGIN
	INSERT mart.report_control(Id, control_id, control_name, fill_proc_name)
	SELECT NEWID(), 44, 'chkDetails','1'
END

--ADD REPORT CONTROL TO COLLECTION
IF NOT EXISTS(SELECT 1 FROM mart.report_control_collection where Id='73388fff-f2cc-4191-855e-98fb0a61d80a')
BEGIN
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '73388fff-f2cc-4191-855e-98fb0a61d80a',	mart.report_control.Id,	'9a1da111-9283-4da5-bf55-083b0bb7617a',495,25,11,44,'True','ResIncludeDetails',NULL,'@details',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=44
END
--ADD REPORT CONTROL TO COLLECTION
IF NOT EXISTS(SELECT 1 FROM mart.report_control_collection where Id='796BA532-BAF5-4709-A2B2-6D66DFA7E7FF')
BEGIN
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '796BA532-BAF5-4709-A2B2-6D66DFA7E7FF',	mart.report_control.Id,	'1249F68D-F35C-4785-9579-0B9831EB1C9B',496,41,14,44,'True','ResIncludeDetails',NULL,'@details',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=44
END
IF NOT EXISTS(SELECT 1 FROM mart.report_control_collection where Id='5962741E-6213-451C-92F3-D3DF2E5AD9A8')
BEGIN
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '5962741E-6213-451C-92F3-D3DF2E5AD9A8',	mart.report_control.Id,	'75FE4BC7-957B-4904-90B3-3334255C40B3',497,40,12,44,'True','ResIncludeDetails',NULL,'@details',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=44
END
IF NOT EXISTS(SELECT 1 FROM mart.report_control_collection where Id='0F6BA834-B019-4775-8C57-94AC5C39C84B')
BEGIN
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '0F6BA834-B019-4775-8C57-94AC5C39C84B',	mart.report_control.Id,	'01338146-2A87-4A7C-AA5B-3F083F7C2C37',498,36,15,44,'True','ResIncludeDetails',NULL,'@details',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=44
END
IF NOT EXISTS(SELECT 1 FROM mart.report_control_collection where Id='3400DFA9-A43E-4F62-93AE-0BDB9A15CB4C')
BEGIN
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '3400DFA9-A43E-4F62-93AE-0BDB9A15CB4C',	mart.report_control.Id,	'50DB32C4-9BBD-4154-877F-1136EAC1DB51',499,33,12,44,'True','ResIncludeDetails',NULL,'@details',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=44
END
IF NOT EXISTS(SELECT 1 FROM mart.report_control_collection where Id='215F8260-26E3-4ABC-869A-0D185C590C41')
BEGIN
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '215F8260-26E3-4ABC-869A-0D185C590C41',	mart.report_control.Id,	'F24B8766-514D-4EB9-AD16-3738AAFA7986',500,28,10,44,'True','ResIncludeDetails',NULL,'@details',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=44
END
IF NOT EXISTS(SELECT 1 FROM mart.report_control_collection where Id='F55FC8E8-3277-4019-9E95-A7B3CD979ACC')
BEGIN
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'F55FC8E8-3277-4019-9E95-A7B3CD979ACC',	mart.report_control.Id,	'89D22115-1419-4C90-BE36-01D79D0BD63C',501,39,14,44,'True','ResIncludeDetails',NULL,'@details',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=44
END
IF NOT EXISTS(SELECT 1 FROM mart.report_control_collection where Id='53B2673C-9057-4119-9EF8-4FD2CF46E6D7')
BEGIN
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '53B2673C-9057-4119-9EF8-4FD2CF46E6D7',	mart.report_control.Id,	'0DA34028-2E35-489C-BA36-3BA410FCDE82',502,29,15,29,'True','ResIncludeDetails',NULL,'@details',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=44
END
IF NOT EXISTS(SELECT 1 FROM mart.report_control_collection where Id='192E6E3D-59F3-4DA8-8BB5-DB16CC0C64F0')
BEGIN
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '192E6E3D-59F3-4DA8-8BB5-DB16CC0C64F0',	mart.report_control.Id,	'8F3F26E9-25AA-4913-8A2A-0435A897DB6D',503,35,12,29,'True','ResIncludeDetails',NULL,'@details',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=44
END
