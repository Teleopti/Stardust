/****** Object:  View [mart].[v_report_control_collection]    Script Date: 02/16/2012 10:55:43 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_report_control_collection]'))
DROP VIEW [mart].[v_report_control_collection]
GO


CREATE View [mart].[v_report_control_collection]
AS
SELECT		control_collection_id,
			print_order, 
			default_value, 
			control_name_resource_key, 
			fill_proc_param, 
			param_name, 
			depend_of1, 
			depend_of2, 
			depend_of3, 
			depend_of4, 
			Id, 
			CollectionId, 
			ControlId
FROM         mart.report_control_collection
UNION
SELECT		control_collection_id,
			print_order, 
			default_value, 
			control_name_resource_key, 
			fill_proc_param, 
			param_name, 
			depend_of1, 
			depend_of2, 
			depend_of3, 
			depend_of4, 
			Id, 
			CollectionId, 
			ControlId
FROM         mart.custom_report_control_collection
GO


