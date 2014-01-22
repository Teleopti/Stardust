IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_agent_schedule_web_result]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_agent_schedule_web_result]
GO

/*
exec mart.report_data_agent_schedule_web_result 
@date_from='2013-02-05 00:00:00',
@date_to='2013-02-06 00:00:00',
@adherence_id=N'1',
@time_zone_id=N'2',
@person_code='11610fe4-0130-4568-97de-9b5e015b2564',
@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'
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
	after_call_work_time_s/answered_calls as AfterCallWorkTime,
	talk_time_s /answered_calls as TalkTime,
	(after_call_work_time_s + talk_time_s) / answered_calls as HandlingTime,
	 ready_time_per_scheduled_ready_time * 100 as ReadyTimePerScheduledReadyTime
from #tmpResult

DROP TABLE #tmpResult

go