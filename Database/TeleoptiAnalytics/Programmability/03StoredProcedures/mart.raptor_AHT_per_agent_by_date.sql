IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_AHT_per_agent_by_date]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_AHT_per_agent_by_date]
GO

-- ======================================================================================================
-- Author:      Xinfeng
-- Create date: 2014-07-18
-- Description: Gets the agent whose AHT under @threshold during @Date for specify @time_zone_id
-- Example: EXEC [mart].[raptor_AHT_per_agent_by_date] @time_zone_code='W. Europe Standard Time', @threshold=1800, @local_date='2014-02-10'
-- ======================================================================================================

CREATE PROCEDURE [mart].[raptor_AHT_per_agent_by_date] 
@threshold int,
@time_zone_code nvarchar(50),
@local_date smalldatetime
AS
Begin
select p.person_code,
       d.date_date,
       convert(decimal(18,2),((sum(talk_time_s + after_call_work_time_s))/case when sum(answered_calls)= 0 THEN 1 ELSE sum(answered_calls) END)) as AHT
  from mart.fact_agent_queue f
 inner join mart.bridge_acd_login_person b
    on f.acd_login_id = b.acd_login_id
 inner join mart.dim_person p
    on p.person_id = b.person_id
 inner join mart.bridge_time_zone tz
    on tz.time_zone_id = p.time_zone_id
   and f.date_id = tz.date_id
   and f.interval_id = tz.interval_id 
 inner join mart.dim_date d
    on tz.local_date_id = d.date_id
 inner join mart.dim_time_zone t
	on p.time_zone_id = t.time_zone_id
 where d.date_date = @local_date
   and t.time_zone_code = @time_zone_code
 group by p.person_code,
       d.date_date
having convert(decimal(18,2),((sum(talk_time_s + after_call_work_time_s))/case when sum(answered_calls)= 0 THEN 1 ELSE sum(answered_calls) END)) < @threshold
End
GO
