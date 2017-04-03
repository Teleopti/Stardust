/****** Object:  StoredProcedure [mart].[report_myreport_load_agent_login_info]    Script Date: 28/11/2008 13:45:12 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_myreport_load_agent_login_info]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_myreport_load_agent_login_info]
GO
/****** Object:  StoredProcedure [mart].[report_myreport_load_agent_login_info]    Script Date: 28/11/2008 13:45:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Virajs
-- Create date: 2008-11-28
-- Update date:2009-02-11 Added new mart schema KJ
-- =============================================

CREATE procedure [mart].[report_myreport_load_agent_login_info]
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

select a.date_date as LogOnDate, 
b.scheduled_ready_time_m as CtiTime, 
a.looged_in_time_s as LoggedInTime, 
a.idle_time_s as IdleTime, 
a.ready_time_s as AvailableTime
from
(select dd.date_date,
isnull(sum(dd.looged_in_time_s)/3600, 0) as looged_in_time_s,
isnull(sum(dd.idle_time_s)/3600, 0) as idle_time_s,
isnull(sum(dd.ready_time_s)/3600, 0) as ready_time_s
from
(select
dd.date_date, 
isnull((fa.logged_in_time_s), 0) as looged_in_time_s, 
isnull((fa.idle_time_s), 0) as idle_time_s,
isnull((fa.ready_time_s), 0) as ready_time_s
from #dim_date dd left outer join mart.fact_agent fa
on dd.date_id = fa.date_id
and fa.acd_login_id = (select balp.acd_login_id 
from mart.bridge_acd_login_person balp 
where balp.person_id in (select dp.person_id from #person dp))) dd
group by dd.date_date) a,
(select dd.date_date,
isnull(sum(dd.scheduled_ready_time_m)/60, 0) as scheduled_ready_time_m
from
(select dd.date_date, 
isnull((fa.scheduled_ready_time_m), 0) as scheduled_ready_time_m
from #dim_date dd left outer join mart.fact_schedule fa WITH (NOLOCK)
on dd.date_id = fa.schedule_date_id
and fa.person_id in (select dp.person_id from #person dp)) dd
group by dd.date_date) b
where a.date_date = b.date_date