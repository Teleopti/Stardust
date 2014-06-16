
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_agent_queue_metrics_web_metrics]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_agent_queue_metrics_web_metrics]
GO

-- =============================================
-- Author:		Team Orange
-- Create date: 2014-06-05
-- Last Update date:
-- Description:	Used by Agent Queue Metrics in My Report on web
-- =============================================
CREATE PROCEDURE [mart].[report_data_agent_queue_metrics_web_metrics] 
@date_from datetime,
@person_code uniqueidentifier,
@business_unit_code uniqueidentifier
as
set nocount on

declare @time_zone_id int
select @time_zone_id = time_zone_id from mart.dim_person where person_code=@person_code

declare @interval_from int
declare @interval_to int
set @interval_from=0
select @interval_to=max(interval_id) from mart.dim_interval

CREATE TABLE #tempResult (
person_code uniqueidentifier,
person_name nvarchar(200),
queue_name nvarchar(200),
date_date date,
answered_calls int,
average_handling_time_s decimal(18,2),
average_talk_time_s decimal(18,2),
average_after_call_work_s decimal(18,2),
hide_time_zone bit,
handling_time_s int,
talk_time_s int,
after_call_work_s int
);

Insert into #tempResult
exec mart.report_data_agent_queue_metrics
@date_from=@date_from,
@date_to=@date_from,
@interval_from=@interval_from,
@interval_to=@interval_to,
@group_page_code=NULL,
@group_page_group_set=NULL,
@group_page_agent_set=NULL,
@site_id=NULL,
@team_set=NULL,
@agent_set=NULL,
@time_zone_id=@time_zone_id,
@person_code=@person_code,
@report_id=NULL,
@language_id=NULL,
@business_unit_code=@business_unit_code, 
@from_matrix=0

SELECT person_name as Person,
	queue_name AS Queue,
	answered_calls AS AnsweredCalls,
	average_after_call_work_s AS AverageAfterCallWork,
	average_handling_time_s AS AverageHandlingTime,
	average_talk_time_s AverageTalkTime
 from #tempResult;

drop table #tempResult