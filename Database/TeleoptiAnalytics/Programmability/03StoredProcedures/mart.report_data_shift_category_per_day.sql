IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_shift_category_per_day]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_shift_category_per_day]
GO
-- =============================================
-- Author:		KJ
-- Create date: 2008-10-01
-- Last Updated:20090302
--				2009-03-02 Excluded timezone UTC from time_zone check KJ
--				2009-02-11 Added new mart schema KJ
--				2008-11-06 Changed # table for fact_schedule_day_count
--				2011-01-25 Use agent_code instead of agent_id
--				2011-06-22 Azure fix DJ
--				2012-01-09 Pass BU to ReportAgents ME
--				2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- Description:	Used by report Shift Category per Day
-- =============================================
--exec report_data_shift_category_per_day @scenario_id=N'0',@date_from='2006-01-08 00:00:00:000',@date_to='2006-01-08 00:00:00:000',@site_id=N'0',@team_set=N'82',@agent_id=N'26',@shift_category_set=N'16,13,14,18,37,10,11,28,23,21,17,3,40,4,36,35,24,15,34,32,26,0,33,38,2,31,20,41,8,7,5,29,30,25,19,39,22,1,27,9,12,6',@time_zone_id=N'1',@person_code='0D035EB3-3BCC-4958-B93E-9B2800F35A1D',@report_id=19,@language_id=1033

CREATE PROCEDURE [mart].[report_data_shift_category_per_day] 
@scenario_id int,
@date_from datetime,
@date_to datetime,
@site_id int,
@team_set nvarchar(max),
@agent_code uniqueidentifier,
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@group_page_agent_code uniqueidentifier,
@shift_category_set nvarchar(max),
@time_zone_id int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
SET NOCOUNT ON
CREATE TABLE #rights_teams (right_id int)
CREATE TABLE #rights_agents (right_id int)
CREATE TABLE #shift_categories(id int)
CREATE TABLE #fact_schedule_day_count(
	local_date smalldatetime NOT NULL,
	[person_id] [int] NOT NULL,
	[scenario_id] [smallint] NOT NULL,
	[starttime] [smalldatetime] NULL,
	[shift_category_id] [int] NOT NULL,
	[day_off_id] [int] NOT NULL,
	[absence_id] [int] NOT NULL,
	[day_count] [int] NULL
)
CREATE TABLE #result(
	date smalldatetime,
	shift_category_name nvarchar(100),
	category_count int,
	category_percent decimal(19,3),
	total_count int,
	category_total INT,
	category_total_percent decimal(19,3),
	sum_total_count int,
	hide_time_zone bit
)
CREATE TABLE #total_count(
	[date] [smalldatetime] NULL,
	[total_count] [int] NULL
)
CREATE TABLE #total_category(
	[shift_category_name] [nvarchar](100) NULL,
	[total_category] [int] NULL
) 

/* Get relevant agents */
INSERT INTO #rights_agents
EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_code, @person_code, @report_id, @business_unit_code


/*Get all teams that user has permission to see. */
INSERT INTO #rights_teams
SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)

INSERT INTO #shift_categories
SELECT * FROM mart.SplitStringInt(@shift_category_set)

/* Check if time zone will be hidden (if only one exist then hide) */
DECLARE @hide_time_zone bit
SET @hide_time_zone = 1

/*get the data to query by clustered key*/
INSERT INTO #fact_schedule_day_count
SELECT
	d.date_date,
	fs.person_id,
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

INSERT INTO #result(date,shift_category_name,category_count,hide_time_zone)
SELECT	f.local_date,
		sc.shift_category_name,
		sum(day_count),
		@hide_time_zone
FROM 
	#fact_schedule_day_count f
INNER JOIN mart.dim_person p
	ON f.person_id=p.person_id
INNER JOIN mart.dim_shift_category sc
	ON sc.shift_category_id=f.shift_category_id
WHERE sc.shift_category_id IN (SELECT id FROM #shift_categories)
AND f.shift_category_id<>-1 --not shifts
GROUP BY f.local_date,sc.shift_category_name

INSERT INTO #total_count
SELECT date,
		sum(category_count)AS 'total_count'
FROM #result
GROUP BY date

UPDATE #result
SET total_count=t.total_count
FROM #total_count t INNER JOIN #result r
ON t.date=r.date

UPDATE #result
SET category_percent = category_count / convert(decimal(24,3),total_count)
FROM #result
WHERE total_count>0

INSERT INTO #total_category
SELECT shift_category_name,
		sum(category_count)AS 'total_category'
FROM #result
GROUP BY shift_category_name

UPDATE #result
SET sum_total_count=(SELECT SUM(category_count) from #result)

UPDATE #result
SET category_total=total_category,
category_total_percent = total_category / convert(decimal(24,3),sum_total_count)
FROM #total_category t
INNER JOIN #result r ON r.shift_category_name=t.shift_category_name
WHERE sum_total_count>0

SELECT date ,
	shift_category_name ,
	category_count ,
	category_percent ,
	total_count ,
	category_total,
	category_total_percent,
	sum_total_count,
	hide_time_zone  
FROM #result 
ORDER BY date,shift_category_name

GO