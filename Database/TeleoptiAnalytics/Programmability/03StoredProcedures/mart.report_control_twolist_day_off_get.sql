/****** Object:  StoredProcedure [mart].[report_control_twolist_day_off_get]    Script Date: 10/07/2008 16:50:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_twolist_day_off_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_twolist_day_off_get]
GO
/****** Object:  StoredProcedure [mart].[report_control_twolist_day_off_get]    Script Date: 10/07/2008 16:50:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--exec report_control_twolist_day_off_get 15,'C04803E2-8D6F-4936-9A90-9B2000148778',1053,'4AD43E49-B233-4D03-A813-9B2000102EBE'

CREATE PROCEDURE [mart].[report_control_twolist_day_off_get]
@report_id uniqueidentifier,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id uniqueidentifier
as
SET NOCOUNT ON
/*
Last Modified:20090211
20090211 Added new mart schema KJ
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
*/

SELECT
	id		= day_off_id,
	name	= isnull(term_language,day_off_name)
FROM
	mart.dim_day_off d WITH(NOLOCK)
INNER JOIN 
	mart.dim_business_unit b WITH(NOLOCK)
ON 
	b.business_unit_id=d.business_unit_id
LEFT JOIN
	mart.language_translation l
ON
	d.day_off_name = l.term_english	AND
	l.language_id = @language_id
WHERE day_off_id<>-1
AND b.business_unit_code=@bu_id 
ORDER BY isnull(term_language,day_off_name)



