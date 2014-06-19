IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_agent_stat_raw]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_agent_stat_raw]
GO


-- =============================================
-- Author:		KJ
-- Create date: 2008-08-06
-- Last Update date:2009-01-07
--					20090107 Bug fix correct person_ids returned when ALL selected KJ 
--					20081006 Bug fix selections for team and agents KJ
--					20090211 Added new mart schema KJ
--					2009-03-02 Excluded timezone UTC from time_zone check KJ
--					2011-01-24 Use agent_code instead of agent_id ME
--					2012-01-09 Passed BU to ReportAgents ME
--					2012-01-26 Change parameters @group_page_group_id and @team_id to sets and nvarchar(max)
--					2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- Description:	Used by report Agent Statistics -  Raw
-- =============================================
--exec report_data_agent_stat_raw @date_from='2006-01-01 00:00:00:000',@date_to='2006-01-03 00:00:00:000',@interval_from=N'0',@interval_to=N'203',@site_id=N'-2',@team_set=N'-2',@agent_set=N'-1',@time_zone_id=N'37',@person_code='F376CA48-27D2-467F-940E-9AF600171D0F',@report_id=16,@language_id=1053

CREATE PROCEDURE [mart].[report_data_agent_stat_raw] 
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

--DECLARE @time_zone_id INT
--SET @time_zone_id=81 --OBS! HÃ…RDKODAT

/*SNABBA UPP PROCEDUR, HÃ„MTA RÃ„TT TIMEZONE FÃ–RST OCH JOINA PÃ… TEMPTABELL SEDAN*/
--SELECT * INTO #bridge_time_zone FROM mart.bridge_time_zone WHERE time_zone_id=@time_zone_id


-- Convert agent_code to agent_id
CREATE TABLE #agent_ids(id int)
IF (mart.GroupPageCodeIsBusinessHierarchy(@group_page_code) = 0)
BEGIN
	INSERT INTO #agent_ids
	SELECT * FROM mart.PersonCodeToId(@group_page_agent_code, @date_from, @date_to, @site_id, @team_set)
END
ELSE
BEGIN
	INSERT INTO #agent_ids
	SELECT * FROM mart.PersonCodeToId(@agent_code, @date_from, @date_to, @site_id, @team_set)
END

/* Get relevant agents */
CREATE TABLE #rights_agents (right_id int)

INSERT INTO #rights_agents
EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_code, @person_code, @report_id, @business_unit_code


/*Get all teams that user has permission to see. */
CREATE TABLE #rights_teams (right_id int)
           INSERT INTO #rights_teams SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)


/* If agent selected is "Not Defined" then add -1 to #rights_agents
if @agent_id=-1
	insert into #rights_agents
		select -1*/

/* Check if time zone will be hidden (if only one exist then hide) */
DECLARE @hide_time_zone bit
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0


SELECT	DISTINCT
		d.date_date,
		i.interval_name,
		p.person_code,
		isnull(l.term_language,p.person_name) person_name,
		da.acd_login_id,
		da.acd_login_name,
		convert(decimal(18,2),ready_time_s)ready_time_s, 
		convert(decimal(18,2),logged_in_time_s)logged_in_time_s, 
		convert(decimal(18,2),not_ready_time_s)not_ready_time_s, 
		convert(decimal(18,2),idle_time_s)idle_time_s,
		convert(int,direct_outbound_calls)direct_outbound_calls,		
		convert(decimal(18,2),direct_outbound_talk_time_s)direct_outbound_calls_talk_time_s,	
		convert(int,direct_incoming_calls)direct_incoming_calls,	
		convert(decimal(18,2),direct_incoming_calls_talk_time_s)direct_incoming_calls_talk_time_s,	
		convert(decimal(18,2),admin_time_s)admin_time_s,
		@hide_time_zone as hide_time_zone
FROM mart.fact_agent fa
INNER JOIN mart.dim_acd_login da ON
	fa.acd_login_id=da.acd_login_id
INNER JOIN mart.bridge_acd_login_person bap
	ON bap.acd_login_id=fa.acd_login_id
INNER JOIN mart.dim_person p ON
	bap.person_id=p.person_id
INNER JOIN mart.bridge_time_zone b
	ON	fa.interval_id= b.interval_id
	AND fa.date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
LEFT JOIN mart.language_translation l
	ON	l.term_english = p.person_name AND	l.language_id = @language_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND i.interval_id BETWEEN @interval_from AND  @interval_to
AND b.time_zone_id = @time_zone_id
AND (p.team_id IN (select right_id from #rights_teams)OR p.team_id=-1)
--AND (p.person_id in (SELECT right_id FROM #rights_agents) OR p.person_id=-1) 
AND (p.person_id in (SELECT right_id FROM #rights_agents)) 
AND (@date_from	between p.valid_from_date AND p.valid_to_date OR  @date_to	between p.valid_from_date AND p.valid_to_date)
ORDER BY d.date_date,i.interval_name,isnull(l.term_language,p.person_name),p.person_code,da.acd_login_name,da.acd_login_id


GO

