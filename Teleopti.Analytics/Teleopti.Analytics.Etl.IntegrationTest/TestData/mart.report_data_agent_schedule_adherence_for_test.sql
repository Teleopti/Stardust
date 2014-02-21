IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_agent_schedule_adherence_for_test]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_agent_schedule_adherence_for_test]
GO
--select * from mart.dim_person
--exec mart.report_data_agent_schedule_adherence_for_test @date_from='2014-02-18 00:00:00',@date_to='2014-02-19 00:00:00',@adherence_id=1,@person_code='80CF5548-C89F-4978-9C56-A2D7016B7943',@time_zone_code=N'W. Europe Standard Time', @report_resource_key ='ResReportAdherencePerDay'
CREATE PROCEDURE [mart].[report_data_agent_schedule_adherence_for_test]
@date_from datetime,
@date_to datetime,
@adherence_id int,
@person_code uniqueidentifier,
@time_zone_code nvarchar(100),
@report_resource_key nvarchar(100)
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
DECLARE @scenario_id int
DECLARE @interval_from int
DECLARE @interval_to int
DECLARE @agent_code uniqueidentifier
DECLARE @activity_set nvarchar(max)
DECLARE @absence_set nvarchar(max)
-----
SET @language_id = 1 --Obsolete parameter
SET @site_id = -2
SET @team_set = -2
SET @sort_by = 1 ---Order By 1=FirstName,2=LastName,3=Shift_start,4=Adherence
SELECT @report_id = id from mart.report where report_name_resource_key=@report_resource_key-- 'D1ADE4AC-284C-4925-AEDD-A193676DBD2F'

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

SET @interval_from=0
SET @interval_to = 96
SET @agent_code = @person_code
SELECT @activity_set = convert(nvarchar(100),activity_id) from mart.dim_activity where activity_name='Phone'
SELECT @activity_set = @activity_set + ','+ convert(nvarchar(100),activity_id) from mart.dim_activity where activity_name='Lunch'

SET @absence_set = ''

select top 1 @scenario_id=scenario_id from mart.dim_scenario where default_scenario=1

/*Run report*/
IF @report_resource_key = 'ResReportAdherencePerDay'
	EXEC mart.report_data_agent_schedule_adherence @date_from, @date_from, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @adherence_id, @sort_by ,@mart_time_zone_id, @person_code, @person_code, @report_id, @language_id, @business_unit_code,0


IF @report_resource_key= 'ResReportScheduledTimePerAgent'
begin
	select  @person_code = UserId from Infratest_analytics.dbo.aspnet_Users where AppLoginName='userthatcreatestestdata'
	insert into mart.permission_report
	select @person_code,t.team_id,0,bu.business_unit_id,1,getdate(),Id
	from mart.report
	inner join mart.dim_team t
            on 1=1
            and t.team_id > -1
	inner join mart.dim_business_unit bu
            on 1=1
            and bu.business_unit_id > -1

	EXEC mart.report_data_scheduled_time_per_agent @scenario_id,@date_from,@date_to,@interval_from,@interval_to,@group_page_code,@group_page_group_set,@group_page_agent_code,@site_id,@team_set,@agent_code,@activity_set,@absence_set,@mart_time_zone_id,@person_code,@report_id,@language_id,@business_unit_code
--select @scenario_id,@date_from,@date_to,@interval_from,@interval_to,@group_page_code,@group_page_group_set,@group_page_agent_code,@site_id,@team_set,@agent_code,@activity_set,@absence_set,@mart_time_zone_id,@person_code,@report_id,@language_id,@business_unit_code
end 
