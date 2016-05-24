IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RTA].[rta_addorupdate_actualagentstate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [RTA].[rta_addorupdate_actualagentstate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RTA].[rta_get_last_batch]') AND type in (N'P', N'PC'))
DROP PROCEDURE [RTA].[rta_get_last_batch]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RTA].[rta_insert_agentstate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [RTA].[rta_insert_agentstate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RTA].[rta_load_actual_agent_statecode]') AND type in (N'P', N'PC'))
DROP PROCEDURE [RTA].[rta_load_actual_agent_statecode]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RTA].[rta_load_agentstate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [RTA].[rta_load_agentstate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RTA].[rta_load_external_logon]') AND type in (N'P', N'PC'))
DROP PROCEDURE [RTA].[rta_load_external_logon]
GO

DROP TABLE RTA.ActualAgentState
GO
