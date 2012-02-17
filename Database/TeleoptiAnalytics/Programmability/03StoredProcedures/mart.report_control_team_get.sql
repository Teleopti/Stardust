IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_team_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_team_get]
GO

--exec report_control_team_get '2006-01-01','2006-01-02',0,10,'C04803E2-8D6F-4936-9A90-9B2000148778',1043,'4AD43E49-B233-4D03-A813-9B2000102EBE'

CREATE PROC [mart].[report_control_team_get]

@date_from		datetime,
@date_to		datetime = @date_from,
@site_id		int,
@report_id		uniqueidentifier,
@person_code	uniqueidentifier,
@language_id	int,
@bu_id uniqueidentifier
as

/*
Last modified:20090706
20090706 Removed reference to mart.langauge_term KJ
20080409 KJ Added table #sites so that ordering works("All" first, not defined after and then the rest of the sites i asc order)
20080626 Added user permissions KJ
20080910 Added parameter @bu_id KJ
20080924 Changed language translation handling if @language is missing in language_translation(then return english) KJ
20081029 Bug fix on correct bu_id KJ
20090211 Added new mart schema KJ
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
*/
--select * from dim_team
--Load "all" first
CREATE TABLE #teams (counter int identity(1,1),id int, name nvarchar(50)) 

INSERT #teams(id,name)
SELECT
	id		= -2,
	name	= 'All'	
	
UPDATE #teams
SET name=l.term_language
FROM 
	mart.language_translation l
WHERE #teams.name=l.term_english COLLATE database_default
AND l.language_id = @language_id

--not defined sedan
INSERT #teams(id,name)
SELECT DISTINCT
	id		= d.team_id,
	name	= isnull(term_language,d.team_name)
FROM
	dim_person d
LEFT JOIN
	mart.language_translation l
ON	l.term_english = d.site_name AND
	l.language_id = @language_id	
WHERE
	(@date_from	between d.valid_from_date	AND d.valid_to_date OR 
	@date_to	between d.valid_from_date	AND d.valid_to_date)	AND
	(d.site_id = -1)
	AND d.team_id=-1 --ONLY THOSE WITHOUT TEAM NAME
GROUP BY
	d.team_id,
	isnull(l.term_language,d.team_name)
ORDER BY isnull(l.term_language,d.team_name)


/*Get all teams that user has permission to see. */
create table  #rights (right_id int)
	insert #rights SELECT * FROM mart.AllOwnedTeams(@person_code, @report_id)

--Get all agents in the period. note: each and every ones time_zone matters!
--The date provided from .asxp is date_Only. With no time_zone-info
--The given date is then matched against each and every agents person period in there time zone
CREATE TABLE #periods
(
	period_id int NOT NULL,
	valid_from_date_local smalldatetime NOT NULL,
	valid_to_date_local smalldatetime NOT NULL
)
INSERT INTO #periods
SELECT * FROM [mart].[DimPersonLocalized](@date_from,@date_to)

INSERT #teams(id,name)
SELECT DISTINCT
	id		= d.team_id,
	name	= isnull(l.term_language,d.team_name)
FROM

	mart.dim_person d
LEFT JOIN
	mart.language_translation l
ON
	l.term_english = d.site_name AND
	l.language_id = @language_id	
WHERE (d.site_id = @site_id OR @site_id=-2)
AND d.team_id<>-1
AND d.person_id in (SELECT period_id FROM #periods) --bara aktuella person perdioder
AND d.team_id in (SELECT right_id FROM #rights)--bara de man har rättighet på
AND d.business_unit_code=@bu_id --20081029 bara det bu som man använder just nu i raptor
GROUP BY
	d.team_id,
	isnull(l.term_language,d.team_name)
ORDER BY isnull(l.term_language,d.team_name)

SELECT id, name
FROM #teams
ORDER BY counter

GO

