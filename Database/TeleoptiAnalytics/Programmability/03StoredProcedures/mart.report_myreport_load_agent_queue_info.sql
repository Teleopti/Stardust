/****** Object:  StoredProcedure [mart].[report_myreport_load_agent_queue_info]    Script Date: 28/11/2008 13:45:12 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_myreport_load_agent_queue_info]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_myreport_load_agent_queue_info]
GO
/****** Object:  StoredProcedure [mart].[report_myreport_load_agent_queue_info]    Script Date: 28/11/2008 13:45:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Virajs
-- Create date: 2008-11-28
-- Update date:2009-02-11 Added new mart schema KJ
-- =============================================

CREATE procedure [mart].[report_myreport_load_agent_queue_info]
@personCode uniqueidentifier,
@startDate smalldatetime,
@endDate smalldatetime
as
set nocount on

select dd.date_id, dd.date_date into #dim_date from mart.dim_date dd 
where dd.date_date between @startDate and @endDate 
order by dd.date_date asc

select dp.person_id into #person from mart.dim_person dp WITH (NOLOCK)
where dp.person_code = @personCode 
and convert(DATETIME, dp.valid_from_date) >= @startDate 
and convert(DATETIME, dp.valid_to_date) <= @endDate

select dq.queue_name, dq.queue_id into #dim_queue from mart.dim_queue dq

select 
(select dq.queue_name from #dim_queue dq where dq.queue_id = fq.queue_id) as QueueName,
sum(fq.answered_calls) as AnsweredContracts,
avg(fq.talk_time_s)/60 as AverageTalkTime,
sum(fq.after_call_work_time_s)/60/60 as AfterWorkContactTime,
sum(fq.talk_time_s)/60/60 as TotalHandlingTime
from mart.fact_agent_queue fq
where fq.date_id in (select dd.date_id from #dim_date dd)
and fq.acd_login_id in (select balp.acd_login_id 
from mart.bridge_acd_login_person balp 
where balp.person_id in (select dp.person_id from #person dp))
group by fq.queue_id

