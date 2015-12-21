/****** Object:  StoredProcedure [mart].[report_data_agent_queue_stat_raw]    Script Date: 10/14/2008 14:06:44 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_agent_queue_stat_raw]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_agent_queue_stat_raw]
GO
/****** Object:  StoredProcedure [mart].[report_data_agent_queue_stat_raw]    Script Date: 10/14/2008 14:06:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		KJ
-- Create date: 2008-08-06
-- Last Update date:2008-10-14
--					2008-10-14 Fixed bug with duplicate rows KJ 
--					20081006 Bug fix selections for team and agents KJ
--					20090211 Added new mart schema KJ
--					20090302 Excluded timezone UTC from time_zone check KJ
--					20090701 Added bugfix for person Not Defined KJ
--					2011-01-24 Use agent_code instead of agent_id
--					2011-10-24 Change paramaters @group_page_group_id and @teamd_id to 
--					@group_page_group_set and @team_set
--					2012-01-09 Pass BU to report_get_AgentsMultipleTeams
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- Description:	Used by report Agent Queue Statistics -  Raw
-- =============================================
-- exec mart.report_data_agent_queue_stat_raw @skill_set=N'-1,2,0,3,5,8,6,7,9,10,12',@workload_set=N'-1,0,1,6,7,2,8,4,11',@date_from='2009-02-02 00:00:00',@date_to='2009-02-08 00:00:00',@interval_from=N'0',@interval_to=N'95',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_id=NULL,@group_page_agent_code=NULL,@site_id=N'1',@team_id=N'5',@agent_code=N'00000000-0000-0000-0000-000000000002',@time_zone_id=N'0',@person_code='10957AD5-5489-48E0-959A-9B5E015B2B5C',@report_id=15,@language_id=1053,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'

CREATE PROCEDURE [mart].[report_data_agent_queue_stat_raw] 
@skill_set nvarchar(max),
@workload_set nvarchar(max),
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@group_page_agent_code uniqueidentifier,
@site_id int,
@team_set nvarchar(max),
@agent_code uniqueidentifier,
@date_from datetime,
@date_to datetime,
@interval_from int,--mellan vilka tider
@interval_to int,
@time_zone_id int,
@person_code uniqueidentifier ,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
SET NOCOUNT ON

CREATE TABLE #rights_agents (right_id int)
CREATE TABLE #rights_teams (right_id int)
CREATE TABLE #skills(id int)
CREATE TABLE #workloads(id int)

/*SNABBA UPP PROCEDUR, HÄMTA RÄTT TIMEZONE FÖRST OCH JOINA PÅ TEMPTABELL SEDAN*/
--SELECT * 
--INTO #bridge_time_zone
--FROM mart.bridge_time_zone
--WHERE time_zone_id=@time_zone_id


/*Get select intervals text*/
DECLARE @selected_start_interval nvarchar(50)
DECLARE @selected_end_interval nvarchar(50)
SET @selected_start_interval=(SELECT left(i.interval_name,5) FROM mart.dim_interval i where interval_id= @interval_from)
--select @selected_start_interval
SET @selected_end_interval=(SELECT right(i.interval_name,5) FROM mart.dim_interval i where interval_id= @interval_to)
--select @selected_end_interval


/* Get relevant agents */
INSERT INTO #rights_agents
	EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_code, @person_code, @report_id, @business_unit_code

-- If agent selected is "Not Defined" then add -1 to #rights_agents
if @agent_code=N'00000000-0000-0000-0000-000000000001' OR @team_set=N'-1'
	insert into #rights_agents
		select -1

/*Get all teams that user has permission to see. */
INSERT INTO #rights_teams 
	SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)

INSERT INTO #skills
	SELECT * FROM mart.SplitStringInt(@skill_set)

INSERT INTO #workloads
	SELECT * FROM mart.SplitStringInt(@workload_set)

/* Check if time zone will be hidden (if only one exist then hide) */
DECLARE @hide_time_zone bit
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0

SELECT DISTINCT 
		d.date_date,
		i.interval_name,
		dq.queue_name,
		isnull(l.term_language,dp.person_name)person_name,
		da.acd_login_id,
		da.acd_login_name,
		convert(int,answered_calls)answered_calls, 
		convert(int,transfered_calls)transferred_calls,
		convert(decimal(18,2),talk_time_s)talk_time_s, 
		convert(decimal(18,2),after_call_work_time_s)after_call_work_s,
		@hide_time_zone as hide_time_zone
FROM mart.fact_agent_queue fq
INNER JOIN mart.dim_acd_login da ON
	fq.acd_login_id=da.acd_login_id
INNER JOIN mart.bridge_acd_login_person bap
	ON bap.acd_login_id=da.acd_login_id
INNER JOIN mart.dim_person dp
	ON dp.person_id=bap.person_id
INNER JOIN mart.dim_queue dq 
ON dq.queue_id=fq.queue_id
INNER JOIN mart.bridge_queue_workload bqw
	ON bqw.queue_id=dq.queue_id
INNER JOIN mart.bridge_time_zone b
	ON	fq.interval_id= b.interval_id
	AND fq.date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
INNER JOIN mart.dim_interval i
ON b.local_interval_id = i.interval_id
LEFT JOIN
	mart.language_translation l
ON
	l.term_english = dp.person_name AND
	l.language_id = @language_id	
WHERE d.date_date BETWEEN @date_from AND @date_to
AND i.interval_id BETWEEN @interval_from AND  @interval_to
AND b.time_zone_id = @time_zone_id
AND bqw.skill_id IN (select id from #skills)
AND bqw.workload_id IN (SELECT id from #workloads)
AND (dp.team_id IN (select right_id from #rights_teams)OR dp.team_id=-1)
AND dp.person_id in (SELECT right_id FROM #rights_agents) 
ORDER BY d.date_date,i.interval_name,dq.queue_name,person_name,da.acd_login_name

GO