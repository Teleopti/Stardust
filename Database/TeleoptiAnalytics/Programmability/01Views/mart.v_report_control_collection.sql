/****** Object:  View [mart].[v_report_control_collection]    Script Date: 02/16/2012 10:55:43 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_report_control_collection]'))
DROP VIEW [mart].[v_report_control_collection]
GO


CREATE View [mart].[v_report_control_collection]
AS
SELECT		Id, 
			CollectionId, 
			ControlId,
			print_order, 
			default_value, 
			control_name_resource_key, 
			fill_proc_param, 
			param_name, 
			DependOf1,
			DependOf2,
			DependOf3,
			DependOf4
			
FROM         mart.report_control_collection
UNION
SELECT		Id, 
			CollectionId, 
			ControlId,
			print_order, 
			default_value, 
			control_name_resource_key, 
			fill_proc_param, 
			param_name, 
			DependOf1,
			DependOf2,
			DependOf3,
			DependOf4
FROM         mart.custom_report_control_collection
GO


