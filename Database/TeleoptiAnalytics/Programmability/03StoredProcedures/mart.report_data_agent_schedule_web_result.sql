IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_agent_schedule_web_result]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_agent_schedule_web_result]
GO

/*
exec mart.report_data_agent_schedule_web_result 
@date_from='2000-02-05 00:00:00',
@date_to='2023-02-07 00:00:00',
@adherence_id=N'1',
@time_zone_id=N'1',
@person_code='59819d2d-dcbf-4618-a0bf-a2ba00eb325b',
@business_unit_code='89a4bde4-5002-48b7-aca9-ef3d5ece5ad2'


exec sp_executesql N'exec mart.report_data_agent_schedule_web_result @date_from=@p0, @date_to=@p1, @time_zone_id=@p2, @person_code=@p3, @adherence_id=@p4, @business_unit_code=@p5',N'@p0 datetime,@p1 datetime,@p2 int,@p3 uniqueidentifier,@p4 int,@p5 uniqueidentifier',@p0='2000-01-01 00:00:00',@p1='2020-01-01 00:00:00',@p2=1,@p3='9FDB1B61-5CCE-4A83-A344-A2BA00EC27D7',@p4=1,@p5='52A6A39C-C721-4696-956B-0CC4F2E30383'
*/

CREATE PROCEDURE [mart].[report_data_agent_schedule_web_result] 
@date_from datetime,
@date_to datetime,
@adherence_id int,
@time_zone_id int,
@person_code uniqueidentifier,
@business_unit_code uniqueidentifier
as
set nocount on	

declare @interval_from int
declare @interval_to int
set @interval_from=0
select @interval_to=max(interval_id) from mart.dim_interval

create table #result 
(
	date smalldatetime,
	person_code uniqueidentifier,
	person_name nvarchar(200),
	scheduled_ready_time smallint,
	ready_time_s smallint,
	ready_time_per_scheduled_ready_time decimal(20,2),
	answered_calls smallint,
	avg_answered_calls decimal(20,2),
	avg_answered_calls_ready_hour decimal(20,2),
	occupancy decimal(20,2),
	adherence_calc_s smallint,
	adherence decimal(20,2),
	deviation_s smallint,
	handling_time_s smallint,
	after_call_work_time_s smallint,
	talk_time_s smallint
)

insert into #result
exec mart.report_data_agent_schedule_result 
@date_from=@date_from,
@date_to=@date_to,
@interval_from=@interval_from,
@interval_to=@interval_to,
@group_page_code=null,
@group_page_group_set=NULL,
@group_page_agent_set=NULL,
@site_id=null,
@team_set=null,
@agent_set=null,
@adherence_id=@adherence_id,
@time_zone_id=@time_zone_id,
@person_code=@person_code,
@report_id=null,
@language_id=null,
@business_unit_code=@business_unit_code, 
@from_matrix=0

select answered_calls as AnsweredCalls,
	after_call_work_time_s/answered_calls as AfterCallWorkTime
from #result

drop table #result

go