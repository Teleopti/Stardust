
/****** Object:  View [mart].[v_report_control]    Script Date: 02/16/2012 10:51:45 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_report_control]'))
DROP VIEW [mart].[v_report_control]
GO

CREATE View [mart].[v_report_control]
AS
SELECT		[control_name],
			[fill_proc_name],
			[Id]
FROM         mart.report_control
UNION 
SELECT		[control_name],
			[fill_proc_name],
			[Id]
FROM         mart.custom_report_control
GO


