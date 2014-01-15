IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[stage].[v_stg_agent_state_split_midnight]'))
DROP VIEW [stage].[v_stg_agent_state_split_midnight]
GO
--SELECT * FROM [stage].[v_stg_agent_state_split_midnight];SELECT * FROM stage.stg_agent_state
--select * from mart.sys_numbers n
--set statistics time on
--2013-11-25 12:34:00.560
CREATE VIEW [stage].[v_stg_agent_state_split_midnight]
AS
WITH d(n,person_code,state_group_name,state_group_code, org_from_date,org_to_date,period_days,begin_period,end_period) AS 
(
	SELECT
		n.n,
		d.person_code,
		d.state_group_name,
		d.state_group_code, 
		d.StateStart,
		DATEADD(ss,d.time_in_state_s,d.StateStart),
		DATEDIFF(DAY, d.StateStart, DATEDIFF(DAY, d.StateStart, DATEADD(ss,d.time_in_state_s,d.StateStart))),
		DATEADD(day, n.n, DATEDIFF(day, 0, d.StateStart)),
		DATEADD(day, n.n+1, DATEDIFF(day, 0, d.StateStart))
	FROM mart.sys_numbers n
	INNER JOIN [stage].[stg_agent_state] AS d
		ON DATEADD(ss,d.time_in_state_s,d.StateStart) > DATEADD(DAY, n.n-1, d.StateStart)
	WHERE d.days_cross_midnight>0 --only the ones stretching across utc midnight
)
SELECT
	d.person_code,
	d.state_group_name,
	d.state_group_code,
	begin_period,
	end_period,
	StateStart = CASE n WHEN 0  THEN org_from_date ELSE begin_period END,
	StateEnd   = CASE n WHEN period_days THEN org_to_date ELSE end_period END
FROM d
