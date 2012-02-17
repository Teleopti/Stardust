/****** Object:  View [mart].[v_report]    Script Date: 02/16/2012 11:02:20 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_report]'))
DROP VIEW [mart].[v_report]
GO

CREATE View [mart].[v_report]
AS
SELECT		url, 
			target, 
			report_name, 
			report_name_resource_key, 
			visible, 
			rpt_file_name, 
			proc_name, 
			help_key, 
			sub1_name, 
			sub1_proc_name, 
			sub2_name, 
			sub2_proc_name, 
			Id, 
			ControlCollectionId
FROM         mart.report
UNION
SELECT		url, 
			target, 
			report_name, 
			report_name_resource_key, 
			visible, 
			rpt_file_name, 
			proc_name, 
			help_key, 
			sub1_name, 
			sub1_proc_name, 
			sub2_name, 
			sub2_proc_name, 
			Id, 
			ControlCollectionId
FROM         mart.custom_report
GO


