IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_state_group]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_state_group]
GO

-- =============================================
-- Author:		David
-- Create date: 2013-11-08
-- Description:	Loads state groups from stage.
--				Note: Business unit is not available in stage but implictly loaded via dim_person
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_state_group]
WITH EXECUTE AS OWNER	
AS

--Create mindate
DECLARE @mindate as smalldatetime
SELECT @mindate=CAST('19000101' as smalldatetime)

--------------------------------------------------------------------------
-- Not Defined shift category
SET IDENTITY_INSERT mart.dim_state_group ON

INSERT INTO mart.dim_state_group
	(
	state_group_id,
	state_group_code,
	state_group_name,
	business_unit_id,
	datasource_id,
	insert_date,
	update_date,
	datasource_update_date,
	is_deleted
	)
SELECT 
	state_group_id				= -1, 
	state_group_code			= NULL, 
	state_group_name			= 'Not Defined', 
	business_unit_id			= NULL,
	datasource_id				= -1,
	insert_date					= @mindate,
	update_date					= @mindate,
	datasource_update_date		= @mindate,
	is_deleted					= 0
WHERE NOT EXISTS (SELECT * FROM mart.dim_state_group where state_group_id = -1)

SET IDENTITY_INSERT mart.dim_state_group OFF

---------------------------------------------------------------------------
-- update changes
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
	is_deleted
	)
SELECT 
	state_group_code			= state_group_code, 
	state_group_name			= state_group_name, 
	business_unit_id			= business_unit_id,
	datasource_id				= datasource_id,
	insert_date					= getdate(),
	update_date					= getdate(),
	datasource_update_date		= getdate(),
	is_deleted					= 0
FROM stage.v_stg_state_group s
WHERE 
	NOT EXISTS (SELECT state_group_id FROM mart.dim_state_group d WHERE d.state_group_code = s.state_group_code and d.datasource_id = s.datasource_id)
GO