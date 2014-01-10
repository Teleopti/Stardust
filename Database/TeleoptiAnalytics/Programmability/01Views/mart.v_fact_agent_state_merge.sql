IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_fact_agent_state_merge]'))
DROP VIEW [mart].[v_fact_agent_state_merge]
GO
--SELECT * FROM mart.v_fact_agent_state_merge
CREATE VIEW mart.v_fact_agent_state_merge
AS
SELECT 
date_id			= btz.local_date_id,
person_id		= ISNULL(dp.person_id,-1),
state_group_id	= sg.state_group_id,
time_in_state_s	= sum(stg.time_in_state_s),
datasource_id	= 1,
insert_date		= cast(getdate() as smalldatetime)
FROM [stage].[stg_agent_state] stg WITH(TABLOCKX) --Lock the table for trigger insert during our data load and later truncate
LEFT JOIN mart.dim_person dp
	ON stg.person_code = dp.person_code
	AND --trim
		(
			(cast(stg.[StateStart] as smalldatetime) >= dp.valid_from_date)
		AND
			(cast(stg.StateStart as smalldatetime) < dp.valid_to_date)
		)
INNER JOIN mart.dim_state_group sg
	ON sg.state_group_code = stg.state_group_code
INNER JOIN mart.dim_date d
	ON cast(DATEADD(dd, DATEDIFF(dd, 0, stg.StateStart), 0) as smalldatetime) = d.date_date
INNER JOIN mart.dim_interval i
	ON cast(dateadd(MINUTE,(DATEPART(HOUR,stg.StateStart)*60+DATEPART(MINUTE,stg.StateStart)),'1900-01-01') as smalldatetime)=i.interval_start
INNER JOIN mart.bridge_time_zone btz
	ON btz.date_id = d.date_id
	AND btz.interval_id = i.interval_id
	AND dp.time_zone_id = btz.time_zone_id
GROUP BY
	btz.local_date_id,
	person_id,
	state_group_id
GO