IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_twolist_agent_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_twolist_agent_get]
GO

--exec mart.report_control_twolist_agent_get '2006-01-01','2006-01-02',0,21,10,'AE83E89B-01A1-4ADB-8605-9B5E015B2569',1053,'928DD0BC-BF40-412E-B970-9B5E015AADEA'
--exec mart.report_control_twolist_agent_get @date_from='2004-07-25 00:00:00',@date_to='2009-03-07 00:00:00',@site_id=N'0',@team_id=N'7',@report_id=12,@person_code='10957AD5-5489-48E0-959A-9B5E015B2B5C',@language_id=-1,@bu_id='928DD0BC-BF40-412E-B970-9B5E015AADEA'
--exec mart.report_control_twolist_agent_get @date_from='2011-01-17 00:00:00',@date_to='2011-01-17 00:00:00',@site_id=N'-2',@team_id=N'-2',@report_id=12,@person_code='21ADAA78-13F2-4A43-9424-9C0100E53912',@language_id=-1,@bu_id='928DD0BC-BF40-412E-B970-9B5E015AADEA'
CREATE PROC [mart].[report_control_twolist_agent_get]
@date_from		datetime,
@date_to		datetime,
@site_id		int,
@team_id		int,
@report_id		int,
@person_code	uniqueidentifier,
@language_id	int,
@bu_id uniqueidentifier
as
/*
Created:20080619 KJ
Last modified:20090330
20090330 Changed order by
20080626 Added permissions KJ
20080910 Added parameter @bu_id KJ
20080924 Chnaged condition for Not Defined
20090211 Added new mart schema KJ
20100729 Bug #11248 DJ
20100802 Bug #11384, re-factor
20101108 Bug #12320 DJ - Putting local date stuff in a function
20110117 PBI #12344 ME - Use person code instead of person id
2012-01-09 Pass BU to AllOwnedAgents
*/

/*
--alternativt kan man göra så här:
DECLARE @show_all bit
SET @show_all = 0 --false

EXEC [mart].[report_control_agent_get]
@date_from,
@date_to,
@site_id,
@team_id,
@report_id,
@person_code,
@language_id,
@bu_id,
@show_all
*/

CREATE TABLE #agents (counter int identity(1,1), name nvarchar(200), person_code uniqueidentifier) 

--select * from dim_person
--not defined
-- 2010-08-20 There can be only one (removing distinct and where clause)
INSERT #agents(name, person_code)
SELECT 
	name	= d.person_name,
	person_code = '00000000-0000-0000-0000-000000000001'
FROM
	mart.dim_person d
WHERE person_id=-1 --Not Defined

--Fix translation "Not Defined"
UPDATE #agents
SET name=l.term_language
FROM 
	mart.language_translation l
WHERE #agents.name=l.term_english COLLATE database_default
AND l.language_id = @language_id

--Get all agents/persons that user has permission to see.
create table  #rights (right_id int)
insert #rights SELECT * FROM mart.AllOwnedAgents(@person_code, @report_id, @bu_id)

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

INSERT #agents(name,person_code)
SELECT DISTINCT
--	id		= dp.person_id,
	name	= dp.person_name,
	person_code = dp.person_code
FROM mart.dim_person dp
WHERE (dp.site_id = @site_id OR @site_id=-2)
AND (dp.team_id = @team_id OR @team_id=-2)
AND dp.person_id<>-1 --inte not defined
AND	dp.person_id in (SELECT period_id FROM #periods) --bara aktuella person perdioder
AND dp.person_id in (SELECT right_id FROM #rights)--bara de man har rättighet på
AND dp.business_unit_code=@bu_id --bara det bu som man använder just nu i raptor
GROUP BY
	dp.person_code,
--	dp.person_id,
	dp.person_name
ORDER BY dp.person_name

SELECT person_code as id, name
FROM #agents
ORDER BY counter

GO

