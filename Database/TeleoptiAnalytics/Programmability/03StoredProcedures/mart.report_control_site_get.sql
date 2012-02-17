IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_site_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_site_get]
GO

--exec mart.report_control_site_get @date_from='2012-02-16 00:00:00',@date_to='2012-02-16 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@report_id='0E3F340F-C05D-4A98-AD23-A019607745C9',@person_code='10957AD5-5489-48E0-959A-9B5E015B2B5C',@language_id=1053,@bu_id='928DD0BC-BF40-412E-B970-9B5E015AADEA'

CREATE Proc [mart].[report_control_site_get]
@date_from datetime,
@date_to datetime = @date_from,
@group_page_code uniqueidentifier,
@report_id uniqueidentifier,
@person_code uniqueidentifier,
@language_id int,
@bu_id uniqueidentifier
as
/*
Last modified: 20090706
20090706 removed reference to mart.language_term KJ
20080409 KJ Added table #sites so that ordering works("All" first, not defined after and then the rest of the sites i asc order)
20080626 Added user permissions
20080910 Added parameter @bu_id KJ
20080924 Changed language translation handling if @language is missing in language_translation(then return english) KJ
20090211 Added new schema mart KJ
20090414 Added COLLATE database_default for joins on strings KJ
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
*/

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
SELECT
	id		= -2,
	name	= 'All'	


UPDATE #sites
SET name=l.term_language
FROM 
	mart.language_translation l
WHERE #sites.name=l.term_english COLLATE database_default
AND l.language_id = @language_id


INSERT #sites(id,name)
SELECT DISTINCT
	id		= d.site_id,
	name	= isnull(l.term_language,d.site_name)
FROM
	
	mart.dim_person d

LEFT JOIN
	mart.language_translation l
ON
	l.term_english = d.site_name COLLATE database_default
	AND
	l.language_id = @language_id	
WHERE  d.site_id=-1 --only those with no sitename
AND	d.person_id in (SELECT period_id FROM #periods) --bara aktuella person perdioder
GROUP BY
	d.site_id,
	isnull(l.term_language,d.site_name)
ORDER BY isnull(l.term_language,d.site_name)


INSERT #sites(id,name)
SELECT DISTINCT
	id		= d.site_id,
	name	= isnull(l.term_language,d.site_name)
FROM
	
	mart.dim_person d

LEFT JOIN
	mart.language_translation l
ON
	l.term_english = d.site_name COLLATE database_default
	AND
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

