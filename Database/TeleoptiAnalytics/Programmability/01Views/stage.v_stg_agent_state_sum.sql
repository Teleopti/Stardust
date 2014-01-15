IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[stage].[v_stg_agent_state_sum]'))
DROP VIEW [stage].[v_stg_agent_state_sum]
GO
--SELECT * FROM stage.v_stg_agent_state_sum;SELECT * FROM stage.stg_agent_state
--set statistics time on
CREATE VIEW stage.v_stg_agent_state_sum
AS
SELECT 
date_id			= ISNULL(btz.local_date_id,-1),
person_id		= ISNULL(dp.person_id,-1),
state_group_id	= ISNULL(sg.state_group_id,-1),
time_in_state_s	= sum(datediff(ss,stg.StateStart,stg.StateEnd)),
datasource_id	= 1,
insert_date		= cast(getdate() as smalldatetime)
FROM (
	SELECT person_code, state_group_name, state_group_code, StateStart, StateEnd FROM [stage].[stg_agent_state] WITH (TABLOCKX)
	WHERE DATEDIFF(dd, 0, StateStart) = DATEDIFF(dd, 0, StateEnd) --On the same utc day
	UNION ALL
	SELECT person_code, state_group_name, state_group_code, StateStart, StateEnd FROM [stage].[v_stg_agent_state_split_midnight] --stretching across utc midnight
	) stg
LEFT JOIN mart.dim_person dp
	ON stg.person_code = dp.person_code
	AND --trim
		(
			(cast(stg.[StateStart] as smalldatetime) >= dp.valid_from_date)
		AND
			(cast(stg.StateStart as smalldatetime) < dp.valid_to_date)
		)
LEFT JOIN mart.dim_state_group sg
	ON sg.state_group_code = stg.state_group_code
INNER JOIN mart.dim_date d
	ON cast(DATEADD(dd, DATEDIFF(dd, 0, stg.StateStart), 0) as smalldatetime) = d.date_date
INNER JOIN mart.dim_interval i
	ON cast(dateadd(MINUTE,(DATEPART(HOUR,stg.StateStart)*60+DATEPART(MINUTE,stg.StateStart)),'1900-01-01') as smalldatetime) between i.interval_start and i.interval_end
LEFT JOIN mart.bridge_time_zone btz
	ON btz.date_id = d.date_id
	AND btz.interval_id = i.interval_id
	AND dp.time_zone_id = btz.time_zone_id
GROUP BY
	btz.local_date_id,
	dp.person_id,
	sg.state_group_id
GO