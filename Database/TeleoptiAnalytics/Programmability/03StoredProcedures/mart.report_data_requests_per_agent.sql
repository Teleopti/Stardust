IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_requests_per_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_requests_per_agent]
GO
-- =============================================
-- Author:		DJ
-- Create date: 2012-01-21
-- Description:	Count number of request and status per agent
------------------------------------------------
-- Change Log
-- Date			Author	Description
------------------------------------------------
-- 2012-02-13	DavidJ	#18135 - Adding missing filter for Agents
-- 2012-02-14	DavidJ	#	- refactor tables
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- 2013-09-17	ErikS	#24754 - Unable to see requests from future personperiod
-- =============================================
--Static dimension table
--------------------------------------------------------
--request_type_id	request_type_name	resource_key
--------------------------------------------------------
--1					Text Request		ResTextRequest
--2					Absence Request		ResAbsenceRequest
--3					Shift Trade Request	ResShiftTradeRequest
--------------------------------------------------------
/*
exec mart.report_data_requests_per_agent @date_from='2012-01-31 00:00:00',@date_to='2012-03-31 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_set=NULL,@site_id=N'0',@team_set=N'7',@agent_set=N'11610fe4-0130-4568-97de-9b5e015b2564',@request_type_id=N'-2',@time_zone_id=N'1',@person_code='6B7DD8B6-F5AD-428F-8934-9B5E015B2B5C',@report_id=27,@language_id=1053,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'
*/
CREATE PROCEDURE [mart].[report_data_requests_per_agent]
@date_from datetime,
@date_to datetime,
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@group_page_agent_set nvarchar(max),
@site_id int,
@team_set nvarchar(max),
@agent_set nvarchar(max),
@time_zone_id int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@request_type_id int,
@business_unit_code uniqueidentifier,
@details int = 0
AS
BEGIN
SET NOCOUNT ON;

--create temp tables
CREATE TABLE  #request_type (request_type_id int)
CREATE TABLE  #rights_agents (right_id int)
CREATE TABLE #rights_teams (right_id int)
CREATE TABLE #selected_agents (selected_id int)
CREATE TABLE #result(
	start_date smalldatetime,
	end_date smalldatetime,
	date_of_request smalldatetime,
	person_id int,
	person_name nvarchar(200),
	request_type_id int,
	request_type_name nvarchar(50),
	request_status_id int,
	request_status_name nvarchar(100),
	request_start_date_count int,
	request_day_count int,
	hide_time_zone bit
	)

--Handle "all" request types from select	ion
IF @request_type_id=-2
	INSERT INTO #request_type
	SELECT request_type_id FROM mart.dim_request_type
ELSE
	INSERT INTO #request_type
	SELECT @request_type_id

--trying to use local time zone for now
DECLARE @hide_time_zone bit
SET @hide_time_zone = 1

--Get all agents/persons that user has permission to see
INSERT INTO #rights_agents 
EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, '00000000-0000-0000-0000-000000000002', @site_id, @team_set, '00000000-0000-0000-0000-000000000002', @person_code, @report_id, @business_unit_code

--Get all teams that user has permission to see
INSERT INTO #rights_teams
SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)

--Check if we are dealing with BusinessHierarchy or Groupings
IF mart.GroupPageCodeIsBusinessHierarchy(@group_page_code) = 0
	BEGIN
		-- Some Group Page was picked
		--Agents = "All"
		IF @group_page_agent_set = '00000000-0000-0000-0000-000000000002'
		BEGIN
			INSERT INTO #selected_agents
			select * from #rights_agents
		END
		ELSE
		BEGIN
		-- Split the person codes
		INSERT INTO #selected_agents
		SELECT * FROM mart.TwolistPersonCodeToIdMultipleTeams(@group_page_agent_set, @date_from, @date_to, @site_id, @team_set)
		END
	END
ELSE
	BEGIN
		-- Business Hierarchy picked
		-- Split the person codes
		INSERT INTO #selected_agents
		SELECT * FROM mart.TwolistPersonCodeToIdMultipleTeams(@agent_set, @date_from, @date_to, @site_id, @team_set)
	END

--------------
--Request
--------------
INSERT INTO #result
SELECT
	start_date				= f.request_startdate,
	end_date                = f.request_enddate,
	date_of_request		    = f.application_datetime,
	person_id				= p.person_id,
	person_name				= p.person_name,
	request_type_id			= rt.request_type_id,
	request_type_name		= ISNULL(typeTranslations.term_language, rt.request_type_name),
	request_status_id		= rs.request_status_id,
	request_status_name		= ISNULL(statusTranslations.term_language, rs.request_status_name),
	request_start_date_count= (f.request_start_date_count),
	request_day_count		= (f.request_day_count),
	hide_time_zone			= @hide_time_zone
FROM mart.fact_request f
inner join mart.dim_date d
	on f.request_start_date_id = d.date_id
inner join mart.dim_person p WITH (NOLOCK)
	on p.person_id = f.person_id
	AND f.request_start_date_id BETWEEN p.valid_from_date_id_local AND p.valid_to_date_id_local
inner join #selected_agents s
	on p.person_id = s.selected_id	
inner join mart.dim_request_status rs
	on rs.request_status_id = f.request_status_id
inner join 	mart.dim_request_type rt
	on rt.request_type_id = f.request_type_id
left outer join mart.language_translation statusTranslations
	on statusTranslations.ResourceKey = rs.resource_key collate database_default
	and statusTranslations.language_id=@language_id
left outer join mart.language_translation typeTranslations
	on typeTranslations.ResourceKey = rt.resource_key collate database_default
	and typeTranslations.language_id=@language_id
WHERE	rt.request_type_id in (SELECT request_type_id FROM #request_type)
AND d.date_date between @date_from AND @date_to
AND p.team_id IN(select right_id from #rights_teams)
AND p.person_id IN (SELECT right_id FROM #rights_agents)

/*GROUP BY
	d.date_date,
	p.person_id,
	p.person_name,
	rt.request_type_id,
	ISNULL(typeTranslations.term_language, rt.request_type_name),
	rs.request_status_id,
	ISNULL(statusTranslations.term_language, rs.request_status_name)*/
order by
	p.person_id,
	d.date_date
	
	
SELECT * FROM #result	

END
GO