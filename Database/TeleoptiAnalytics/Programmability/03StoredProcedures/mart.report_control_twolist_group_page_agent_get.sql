/****** Object:  StoredProcedure [mart].[report_control_group_page_agent_get]    Script Date: 09/29/2010 11:46:41 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_twolist_group_page_agent_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_twolist_group_page_agent_get]
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		HG
-- Create date: 2010-08-31
-- Description:	Loads GroupPage Agents to report control cboGroupPageAgent

-- Change Log
-- Date			Author	Description
------------------------------------------------
-- 2010-09-01	DJ		Adding dates and group_page_code
-- 2011-01-20	ME		Changing from person_id to person_code
-- 2012-01-09	ME		Pass BU to AllOwnedAgents
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- =============================================
-- exec mart.report_control_twolist_group_page_agent_get @date_from='2009-02-02 00:00:00',@date_to='2009-02-08 00:00:00',@group_page_code=N'59d10abe-8a7e-4474-904a-295e918fa5d2',@group_page_group_id=N'10',@report_id=12,@person_code='21ADAA78-13F2-4A43-9424-9C0100E53912',@language_id=-1,@bu_id='928DD0BC-BF40-412E-B970-9B5E015AADEA'
CREATE PROCEDURE [mart].[report_control_twolist_group_page_agent_get]
@date_from		datetime,
@date_to		datetime = @date_from,
@report_id		uniqueidentifier,
@group_page_code	uniqueidentifier,
@group_page_group_id		int,
@person_code	uniqueidentifier,
@language_id	int,
@bu_id uniqueidentifier
as

-------------------------------------------------------------------
CREATE TABLE #agents (counter int identity(1,1), person_code uniqueidentifier, name nvarchar(200)) 
--all

--IF (@show_all=1) --Note: if called from [mart].[report_control_twolist_agent_get] the value is false = 0

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


--not defined
-- 2010-08-02 There can be only one (removing distinct and where clause)
--INSERT #agents(person_code,name)
--SELECT 
--	person_code		= d.person_code,
--	name			= '00000000-0000-0000-0000-000000000001'
--FROM
--	mart.dim_person d WITH (NOLOCK)
--WHERE person_id=-1 --Not Defined

--Fix translation for "All" + "Not Defined"
UPDATE #agents
SET name=l.term_language
FROM 
	mart.language_translation l
WHERE #agents.name=l.term_english COLLATE database_default
AND l.language_id = @language_id



/*Get all PersonPeriods that user has permission to see. */
create table  #rights (right_id int)
	insert #rights SELECT * FROM mart.AllOwnedAgents(@person_code, @report_id, @bu_id)

INSERT #agents(person_code,name)
SELECT DISTINCT
--	id		= d.person_id,
	person_code = d.person_code,
	name		= d.person_name
FROM
	mart.dim_person d WITH(NOLOCK)
INNER JOIN  mart.bridge_group_page_person bridge WITH(NOLOCK)
	ON bridge.person_id = d.person_id
INNER JOIN  mart.dim_group_page dim WITH(NOLOCK)
	ON dim.group_page_id = bridge.group_page_id
WHERE dim.group_page_code = @group_page_code
AND dim.group_id = @group_page_group_id
AND	d.person_id in (SELECT period_id FROM #periods) --bara aktuella person perdioder
AND d.person_id<>-1
AND  d.person_id in (SELECT right_id FROM #rights)--bara de man har r?ttighet p?
AND d.business_unit_code=@bu_id --20081029 bara det bu som man anv?nder just nu i raptor
GROUP BY
--	d.person_id,
	d.person_code,
	d.person_name
ORDER BY d.person_name

SELECT person_code AS id, name
FROM #agents
ORDER BY counter


GO


