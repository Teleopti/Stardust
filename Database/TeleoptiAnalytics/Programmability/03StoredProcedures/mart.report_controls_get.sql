IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_controls_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_controls_get]
GO


-- exec mart.report_controls_get @report_id='3DC549EA-2BB5-4643-8523-CA5FC02FE571',@group_page_code='D5AE2A10-2E17-4B3C-816C-1A0E81CD767C'	

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

DECLARE @display BIT
DECLARE @dont_display BIT
DECLARE @interval_length int

SET @display = 1
SET @dont_display = 0
SELECT @interval_length = value FROM mart.sys_configuration WHERE [key] = 'IntervalLengthMinutes'

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
		--henrikl, 7 rows added for custom report
		INSERT into #grouppagecontrols SELECT  'D464415A-4199-4EB7-BA2F-A471760F09BC'
		INSERT into #grouppagecontrols SELECT  '0594AA2A-66DD-48E9-BA88-B68B98152B76'
		INSERT into #grouppagecontrols SELECT  'D11B7738-8655-43DF-814B-5BB3FFC38C1B'
		INSERT into #grouppagecontrols SELECT  '710F05D5-DF77-44D6-8B58-5326B6665989'
		INSERT into #grouppagecontrols SELECT  '6B303D5A-203F-4AA0-80EB-E40B791D281B'
		INSERT into #grouppagecontrols SELECT  '05E8C12F-FC3B-465C-B019-105806CC1029'
		INSERT into #grouppagecontrols SELECT  '6BED0B7F-2434-4699-8BBB-FBCB2B3D7BA7'
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
				@display AS display,
				@interval_length AS interval_length_minutes
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
				@dont_display AS display,
				@interval_length AS interval_length_minutes
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
		--henrikl, 6 rows added for custom report
		INSERT into #grouppagecontrols SELECT  'A4DE2EF1-DB3C-4CC5-A4E6-56D1DF8A06CB'
		INSERT into #grouppagecontrols SELECT  'FFB22DC9-793B-4586-AEAC-C4F19850A5FF'
		INSERT into #grouppagecontrols SELECT  '15A354FD-A10E-4045-AD9D-4B8945F4A2A9'
		INSERT into #grouppagecontrols SELECT  '2555679A-73EE-4E7C-950A-AF547119AE99'
		INSERT into #grouppagecontrols SELECT  '1A292B23-F497-4AFF-B616-93A50DCFF639'
		INSERT into #grouppagecontrols SELECT  '31AFCFA9-7B79-4A67-A40E-4073A22FB67F'
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
				@display AS Display,
				@interval_length AS interval_length_minutes
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
				@dont_display AS Display,
				@interval_length AS interval_length_minutes
			FROM mart.v_report r
				INNER JOIN mart.v_report_control_collection cc ON cc.CollectionId = r.ControlCollectionId
				INNER JOIN mart.v_report_control c ON cc.ControlId = c.Id
			WHERE r.Id = @report_id
				AND cc.ControlId IN(select * from #grouppagecontrols)
			--ORDER BY print_order
		) AS Controls
		ORDER BY Controls.print_order
	END

GO