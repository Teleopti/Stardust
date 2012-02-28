IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_twolist_site_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_twolist_site_get]
GO

--EXEC report_control_twolist_site_get '2006-01-01','2006-01-31',12,'C04803E2-8D6F-4936-9A90-9B2000148778',1033,'4AD43E49-B233-4D03-A813-9B2000102EBE'
/*
CREATED:	20080915 KJ
-- =============================================
-- Change Log:
-- Date			By		Description
-- =============================================
-- 2009-02-11	KaJe	Added new mart schema 
-- 2009-04-27	DaJo	maxdate format (default input)
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
*/

CREATE Proc [mart].[report_control_twolist_site_get]

@date_from datetime,
@date_to datetime = '20591231',
@report_id uniqueidentifier,
@person_code uniqueidentifier,
@language_id int,
@bu_id uniqueidentifier
as

CREATE TABLE #sites (counter int identity(1,1),id int, name nvarchar(50)) 
CREATE TABLE  #rights (right_id int)
CREATE TABLE #periods
(
	period_id int NOT NULL,
	valid_from_date_local smalldatetime NOT NULL,
	valid_to_date_local smalldatetime NOT NULL
)

/*Get all sites that user has permission to see. */
insert #rights SELECT * FROM mart.AllOwnedSites(@person_code, @report_id)

--Get all agents in the period. note: each and every ones time_zone matters!
--The date provided from .asxp is date_Only. With no time_zone-info
--The given date is then matched against each and every agents person period in there time zone
INSERT INTO #periods
SELECT * FROM [mart].[DimPersonLocalized](@date_from,@date_to)

INSERT #sites(id,name)
SELECT DISTINCT
	id		= d.site_id,
	name	= isnull(l.term_language,d.site_name)
FROM
	
	mart.dim_person d

LEFT JOIN
	mart.language_translation l
ON
	l.term_english = d.site_name AND
	l.language_id = @language_id	
WHERE d.site_id=-1 --only those with no sitename
AND	d.person_id in (SELECT period_id FROM #periods) --bara aktuella person perdioder
GROUP BY
	d.site_id,
	isnull(l.term_language,d.site_name)
ORDER BY isnull(l.term_language,d.site_name)

/*Get all sites that user has permission to see. */
insert #rights SELECT * FROM mart.AllOwnedSites(@person_code, @report_id)

INSERT #sites(id,name)
SELECT DISTINCT
	id		= d.site_id,
	name	= isnull(l.term_language,d.site_name)
FROM
	
	mart.dim_person d

LEFT JOIN
	mart.language_translation l
ON
	l.term_english = d.site_name AND
	l.language_id = @language_id	
WHERE d.site_id<>-1 --all the rest
AND	d.person_id in (SELECT period_id FROM #periods) --bara aktuella person perdioder
AND d.site_id in (SELECT right_id FROM #rights)--bara de man har rättighet på
AND d.business_unit_code=@bu_id --20080910 bara det bu som man använder just nu i raptor
GROUP BY
	d.site_id,
	isnull(l.term_language,d.site_name)
ORDER BY isnull(l.term_language,d.site_name)


SELECT id, name
FROM #sites
ORDER BY counter

GO

