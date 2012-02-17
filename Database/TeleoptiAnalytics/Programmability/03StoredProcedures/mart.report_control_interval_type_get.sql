IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_interval_type_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_interval_type_get]
GO


--report_control_interval_type_get 1,'C04803E2-8D6F-4936-9A90-9B2000148778',1053,'4AD43E49-B233-4D03-A813-9B2000102EBE'


CREATE PROCEDURE [mart].[report_control_interval_type_get]

@report_id uniqueidentifier,
@person_code uniqueidentifier,
@language_id int,
@bu_id uniqueidentifier
/*

20080708 Added new table period_type
20080528 Added Day, Week, Month KJ
20080910 Added parameter @bu_id KJ
20080924 Added join to term_id KJ
20090211 Added new mart schema KJ
20090701 Added join on resourcekey, removed join on term_id KJ
20110715 Collation conflict for Analytics="Cyrillic_General_CI_AS" vs. tempdb = "Finnish_Swedish_CI_AS" 
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
*/
AS
CREATE TABLE #lang(
	[id] [int] NOT NULL,
	[name] [nvarchar](100) NULL,
	[resource_key] [nvarchar](500) NULL
)

INSERT INTO #lang	
SELECT	id= period_type_id,
		name=period_type_name,
		resource_key = resource_key
FROM mart.period_type p

UPDATE #lang
SET name=ISNULL(term_language,term_english)
FROM mart.language_translation l
INNER JOIN #lang ON #lang.resource_key =l.resourcekey collate database_default
WHERE language_id = @language_id

SELECT id, name 
FROM #lang
ORDER BY ID



GO

