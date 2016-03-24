--sps
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_agent_state_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_agent_state_load]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_state_group_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_state_group_load]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_state_group_load_livefeed') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_state_group_load_livefeed]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_twolist_state_group_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_twolist_state_group_get]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_time_in_state_per_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_time_in_state_per_agent]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_stg_state_group_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [stage].[etl_stg_state_group_delete]
GO

--views
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[stage].[v_stg_agent_state_split_midnight]'))
DROP VIEW [stage].[v_stg_agent_state_split_midnight]
GO
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[stage].[v_stg_agent_state_sum]'))
DROP VIEW [stage].[v_stg_agent_state_sum]
GO

--tables
DROP TABLE [mart].[sys_numbers]
GO
DROP TABLE [stage].[stg_state_group]
GO
DROP TABLE [stage].[stg_agent_state]
GO
DROP TABLE [mart].[fact_agent_state]
GO
DROP TABLE [mart].[dim_state_group]
GO
IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[RTA].[tr_ActualAgentState_update]'))
DROP TRIGGER [RTA].[tr_ActualAgentState_update]
GO

--delete table data

DELETE FROM mart.report where id= 'BB8C21BA-0756-4DDC-8B26-C9D5715A3443'

DELETE FROM mart.report_control_collection where CollectionId='F775ED72-5B41-4FEA-87DB-04AD347D4537'

DELETE FROM mart.report_control WHERE control_name='twolistStateGroup' and control_id=44

