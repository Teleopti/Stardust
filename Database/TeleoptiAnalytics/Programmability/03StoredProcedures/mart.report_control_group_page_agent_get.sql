IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_group_page_agent_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_group_page_agent_get]
GO

-- =============================================
-- Author:		HG
-- Create date: 2010-08-31
-- Description:	Loads GroupPage Agents to report control cboGroupPageAgent

-- Change Log
-- Date			Author	Description
------------------------------------------------
-- 2010-09-01	DJ		Adding dates and group_page_code
-- 2011-01-21	ME		Use person_code instead of person_id
-- 2012-01-09	ME		Pass BU to AllOwnedAgents
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- =============================================
-- exec mart.report_control_group_page_agent_get @date_from='2009-02-02 00:00:00',@date_to='2009-02-08 00:00:00',@group_page_code=N'59d10abe-8a7e-4474-904a-295e918fa5d2',@group_page_group_id=N'10',@report_id=12,@person_code='21ADAA78-13F2-4A43-9424-9C0100E53912',@language_id=-1,@bu_id='928DD0BC-BF40-412E-B970-9B5E015AADEA'
CREATE PROCEDURE [mart].[report_control_group_page_agent_get]
@date_from				datetime,
@date_to				datetime = @date_from,
@report_id				uniqueidentifier,
@group_page_code		uniqueidentifier,
@group_page_group_id	int,
@person_code			uniqueidentifier,
@language_id			int,
@bu_id uniqueidentifier
as

-------------------------------------------------------------------
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


--Get all agents in the period. note: each and every ones time_zone matters!
--The date provided from .asxp is date_Only. With no time_zone-info
--The given date is then matched against each and every agents person period in there time zone
INSERT INTO #periods
	SELECT * FROM [mart].[DimPersonLocalized](@date_from,@date_to)


--all
INSERT #agents(person_code,name)
	SELECT
		id		= '00000000-0000-0000-0000-000000000002',
		name	=  'All'	
	FROM
		mart.dim_person d WITH(NOLOCK)
	WHERE person_id=-1 --Not Defined

--Fix translation for "All" + "Not Defined"
UPDATE #agents
SET name=l.term_language
FROM 
	mart.language_translation l
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
		mart.dim_person d WITH(NOLOCK)
	INNER JOIN  mart.bridge_group_page_person bridge WITH(NOLOCK)
		ON bridge.person_id = d.person_id
	INNER JOIN  mart.dim_group_page dim WITH(NOLOCK)
		ON dim.group_page_id = bridge.group_page_id
	WHERE dim.group_page_code = @group_page_code
		AND dim.group_id = @group_page_group_id
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