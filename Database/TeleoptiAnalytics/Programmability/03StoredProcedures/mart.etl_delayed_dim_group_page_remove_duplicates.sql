IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_delayed_dim_group_page_remove_duplicates]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_delayed_dim_group_page_remove_duplicates]
GO
-- =============================================
-- Author:		<KJ>
-- Create date: <2016-09-06,>
-- Description:	<#40417: Do a proper clean of bridge_group_page_person in case of mixed group_page_codes in BUs and also make sure no duplicates in dim_group_page. Run it once as a delayed job in Nightly.>
-- =============================================
CREATE PROCEDURE mart.etl_delayed_dim_group_page_remove_duplicates
@delete_all bit = 0,
@is_delayed_job bit=1
AS
BEGIN

	SELECT dgp.group_page_code, dgp.group_name,dgp.business_unit_id ,group_page_id AS remove_group_page_id, keep_group_page_id
	INTO #dupl
	FROM mart.dim_group_page dgp
	JOIN (SELECT group_page_code,group_name,business_unit_id, MIN(group_page_id) AS keep_group_page_id
	  FROM mart.dim_group_page
	  GROUP BY group_page_code,group_name,business_unit_id HAVING COUNT(group_page_id) > 1) err ON dgp.group_page_code = err.group_page_code and dgp.group_name=err.group_name and dgp.business_unit_id=err.business_unit_id
	WHERE group_page_id <> keep_group_page_id
	order by group_page_code
	
	IF @delete_all=1
	BEGIN
	--we clean all to make sure no mixed references exists between BUs
	DELETE FROM mart.bridge_group_page_person
	END
	ELSE
	BEGIN
		DELETE FROM mart.bridge_group_page_person
		FROM mart.bridge_group_page_person AS fact
		INNER JOIN #dupl AS dupl
		ON fact.group_page_id = dupl.remove_group_page_id
	END

	-- Delete  duplicates from dim person
	DELETE mart.dim_group_page
	FROM mart.dim_group_page gp
	INNER JOIN #dupl AS dupl
	ON gp.group_page_id = dupl.remove_group_page_id

	DROP TABLE #dupl
END
GO
