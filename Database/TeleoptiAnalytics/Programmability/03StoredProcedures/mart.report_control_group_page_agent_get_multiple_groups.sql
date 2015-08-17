IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_group_page_agent_get_multiple_groups]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_group_page_agent_get_multiple_groups]
GO

-- =============================================
-- Author:		Jonas n
-- Create date: 2011-10-26
-- Description:	Loads GroupPage Agents to report selection control. Takes one or more groups as parameter.

-- Change Log
-- Date			Author	Description
------------------------------------------------
-- 2012-01-09	ME		Pass BU to AllOwnedAgents
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- =============================================
CREATE PROCEDURE [mart].[report_control_group_page_agent_get_multiple_groups]
@date_from				datetime,
@date_to				datetime = @date_from,
@report_id				uniqueidentifier,
@group_page_code		uniqueidentifier,
@group_page_group_set	nvarchar(max),
@person_code			uniqueidentifier,
@language_id			int,
@bu_id uniqueidentifier
as

-------------------------------------------------------------------
CREATE TABLE #teams (id int)
CREATE TABLE #periods
(
	period_id int NOT NULL,
	valid_from_date_local smalldatetime NOT NULL,
	valid_to_date_local smalldatetime NOT NULL
)
CREATE TABLE #agents 
(
	counter int identity(1,1), 
	person_code uniqueidentifier, 
	name nvarchar(200)
)
CREATE TABLE #rights (right_id int)

--get the chosen team(s)
INSERT #teams
	SELECT * FROM mart.SplitStringInt(@group_page_group_set)

--Get all agents in the period. note: each and every ones time_zone matters!
--The date provided from .asxp is date_Only. With no time_zone-info
--The given date is then matched against each and every agents person period in there time zone
INSERT INTO #periods
	SELECT * FROM [mart].[DimPersonLocalized](@date_from,@date_to)


--all we don't want all in adherence per agent (6A3EB69B-690E-4605-B80E-46D5710B28AF)
if @report_id <> '6A3EB69B-690E-4605-B80E-46D5710B28AF'
INSERT #agents(person_code,name)
	SELECT
		id		= '00000000-0000-0000-0000-000000000002',
		name	=  'All'	
	FROM
		mart.dim_person d WITH (NOLOCK)
	WHERE person_id=-1 --Not Defined

--Fix translation for "All" + "Not Defined"
UPDATE #agents
SET name=l.term_language
FROM 
	mart.language_translation l WITH (NOLOCK)
WHERE #agents.name=l.term_english COLLATE database_default
	AND l.language_id = @language_id



/*Get all PersonPeriods that user has permission to see. */
INSERT #rights 
	SELECT * FROM mart.AllOwnedAgents(@person_code, @report_id, @bu_id)

INSERT #agents(person_code,name)
	SELECT DISTINCT
		person_code	= d.person_code,
		name		= d.person_name
	FROM
		mart.dim_person d WITH (NOLOCK)
	INNER JOIN  mart.bridge_group_page_person bridge WITH (NOLOCK)
		ON bridge.person_id = d.person_id
	INNER JOIN  mart.dim_group_page dim WITH (NOLOCK)
		ON dim.group_page_id = bridge.group_page_id
	WHERE dim.group_page_code = @group_page_code
		AND dim.group_id IN (SELECT id FROM #teams)
		AND	d.person_id IN (SELECT period_id FROM #periods)
		AND d.person_id <> -1
		AND  d.person_id IN (SELECT right_id FROM #rights)
		AND d.business_unit_code=@bu_id
	GROUP BY
		d.person_code,
		d.person_name
	ORDER BY d.person_name

SELECT person_code AS id, name
FROM #agents
ORDER BY counter

GO