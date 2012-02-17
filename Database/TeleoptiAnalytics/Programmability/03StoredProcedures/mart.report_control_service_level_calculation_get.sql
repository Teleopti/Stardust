IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_service_level_calculation_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_service_level_calculation_get]
GO


CREATE Proc [mart].[report_control_service_level_calculation_get]
@report_id uniqueidentifier,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id uniqueidentifier
as
-- Updated:
--	20080910 Added parameter @bu_id KJ
--	20090211 Added new mart schema KJ
--  20090701 Added new join on resource_key and removed join on term_id KJ
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
SELECT
	id		= service_level_id,
	name	= isnull(term_language,service_level_name)
FROM
	mart.service_level_calculation s
LEFT JOIN
	mart.language_translation l
ON
	s.resource_key = l.resourcekey		AND
	l.language_id = @language_id
ORDER BY isnull(term_language,service_level_name)

GO

