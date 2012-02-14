IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_twolist_team_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_twolist_team_get]
GO


/*
CREATED:	20080915 KJ
-- =============================================
-- Change Log:
-- Date			By		Description
-- =============================================
-- 2009-02-11	KaJe	Added new mart schema 
-- 2009-04-27	DaJo	maxdate format (default input)
-- 2011-10-19	JoNo	Since this sp is not in use as far as I can see i recycle this one to the new control for twolist team.
-- 2012-01-03	JoNo	Added parameter @group_page_code for dynamic loading purposes. Not in use in SP.
*/

CREATE PROC [mart].[report_control_twolist_team_get]

@date_from			datetime,
@date_to			datetime = @date_from,
@group_page_code	uniqueidentifier,
@site_id			int,
@report_id			int,
@person_code		uniqueidentifier,
@language_id		int,
@bu_id				uniqueidentifier
as

CREATE TABLE #teams (counter int identity(1,1),id int, name nvarchar(50))
CREATE TABLE  #rights (right_id int)
CREATE TABLE #periods
(
	period_id int NOT NULL,
	valid_from_date_local smalldatetime NOT NULL,
	valid_to_date_local smalldatetime NOT NULL
)

--not defined
INSERT INTO #teams(id,name)
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
INSERT INTO #rights 
	SELECT * FROM mart.AllOwnedTeams(@person_code, @report_id)

--Get all agents in the period. note: each and every ones time_zone matters!
--The date provided from .asxp is date_Only. With no time_zone-info
--The given date is then matched against each and every agents person period in there time zone
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

SELECT 
	id, 
	name
FROM #teams
ORDER BY counter

GO

