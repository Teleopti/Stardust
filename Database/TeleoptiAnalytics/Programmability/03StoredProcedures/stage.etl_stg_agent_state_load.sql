IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[etl_stg_agent_state_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [stage].[etl_stg_agent_state_load]
GO
-- =============================================
-- Author:		DJ+KJ
-- Create date: 2014-01-16
-- Update date: yyyy-mm-dd
-- Description:	Loads states from RTA-history into stage/work table.
--				To minimise the time we block table: [RTA].[ActualAgentState_History]
--				see: RTA.tr_ActualAgentState_update
-- =============================================
--EXEC [stage].[etl_stg_agent_state_load] '00000000-0000-0000-0000-000000000000'
CREATE PROCEDURE [stage].[etl_stg_agent_state_load] 
@business_unit_code uniqueidentifier		
WITH EXECUTE AS OWNER
AS

SET NOCOUNT ON

INSERT INTO [stage].[stg_agent_state] (StateStart, person_code, time_in_state_s, state_group_code, days_cross_midnight)
SELECT StateStart, person_code, time_in_state_s, state_group_code, days_cross_midnight
FROM [RTA].[ActualAgentState_History] WITH (TABLOCKX)

TRUNCATE TABLE [RTA].[ActualAgentState_History]

