IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_number_of_calls_per_agent_by_date]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_number_of_calls_per_agent_by_date]
GO

-- ======================================================================================================
-- Author:		DJ/ES
-- Create date: 2014-07-14
-- Updated by: Jianguang/Hongzhao
-- Update date:  2015-03-11
-- Description:	Gets the agent whose answered calls over @threshold calls during @Date for specify @time_zone_id
-- Update description: add a parameter when selecting number of calls to restrict the selecion within business_unit_code parameter
-- Example: EXEC [mart].[raptor_number_of_calls_per_agent_by_date] @time_zone_code='UTC', @threshold=10, @local_date='2014-07-10', @business_unit_code = '48C43F5E-EB5A-485B-82FF-A1B000B75497'
-- ======================================================================================================

CREATE PROCEDURE [mart].[raptor_number_of_calls_per_agent_by_date] 
@threshold int,
@time_zone_code nvarchar(50),
@local_date smalldatetime, 
@business_unit_code uniqueidentifier
AS
Begin
select
	p.person_code,
	d.date_date,
	sum(answered_calls) as 'answered_call'
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
	on t.time_zone_id = p.time_zone_id
where p.business_unit_code = @business_unit_code and d.date_date = @local_date
  and t.time_zone_code = @time_zone_code
group by
	p.person_code,
	d.date_date
having sum(answered_calls) > @threshold
End

GO