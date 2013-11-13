IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_fact_agent_state_merge]'))
DROP VIEW [mart].[v_fact_agent_state_merge]
GO
CREATE VIEW mart.v_fact_agent_state_merge
AS
SELECT 
date_id			= d.date_id,
person_id		= ISNULL(dp.person_id,-1),
interval_id		= i.interval_id,
state_group_id	= sg.state_group_id,
time_in_state_s	= sum(stg.time_in_state_s),
datasource_id	= 1,
insert_date		= cast(getdate() as smalldatetime)
FROM [stage].[stg_agent_state] stg WITH(TABLOCKX) --Lock the table for trigger insert during our data load and later truncate
LEFT JOIN mart.dim_person dp
	ON stg.person_code = dp.person_code
	AND --trim
		(
			(stg.[date]+stg.interval >= dp.valid_from_date)
		AND
			(stg.[date]+stg.interval < dp.valid_to_date)
		)
INNER JOIN mart.dim_state_group sg
	ON sg.state_group_code = stg.state_group_code
INNER JOIN mart.dim_date d
	ON stg.[date] = d.date_date
INNER JOIN mart.dim_interval i
	ON stg.interval BETWEEN i.interval_start AND i.interval_end
GROUP BY date_id,person_id,interval_id,state_group_id
GO