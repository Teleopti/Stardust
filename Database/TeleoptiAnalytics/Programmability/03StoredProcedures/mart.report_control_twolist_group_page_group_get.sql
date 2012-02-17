IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_twolist_group_page_group_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_twolist_group_page_group_get]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jonas n
-- Create date: 2011-10-19
-- Description:	Load data for report control twolistGroupPageGroup
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- =============================================
CREATE PROCEDURE [mart].[report_control_twolist_group_page_group_get]
@group_page_code uniqueidentifier, --parent group page
@report_id uniqueidentifier,
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


