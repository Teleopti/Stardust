IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_adherence_calculation_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_adherence_calculation_get]
GO

/*
Last Updated:2009-07-01
20090211 New Mart schema KJ
20080910 Added parameter @bu_id KJ
20090701 Added join on new field resource_key instead of term_id KJ
2012-02-15 Changed to uniqueidentifier as report_id - Ola
*/
CREATE Proc [mart].[report_control_adherence_calculation_get]
@report_id uniqueidentifier,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id uniqueidentifier
as

SELECT
	id		= adherence_id,
	name	= isnull(term_language,adherence_name)
FROM
	mart.adherence_calculation a
LEFT JOIN
	mart.language_translation l
ON
	a.resource_key = l.resourcekey	AND
	l.language_id = @language_id
ORDER BY isnull(term_language,adherence_name)

GO

