IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RTA].[rta_load_actual_agent_statecode]') AND type in (N'P', N'PC'))
DROP PROCEDURE [RTA].[rta_load_actual_agent_statecode]
GO


-- =============================================
-- Author:  Erik S
-- Create date: 2012-11-28
-- Description: Load acual agent state
-- =============================================
ALTER PROCEDURE [RTA].[rta_load_actual_agent_statecode]
AS
BEGIN
 SET NOCOUNT ON;

 SELECT StateCode, PersonId  FROM [RTA].[ActualAgentState]
END