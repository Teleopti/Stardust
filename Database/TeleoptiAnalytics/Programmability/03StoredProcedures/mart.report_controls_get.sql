IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_controls_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_controls_get]
GO


-- exec mart.report_controls_get @report_id='0E3F340F-C05D-4A98-AD23-A019607745C9', @group_page_code='E12C7EB1-23B6-4730-8019-5013C9758663'

CREATE PROCEDURE [mart].[report_controls_get]
/*
Skapad: 	2008-03-19
Av: 		Ola
Syfte:		Ladda alla kontroller som ska visas när man ska göra urval för en rapport
Används:	Av .NET komponten som laddar valsidorna
uppdaterad: 20090211 Nytt mart schema KJ
			20100902 La till @group_page_index /HG
			-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
*/
@report_id uniqueidentifier,
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

CREATE TABLE #grouppagecontrols (id uniqueidentifier)



IF EXISTS (SELECT distinct group_page_code FROM mart.dim_group_page WHERE group_page_code = @group_page_code)
	BEGIN
		-- Get set of controls for other group page than Main (Business Hierarchy)
		INSERT into #grouppagecontrols  SELECT  'E12C7EB1-23B6-4730-8019-5013C9758663'
		INSERT into #grouppagecontrols SELECT  'AF3DCA13-71A9-4598-96A7-7EFE703F3C9F'
		INSERT into #grouppagecontrols SELECT  '6BDA3854-7915-4689-910E-9CEEE52D014B'
		INSERT into #grouppagecontrols SELECT  '989F6F70-29F5-43FB-9291-AA02B6503C08'
		INSERT into #grouppagecontrols SELECT  '3D4F57F4-EC28-408B-BB96-E90DEABD16AD'
		INSERT into #grouppagecontrols SELECT  'A7EF3BC8-E7B0-4C8F-B333-FB96068A21E9'
		INSERT into #grouppagecontrols SELECT  'EFE140D0-904A-4326-BEC2-D45945F7EC6E'
		SELECT Controls.* FROM
		(
			SELECT 
				cc.Id, 
				c.control_name, 
				cc.control_name_resource_key, 
				default_value, 
				fill_proc_name,
				ISNULL(DependOf1, '00000000-0000-0000-0000-000000000000') AS DependOf1, 
				ISNULL(DependOf2,'00000000-0000-0000-0000-000000000000') AS DependOf2, 
				ISNULL(DependOf3,'00000000-0000-0000-0000-000000000000') as DependOf3, 
				ISNULL(DependOf4,'00000000-0000-0000-0000-000000000000') as DependOf4,
				ISNULL(fill_proc_param, -1000) as fill_proc_param,
				param_name,
				print_order,
				@display AS display
			FROM mart.v_report r
				INNER JOIN mart.v_report_control_collection cc ON cc.CollectionId = r.ControlCollectionId
				INNER JOIN mart.v_report_control c ON cc.ControlId = c.Id
			WHERE r.Id = @report_id
				AND cc.ControlId NOT IN(select * from #grouppagecontrols)
			--ORDER BY print_order
			UNION
			SELECT 
				cc.Id, 
				c.control_name, 
				cc.control_name_resource_key, 
				default_value, 
				fill_proc_name,
				ISNULL(DependOf1, '00000000-0000-0000-0000-000000000000') AS DependOf1, 
				ISNULL(DependOf2,'00000000-0000-0000-0000-000000000000') AS DependOf2, 
				ISNULL(DependOf3,'00000000-0000-0000-0000-000000000000') as DependOf3, 
				ISNULL(DependOf4,'00000000-0000-0000-0000-000000000000') as DependOf4,
				ISNULL(fill_proc_param, -1000) as fill_proc_param,
				param_name,
				print_order,
				@dont_display AS display
			FROM mart.v_report r
				INNER JOIN mart.v_report_control_collection cc ON cc.CollectionId = r.ControlCollectionId
				INNER JOIN mart.v_report_control c ON cc.ControlId = c.Id
			WHERE r.Id = @report_id
				AND cc.ControlId IN(select * from #grouppagecontrols)
		) AS Controls
		ORDER BY Controls.print_order
		
	END
ELSE
	BEGIN
		-- Get set of controls for group page Main (Business Hierarchy)
		INSERT into #grouppagecontrols  SELECT  '8306A37C-2134-4717-BAF6-071112AB27B6'
		INSERT into #grouppagecontrols SELECT  'F257C91F-CD6A-4EE0-918A-3C39BD8AAF04'
		INSERT into #grouppagecontrols SELECT  'D7E6E133-0F28-46D1-B00E-873B49C7ACDF'
		INSERT into #grouppagecontrols SELECT  '80770D4D-11EF-42CB-9C91-9E6A27AF35E4'
		INSERT into #grouppagecontrols SELECT  '5A9C7B5C-C0C6-4C31-817F-FDAA0D093B85'
		INSERT into #grouppagecontrols SELECT  'B12E74F7-48EB-4FAF-8231-B5C422F80C9A'
		SELECT Controls.* FROM
		(
			SELECT 
				cc.Id, 
				c.control_name, 
				cc.control_name_resource_key, 
				default_value, 
				fill_proc_name,
				ISNULL(DependOf1, '00000000-0000-0000-0000-000000000000') AS DependOf1, 
				ISNULL(DependOf2,'00000000-0000-0000-0000-000000000000') AS DependOf2, 
				ISNULL(DependOf3,'00000000-0000-0000-0000-000000000000') as DependOf3, 
				ISNULL(DependOf4,'00000000-0000-0000-0000-000000000000') as DependOf4,
				ISNULL(fill_proc_param, -1000) as fill_proc_param,
				param_name,
				print_order,
				@display AS Display
			FROM mart.report r
				INNER JOIN mart.v_report_control_collection cc ON cc.CollectionId = r.ControlCollectionId
				INNER JOIN mart.v_report_control c ON cc.ControlId = c.Id
			WHERE r.Id = @report_id
				AND cc.ControlId NOT IN(select * from #grouppagecontrols)
			--ORDER BY print_order
			UNION
			SELECT 
				cc.Id, 
				c.control_name, 
				cc.control_name_resource_key, 
				default_value, 
				fill_proc_name,
				ISNULL(DependOf1, '00000000-0000-0000-0000-000000000000') AS DependOf1, 
				ISNULL(DependOf2,'00000000-0000-0000-0000-000000000000') AS DependOf2, 
				ISNULL(DependOf3,'00000000-0000-0000-0000-000000000000') as DependOf3, 
				ISNULL(DependOf4,'00000000-0000-0000-0000-000000000000') as DependOf4,
				ISNULL(fill_proc_param, -1000) as fill_proc_param,
				param_name,
				print_order,
				@dont_display AS Display
			FROM mart.report r
				INNER JOIN mart.v_report_control_collection cc ON cc.CollectionId = r.ControlCollectionId
				INNER JOIN mart.v_report_control c ON cc.ControlId = c.Id
			WHERE r.Id = @report_id
				AND cc.ControlId IN(select * from #grouppagecontrols)
			--ORDER BY print_order
		) AS Controls
		ORDER BY Controls.print_order
	END

GO