IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_state_group_load_livefeed]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_state_group_load_livefeed]
GO

-- =============================================
-- Author:		David
-- Create date: 2013-11-08
-- Description:	Loads state groups from stage.
--				Note: Business unit is not available in stage but implictly loaded via dim_person
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_state_group_load_livefeed]
AS
--------------------
-- get new and updated from live feeed
--------------------
UPDATE mart.dim_state_group
SET 
	state_group_name			= s.state_group_name,
	update_date					= getdate()
FROM stage.v_stg_state_group s
WHERE 
	s.state_group_code	= mart.dim_state_group.state_group_code AND
	s.datasource_id		= mart.dim_state_group.datasource_id AND
	s.state_group_name	<>mart.dim_state_group.state_group_name 

-- Insert new state groups
INSERT INTO mart.dim_state_group
	(
	state_group_code,
	state_group_name,
	business_unit_id,
	datasource_id,
	insert_date,
	update_date,
	datasource_update_date,
	is_deleted,
	is_log_out_state
	)
SELECT 
	state_group_code			= state_group_code, 
	state_group_name			= state_group_name, 
	business_unit_id			= business_unit_id,
	datasource_id				= datasource_id,
	insert_date					= getdate(),
	update_date					= getdate(),
	datasource_update_date		= getdate(),
	is_deleted					= 0,
	is_log_out_state			= -1
FROM stage.v_stg_state_group s
WHERE 
	NOT EXISTS (SELECT state_group_id FROM mart.dim_state_group d WHERE d.state_group_code = s.state_group_code and d.datasource_id = s.datasource_id)