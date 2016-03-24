--sps
DROP PROCEDURE [mart].[etl_fact_agent_state_load]
GO
DROP PROCEDURE [mart].[etl_dim_state_group_load]
GO
DROP PROCEDURE [mart].[etl_dim_state_group_load_livefeed]
GO
DROP PROCEDURE [mart].[report_control_twolist_state_group_get]
GO
DROP PROCEDURE [mart].[report_data_time_in_state_per_agent]
GO
DROP PROCEDURE [stage].[etl_stg_state_group_delete]
GO

--views
DROP VIEW [stage].[v_stg_agent_state_split_midnight]
GO
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

DROP TRIGGER [RTA].[tr_ActualAgentState_update]
GO

--delete table data

DELETE FROM mart.report where id= 'BB8C21BA-0756-4DDC-8B26-C9D5715A3443'

DELETE FROM mart.report_control_collection where CollectionId='F775ED72-5B41-4FEA-87DB-04AD347D4537'

DELETE FROM mart.report_control WHERE control_name='twolistStateGroup' and control_id=44

