IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_agent_schedule_adherence_for_test]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_agent_schedule_adherence_for_test]
GO
--select * from mart.dim_person
--exec mart.report_data_agent_schedule_adherence_for_test @date_from='2014-02-18 00:00:00',@date_to='2014-02-19 00:00:00',@adherence_id=1,@person_code='80CF5548-C89F-4978-9C56-A2D7016B7943',@time_zone_code=N'W. Europe Standard Time'
CREATE PROCEDURE [mart].[report_data_agent_schedule_adherence_for_test]
@date_from datetime,
@date_to datetime,
@adherence_id int,
@person_code uniqueidentifier,
@time_zone_code nvarchar(100)

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
DECLARE @language_id int
-----
SET @language_id = 1 --Obsolete parameter
SET @site_id = -2
SET @team_set = -2
SET @sort_by = 1 ---Order By 1=FirstName,2=LastName,3=Shift_start,4=Adherence
SET @report_id = 'D1ADE4AC-284C-4925-AEDD-A193676DBD2F'

--Groupings, not used from test
SET @group_page_code = NULL
SET @group_page_group_set = NULL
SET @group_page_agent_code = NULL

--Get the agent_id and team_id for this PersonPeriod
--Note: Currently use Top 1 just in case we have overlapping personperiods !!! 
SELECT TOP 1 @agent_person_id = person_id,@team_set=team_id, @business_unit_id = business_unit_id,@business_unit_code =business_unit_code  FROM mart.dim_person
WHERE person_code = @person_code
AND (
	@date_from BETWEEN mart.dim_person.valid_from_date AND mart.dim_person.valid_to_date
	OR
	@date_to BETWEEN mart.dim_person.valid_from_date AND mart.dim_person.valid_to_date
	)
ORDER BY insert_date desc

SET @mart_time_zone_id = (SELECT time_zone_id FROM mart.dim_time_zone
WHERE time_zone_code = @time_zone_code)

EXEC mart.report_data_agent_schedule_adherence @date_from, @date_from, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @adherence_id, @sort_by ,@mart_time_zone_id, @person_code, @person_code, @report_id, @language_id, @business_unit_code,0