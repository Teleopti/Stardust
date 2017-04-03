IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_adherence_report_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_adherence_report_load]
GO
-- =============================================
-- Author:		ZT
-- Update date: 2009-08-21
-- Description:	Loader of adherence data for SDK and MyTime
-- =============================================
-- Update log
-- Date			Autor	Description
-- =============================================
-- 2009-09-16	DJ		Re-factoring of permission in sub-SP DJ
-- 2010-01-26	DJ		Item #9263
-- 2010-09-30	DJ		Inteface changed on Sub-SP
-- 2011-01-07	RobinK	Removed scenario as it was removed as argument in sub-SP
-- 2011-01-25	ME		Use @agent_person_code instead of @agent_person_id
-- 2011-02-24	DJ		Fixing new inteface for sub-Sp
-- =============================================
CREATE PROCEDURE [mart].[raptor_adherence_report_load]

@date_from datetime,
@time_zone_id nvarchar(100),
@person_code uniqueidentifier,
@agent_person_code uniqueidentifier,
@language_id int,
@adherence_id int

AS

DECLARE @site_id int
DECLARE @team_set nvarchar(max)
DECLARE @sort_by int
DECLARE @report_id uniqueidentifier
DECLARE @mart_time_zone_id int
DECLARE @agent_person_id int
DECLARE @business_unit_id int
DECLARE @group_page_code uniqueidentifier
DECLARE @group_page_group_set nvarchar(max)
DECLARE @group_page_agent_code uniqueidentifier
DECLARE @business_unit_code uniqueidentifier
-----


SET @site_id = -2
SET @team_set = -2
SET @sort_by = 1 ---sortera p† 1=F”rnamn,2=Efternamn,3=Shift_start,4=Adherence
SET @report_id = 'D1ADE4AC-284C-4925-AEDD-A193676DBD2F'

--Groupings, not used from MyTime
SET @group_page_code = NULL
SET @group_page_group_set = NULL
SET @group_page_agent_code = NULL

--Get the agent_id and team_id for this PersonPeriod
--Note: Currently use Top 1 just in case we have overlapping personperiods !!! 
SELECT TOP 1 @agent_person_id = person_id,@team_set=team_id, @business_unit_id = business_unit_id,@business_unit_code =business_unit_code  FROM mart.dim_person WITH (NOLOCK)
WHERE person_code = @agent_person_code
AND @date_from BETWEEN mart.dim_person.valid_from_date_local AND mart.dim_person.valid_to_date_local
ORDER BY insert_date desc

SET @mart_time_zone_id = (SELECT time_zone_id FROM mart.dim_time_zone
WHERE time_zone_code = @time_zone_id)

EXEC mart.report_data_agent_schedule_adherence @date_from, @date_from, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @adherence_id, @sort_by ,@mart_time_zone_id, @person_code, @agent_person_code, @report_id, @language_id, @business_unit_code,0