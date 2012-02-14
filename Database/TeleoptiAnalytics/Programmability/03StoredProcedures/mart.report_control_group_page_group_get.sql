IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_group_page_group_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_group_page_group_get]
GO



-- =============================================
-- Author:		DJ
-- Create date: 2010-09-01
-- Description:	Loads Groups from a specific GroupPage

-- EXEC mart.report_control_group_page_group_get @report_id=13,@person_code='928DD0BC-BF40-412E-B970-9B5E015AADEA',@group_page_code='0AB77CB7-F2C1-43FC-9F99-152A1DEB6968',@language_id=1025,@bu_id='928DD0BC-BF40-412E-B970-9B5E015AADEA'
-- Change Log
-- Date			Author	Description
-- =============================================
CREATE PROCEDURE [mart].[report_control_group_page_group_get] 
@group_page_code uniqueidentifier, --parent group page
@report_id int,
@person_code uniqueidentifier, -- user
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id uniqueidentifier
AS

SELECT DISTINCT
	id		= grp.group_id,
	name	= grp.group_name
FROM
	mart.dim_group_page grp
WHERE	grp.business_unit_code = @bu_id
AND grp.group_page_code = @group_page_code
ORDER BY grp.group_name


GO


