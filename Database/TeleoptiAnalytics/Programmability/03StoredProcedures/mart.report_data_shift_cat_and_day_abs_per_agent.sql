IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_shift_cat_and_day_abs_per_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_shift_cat_and_day_abs_per_agent]
GO

-- =============================================
-- Author:		KJ
-- Create date: 2008-10-07
-- Last Updated:20090330
--					2011-01-25 Use agent_code instead of agent_id
--					2009-03-30 Fixed bug on time_zone KJ
--					2009-03-02 Excluded timezone UTC from time_zone check KJ
--					2009-02-11 Added new mart schema KJ
--					2009-01-08 Removed #bridge_time_zone KJ
--					2008-11-06 Changed # table for fact_schedule_day_count
--					2008-10-28 Added Day Offs 
--					2011-09-20 Added date. note: This SP is now used from two rdlc-files!
--					2011-10-21 Change paramaters @group_page_group_id and @teamd_id to 
--					@group_page_group_set and @team_set
--					2012-01-09 Pass BU to report_get_AgentsMultipleTeams
--					2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- Description:	Used by reports:
-- 1) select * from mart.report where report_id = 20
-- 2) select * from mart.report where report_id = 26
-- =============================================
--exec report_data_shift_cat_and_day_abs_per_agent @scenario_id=N'0',@date_from='2006-01-01 00:00:00:000',@date_to='2006-01-05 00:00:00:000',@site_id=N'-2',@team_set=N'-2',@agent_id=N'-2',@shift_category_set=N'1,30,35,13,27,24,22',@day_off_set=N'4',@absence_set=N'1,2,3,4,5,6',@time_zone_id=N'1',@person_code='CCA9770F-6483-4015-8761-9B430010EE0F',@report_id=19,@language_id=1053


CREATE PROCEDURE [mart].[report_data_shift_cat_and_day_abs_per_agent] 
@scenario_id int,
@date_from datetime,
@date_to datetime,
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@group_page_agent_code uniqueidentifier,
@site_id int,
@team_set nvarchar(max),
@agent_code uniqueidentifier,
@shift_category_set nvarchar(max),
@day_off_set nvarchar(max),
@absence_set nvarchar(max),
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
SET NOCOUNT ON

CREATE TABLE #result(
	counter int identity(1,1),
	person_id int,
	date_date smalldatetime,
	person_name nvarchar(200),
	category_name nvarchar(100),--shift_category/day_off/absence
	category_count int,
	category_type nvarchar(30) --'shift_category','day_off','absence'
	)


CREATE TABLE #fact_schedule_day_count(
	[local_date] smalldatetime NOT NULL,
	[person_id] [int] NOT NULL ,
	person_name nvarchar(200),
	[scenario_id] [smallint] NOT NULL ,
	[starttime] [smalldatetime] NULL,
	[shift_category_id] [int] NOT NULL ,
	[day_off_id] [int] NOT NULL,
	[absence_id] [int] NOT NULL ,
	[day_count] [int] NULL
)

CREATE TABLE #rights_agents (right_id int)
CREATE TABLE #rights_teams (right_id int)
CREATE TABLE #shift_categories(id int)
CREATE TABLE #dayoffs(id int)
CREATE TABLE #absences(id int)

INSERT INTO #rights_teams
	SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)

INSERT INTO #rights_agents
	EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_code, @person_code, @report_id, @business_unit_code

INSERT INTO #shift_categories
SELECT * FROM mart.SplitStringInt(@shift_category_set)

INSERT INTO #dayoffs
SELECT * FROM mart.SplitStringInt(@day_off_set)

INSERT INTO #absences
SELECT * FROM mart.SplitStringInt(@absence_set)

/* Get data from mart.fact_schedule_day_count */
INSERT INTO #fact_schedule_day_count (local_date,person_id,person_name,scenario_id,starttime,shift_category_id,day_off_id,absence_id,day_count)
SELECT
	d.date_date,
	p.person_id,
	p.person_name,
	fs.scenario_id,
	fs.starttime,
	fs.shift_category_id,
	fs.day_off_id,
	fs.absence_id,
	fs.day_count
FROM mart.fact_schedule_day_count fs
INNER JOIN mart.dim_person p
	ON fs.person_id=p.person_id
INNER JOIN mart.dim_date d
	ON fs.shift_startdate_local_id = d.date_id
WHERE d.date_date BETWEEN  @date_from AND @date_to
AND fs.scenario_id=@scenario_id
AND p.team_id IN(select right_id from #rights_teams)--where viewer has team permissions
AND p.person_id in (SELECT right_id FROM #rights_agents)--where viewer has agent permissions



/*First Shift Categories*/
INSERT #result(person_id,person_name,category_name,category_count,category_type,date_date)
SELECT	f.person_id,
		f.person_name,
		sc.shift_category_name,
		1, --will/should only be one (1) per day, agent, scenario
		'shift_category',
		f.local_date
FROM 
	#fact_schedule_day_count f
INNER JOIN mart.dim_shift_category sc
	ON sc.shift_category_id=f.shift_category_id
WHERE sc.shift_category_id IN (SELECT id FROM #shift_categories)
AND f.shift_category_id<>-1 --is a shift

/*Then Day Off*/
INSERT #result(person_id,person_name,category_name,category_count,category_type,date_date)
SELECT	p.person_id,
		p.person_name,
		ISNULL(lt.term_language,dd.day_off_name),
		1, --will/should only be one (1) per day, agent, scenario
		'day_off',
		f.local_date
FROM 
	#fact_schedule_day_count f
INNER JOIN mart.dim_person p
	ON f.person_id=p.person_id
INNER JOIN mart.dim_day_off dd
	ON dd.day_off_id=f.day_off_id
LEFT JOIN mart.language_translation lt
	ON dd.day_off_name=lt.term_english AND lt.language_id=@language_id
WHERE dd.day_off_id in (SELECT id FROM #dayoffs)
AND f.day_off_id<>-1 --is a dayoff

/*Then Full Day Absences*/
INSERT #result(person_id,person_name,category_name,category_count,category_type,date_date)
SELECT	p.person_id,
		p.person_name,
		da.absence_name,
		1,
		'absence',
		f.local_date
FROM 
	#fact_schedule_day_count f
INNER JOIN mart.dim_person p
	ON f.person_id=p.person_id
INNER JOIN mart.dim_absence da
	ON da.absence_id=f.absence_id
WHERE da.absence_id IN (SELECT id FROM #absences)
AND f.absence_id<>-1 --is absence

--return RS
SELECT *
FROM #result 
ORDER BY counter  


GO