IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_agent_state_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_agent_state_load]
GO
-- =============================================
-- Author:		David J
-- Create date: 2013-11-08
-- Description:	Loads agent states
-- Update date: 
-- =============================================
--EXEC [mart].[etl_fact_agent_state_load] '00000000-0000-0000-0000-000000000000'
CREATE PROCEDURE [mart].[etl_fact_agent_state_load] 
@business_unit_code uniqueidentifier
WITH EXECUTE AS OWNER	
AS
SET NOCOUNT ON

--load dim_state_group
EXEC [mart].[etl_dim_state_group]

--delete and load into intermediate table in the same command
DELETE stg
OUTPUT
	deleted.person_code,
	deleted.state_group_code,
	deleted.time_in_state_s,
	deleted.state_start
INTO [stage].[stg_agent_state_loading](person_code,state_group_code,time_in_state_s,state_start)
FROM stage.stg_agent_state stg
INNER JOIN mart.dim_state_group d
	ON d.state_group_code = stg.state_group_code

SET NOCOUNT OFF
INSERT INTO mart.fact_agent_state
SELECT 
date_id			= d.date_id,
person_id		= dp.person_id,
interval_id		= i.interval_id,
state_start		= stg.state_start,
state_group_id	= sg.state_group_id,
time_in_state_s	= stg.time_in_state_s,
datasource_id	= sg.datasource_id,
insert_date		= getdate()
FROM [stage].[stg_agent_state_loading] stg
INNER JOIN mart.dim_person dp
	ON stg.person_code = dp.person_code
	AND --trim
		(
			(stg.state_start >= dp.valid_from_date)
		AND
			(stg.state_start < dp.valid_to_date)
		)
INNER JOIN mart.dim_state_group sg
	ON sg.state_group_code = stg.state_group_code
INNER JOIN mart.dim_date d
	ON DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) = d.date_date
INNER JOIN mart.dim_interval i
	ON dateadd(MINUTE,(DATEPART(HOUR,stg.state_start)*60+DATEPART(MINUTE,stg.state_start)),'1900-01-01') BETWEEN i.interval_start AND i.interval_end
WHERE stg.time_in_state_s > 0

--truncate "temporary" data
TRUNCATE TABLE [stage].[stg_agent_state_loading]
GO