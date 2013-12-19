IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_agent_state_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_agent_state_load]
GO
-- =============================================
-- Author:		David J
-- Create date: 2013-11-08
-- Description:	Loads agent states
-- Had to write a IF to handle SQL 2005
-- Update date: 
-- =============================================
--EXEC [mart].[etl_fact_agent_state_load] '00000000-0000-0000-0000-000000000000'
CREATE PROCEDURE [mart].[etl_fact_agent_state_load] 
@business_unit_code uniqueidentifier
WITH EXECUTE AS OWNER	
AS
SET NOCOUNT ON

--re-load dim_state_group with state groups that might have poped in between ETL.dim_state_group and now()
EXEC [mart].[etl_dim_state_group_load_livefeed]

--continue
SET NOCOUNT OFF

--existing rows
UPDATE f
SET f.time_in_state_s = f.time_in_state_s + v.time_in_state_s
FROM mart.fact_agent_state AS f WITH (TABLOCK)
INNER JOIN mart.v_fact_agent_state_merge AS v
ON (
		f.date_id		= v.date_id
	AND f.person_id		= v.person_id
	AND f.state_group_id= v.state_group_id
	)

--new rows
INSERT INTO mart.fact_agent_state
	(
	date_id,
	person_id,
	state_group_id,
	time_in_state_s,
	datasource_id,
	insert_date
	)
SELECT
	date_id,
	person_id,
	state_group_id,
	time_in_state_s,
	datasource_id,
	insert_date
FROM mart.v_fact_agent_state_merge v
WHERE NOT EXISTS (
	SELECT 1
	FROM mart.fact_agent_state
	WHERE
	date_id				= v.date_id
	AND person_id		= v.person_id
	AND state_group_id	= v.state_group_id
	)

TRUNCATE TABLE [stage].[stg_agent_state]
