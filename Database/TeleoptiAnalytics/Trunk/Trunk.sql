

----------------  
--Name: David Jonsson
--Date: 2014-07-01
--Desc: Bug #27933 - Give the end user a posibility to cherrypick tables for update_stat
----------------
SET NOCOUNT OFF
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_updatestat_tables]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[sys_updatestat_tables](
	table_schema sysname NOT NULL,
	table_name sysname NOT NULL,
	options NVARCHAR(200) NULL
	CONSTRAINT [PK_sys_updatestat_tables] PRIMARY KEY CLUSTERED 
	(
		table_schema ASC,
		table_name ASC
	)
)
END

GO
IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'sqlserver_updatestat' AND jobstep_id=87)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(87,N'sqlserver_updatestat')
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_number_of_calls_per_agent_by_date]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_contract_load]
GO

-- =============================================
-- Author:		DJ/ES
-- Create date: 2014-07-14
-- Description:	Gets the agent who answer more then @threshold calls during @Date
-- =============================================
--EXEC [mart].[raptor_number_of_calls_per_agent_by_date] @threshold=10,@local_date='2014-07-10'
CREATE PROCEDURE [mart].[raptor_number_of_calls_per_agent_by_date] 
@threshold int,
@local_date smalldatetime
	
AS

select
	count(answered_calls) as 'answered_call',
	p.person_code,
	d.date_date
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
where d.date_date = @local_date
group by
	p.person_code,
	d.date_date
having count(answered_calls) > @threshold
GO
