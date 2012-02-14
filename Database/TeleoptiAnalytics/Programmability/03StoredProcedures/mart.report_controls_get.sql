IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_controls_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_controls_get]
GO


-- exec [report_controls_get] @report_id=1

CREATE PROCEDURE [mart].[report_controls_get]
/*
Skapad: 	2008-03-19
Av: 		Ola
Syfte:		Ladda alla kontroller som ska visas när man ska göra urval för en rapport
Används:	Av .NET komponten som laddar valsidorna
uppdaterad: 20090211 Nytt mart schema KJ
			20100902 La till @group_page_index /HG
*/
@report_id int,
@group_page_code uniqueidentifier
AS
/*
DBID = (int) row.ItemArray[0];
Name = (string) row.ItemArray[1];
Text = (string) row.ItemArray[2];
DefaultValue = (string) row.ItemArray[3];
ProcName = (string) row.ItemArray[4];

ProcParam = (int) row.ItemArray[8];
ParamName = (string) row.ItemArray[9];

*/
--select @group_page_code
--return

DECLARE @display BIT
DECLARE @dont_display BIT

SET @display = 1
SET @dont_display = 0

IF EXISTS (SELECT distinct group_page_code FROM mart.dim_group_page WHERE group_page_code = @group_page_code)
	BEGIN
		-- Get set of controls for other group page than Main (Business Hierarchy)
		SELECT Controls.* FROM
		(
			SELECT 
				cc.control_collection_id, 
				c.control_name, 
				cc.control_name_resource_key, 
				default_value, 
				fill_proc_name,
				ISNULL(depend_of1,0) AS depend_of1, 
				ISNULL(depend_of2,0) AS depend_of2, 
				ISNULL(depend_of3,0) as depend_of3, 
				ISNULL(depend_of4,0) as depend_of4,
				ISNULL(fill_proc_param, -1000) as fill_proc_param,
				param_name,
				print_order,
				@display AS display
			FROM mart.report r
				INNER JOIN mart.report_control_collection cc ON cc.collection_id = r.control_collection_id
				INNER JOIN mart.report_control c ON cc.control_id = c.control_id
			WHERE report_id = @report_id
				AND cc.control_id NOT IN(3, 4, 5, 18, 34, 36, 38)
			--ORDER BY print_order
			UNION
			SELECT 
				cc.control_collection_id, 
				c.control_name, 
				cc.control_name_resource_key, 
				default_value, 
				fill_proc_name,
				ISNULL(depend_of1,0) AS depend_of1, 
				ISNULL(depend_of2,0) AS depend_of2, 
				ISNULL(depend_of3,0) as depend_of3, 
				ISNULL(depend_of4,0) as depend_of4,
				ISNULL(fill_proc_param, -1000) as fill_proc_param,
				param_name,
				print_order,
				@dont_display AS display
			FROM mart.report r
				INNER JOIN mart.report_control_collection cc ON cc.collection_id = r.control_collection_id
				INNER JOIN mart.report_control c ON cc.control_id = c.control_id
			WHERE report_id = @report_id
				AND cc.control_id IN(3, 4, 5, 18, 34, 36, 38)
		) AS Controls
		ORDER BY Controls.print_order
		
	END
ELSE
	BEGIN
		-- Get set of controls for group page Main (Business Hierarchy)
		SELECT Controls.* FROM
		(
			SELECT 
				cc.control_collection_id, 
				c.control_name, 
				cc.control_name_resource_key, 
				default_value, 
				fill_proc_name,
				ISNULL(depend_of1,0) AS depend_of1, 
				ISNULL(depend_of2,0) AS depend_of2, 
				ISNULL(depend_of3,0) as depend_of3, 
				ISNULL(depend_of4,0) as depend_of4,
				ISNULL(fill_proc_param, -1000) as fill_proc_param,
				param_name,
				print_order,
				@display AS Display
			FROM mart.report r
				INNER JOIN mart.report_control_collection cc ON cc.collection_id = r.control_collection_id
				INNER JOIN mart.report_control c ON cc.control_id = c.control_id
			WHERE report_id = @report_id
				AND cc.control_id NOT IN(30, 31, 32, 35, 37, 39)
			--ORDER BY print_order
			UNION
			SELECT 
				cc.control_collection_id, 
				c.control_name, 
				cc.control_name_resource_key, 
				default_value, 
				fill_proc_name,
				ISNULL(depend_of1,0) AS depend_of1, 
				ISNULL(depend_of2,0) AS depend_of2, 
				ISNULL(depend_of3,0) as depend_of3, 
				ISNULL(depend_of4,0) as depend_of4,
				ISNULL(fill_proc_param, -1000) as fill_proc_param,
				param_name,
				print_order,
				@dont_display AS Display
			FROM mart.report r
				INNER JOIN mart.report_control_collection cc ON cc.collection_id = r.control_collection_id
				INNER JOIN mart.report_control c ON cc.control_id = c.control_id
			WHERE report_id = @report_id
				AND cc.control_id IN(30, 31, 32, 35, 37, 39)
			--ORDER BY print_order
		) AS Controls
		ORDER BY Controls.print_order
	END

GO