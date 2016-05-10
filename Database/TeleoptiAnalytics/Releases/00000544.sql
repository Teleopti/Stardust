IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_scenario_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_scenario_delete]
GO
