IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_agent_get_multiple_teams]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_agent_get_multiple_teams]
GO


-- =============================================
-- Author:		Jonas n
-- Create date: 2011-10-26
--				2012-01-09 Pass BU to AllOwnedAgents
-- Description:	Loads agents to report selection control. Takes one or more teams as parameter.
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- =============================================
CREATE PROC [mart].[report_control_agent_get_multiple_teams]
@date_from		datetime,
@date_to		datetime = @date_from,
@site_id		int,
@team_set		nvarchar(max),
@report_id		uniqueidentifier,
@person_code	uniqueidentifier,
@language_id	int,
@bu_id uniqueidentifier
as

-------------------------------------------------------------------
CREATE TABLE #teams (id int)
CREATE TABLE #agents 
(
	counter int identity(1,1), 
	person_code uniqueidentifier, 
	name nvarchar(200)
) 
CREATE TABLE #rights (right_id int)
CREATE TABLE #periods
(
	period_id int NOT NULL,
	valid_from_date_local smalldatetime NOT NULL,
	valid_to_date_local smalldatetime NOT NULL
)

--get the chosen team(s)
INSERT #teams
	SELECT * FROM mart.SplitStringInt(@team_set)

--all
INSERT #agents(person_code,name)
	SELECT
		person_code		= '00000000-0000-0000-0000-000000000002',
		name			= 'All'	


--not defined
-- 2010-08-02 There can be only one (removing distinct and where clause)
INSERT #agents(person_code,name)
	SELECT 
		personcode		= '00000000-0000-0000-0000-000000000001',
		name			= d.person_name
	FROM
		mart.dim_person d
	WHERE person_id=-1 --Not Defined

--Fix translation for "All" + "Not Defined"
UPDATE #agents
SET name=l.term_language
FROM 
	mart.language_translation l
WHERE #agents.name=l.term_english COLLATE database_default
AND l.language_id = @language_id

--Get all PersonPeriods that user has permission to see.
INSERT #rights 
	SELECT * FROM mart.AllOwnedAgents(@person_code, @report_id, @bu_id)

--Get all agents in the period. note: each and every ones time_zone matters!
--The date provided from .asxp is date_Only. With no time_zone-info
--The given date is then matched against each and every agents person period in there time zone
INSERT INTO #periods
	SELECT * FROM [mart].[DimPersonLocalized](@date_from,@date_to)

INSERT #agents(person_code,name)
	SELECT DISTINCT
		person_code		= dp.person_code,
		name			= dp.person_name
	FROM mart.dim_person dp
	WHERE (dp.site_id = @site_id OR @site_id=-2)
	AND (dp.team_id IN (SELECT id FROM #teams) OR -2 IN (SELECT id FROM #teams))
	AND dp.person_id<>-1
	AND	dp.person_id in (SELECT period_id FROM #periods) --bara aktuella person perdioder
	AND  dp.person_id in (SELECT right_id FROM #rights)--bara de man har rättighet på
	AND dp.business_unit_code=@bu_id --bara det bu som man använder just nu i raptor
	GROUP BY
		dp.person_code,
		dp.person_name
	ORDER BY dp.person_name

SELECT person_code AS id, name
FROM #agents
ORDER BY counter

GO

