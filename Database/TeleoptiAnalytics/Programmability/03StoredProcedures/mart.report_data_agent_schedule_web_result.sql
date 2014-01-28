IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_agent_schedule_web_result]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_agent_schedule_web_result]
GO

CREATE PROCEDURE [mart].[report_data_agent_schedule_web_result] 
@date_from datetime,
@date_to datetime,
@adherence_id int,
@person_code uniqueidentifier,
@business_unit_code uniqueidentifier
as
set nocount on

declare @interval_from int
declare @interval_to int
declare @time_zone_id int
set @interval_from=0
select @interval_to=max(interval_id) from mart.dim_interval
select @time_zone_id = time_zone_id from mart.dim_person where person_code=@person_code

CREATE TABLE #tmpResult
	(
	date smalldatetime,
	person_code uniqueidentifier,
	person_name nvarchar(200),
	scheduled_ready_time decimal(20,2),
	ready_time_s int,
	ready_time_per_scheduled_ready_time decimal(20,2),
	answered_calls int DEFAULT 0,
	avg_answered_calls decimal(20,2),
	avg_answered_calls_ready_hour decimal(20,2),
	occupancy decimal(20,2),
	adherence_calc_s decimal(20,2),
	adherence decimal(20,2),
	deviation_s decimal(20,2),
	handling_time_s decimal(20,2),
	after_call_work_time_s decimal(20,2),
	talk_time_s  decimal(20,2)
	)

INSERT INTO  #tmpResult 
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
	case
		when answered_calls = 0 then 0
		else after_call_work_time_s/answered_calls
	end as AfterCallWorkTime,
	case
		when answered_calls = 0 then 0
		else talk_time_s/answered_calls
	end as TalkTime,
	case
		when answered_calls = 0 then 0
		else (after_call_work_time_s + talk_time_s) / answered_calls	
	end as HandlingTime,
	ready_time_per_scheduled_ready_time as ReadyTimePerScheduledReadyTime,
	adherence as Adherence
from #tmpResult

DROP TABLE #tmpResult

go