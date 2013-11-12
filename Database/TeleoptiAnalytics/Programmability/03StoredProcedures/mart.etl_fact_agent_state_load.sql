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
IF (SELECT compatibility_level FROM sys.databases WHERE name = DB_NAME()) > 90
BEGIN
EXEC dbo.sp_executesql @statement = N'
CREATE PROCEDURE [mart].[etl_fact_agent_state_load] 
@business_unit_code uniqueidentifier
WITH EXECUTE AS OWNER	
AS
SET NOCOUNT ON


--load dim_state_group
EXEC [mart].[etl_dim_state_group]

--continue
SET NOCOUNT OFF

MERGE mart.fact_agent_state AS f
USING mart.v_fact_agent_state_merge AS v
ON (
		f.date_id		= v.date_id
	AND f.person_id		= v.person_id
	AND f.interval_id	= v.interval_id
	AND f.state_group_id= v.state_group_id
	)
WHEN MATCHED
    THEN
	UPDATE SET f.time_in_state_s = f.time_in_state_s + v.time_in_state_s
WHEN NOT MATCHED THEN
    INSERT (date_id, person_id, interval_id, state_group_id, time_in_state_s, datasource_id, insert_date)
        VALUES (v.date_id, v.person_id, v.interval_id, v.state_group_id, v.time_in_state_s, v.datasource_id, v.insert_date);

TRUNCATE TABLE [stage].[stg_agent_state]
' 
END
ELSE
BEGIN
EXEC dbo.sp_executesql @statement = N'
CREATE PROCEDURE [mart].[etl_fact_agent_state_load] 
@business_unit_code uniqueidentifier
WITH EXECUTE AS OWNER
AS
--if we you see this version of the SP; we have the wrong compabilty level on the database
--We flush the data until compability level is fixed
IF (SELECT compatibility_level FROM sys.databases WHERE name = DB_NAME()) > 90
BEGIN
	TRUNCATE TABLE [stage].[stg_agent_state]
END
' 
END
GO


