IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_state_group_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_state_group_load]
GO

-- =============================================
-- Author:		David
-- Create date: 2013-11-08
-- Description:	Loads state groups from stage.
--				Note: Business unit is not available in stage but implictly loaded via dim_person
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_state_group_load]
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
	is_deleted,
	is_log_out_state
	)
SELECT 
	state_group_id				= -1, 
	state_group_code			= NULL, 
	state_group_name			= 'Not Defined', 
	business_unit_id			= -1,
	datasource_id				= -1,
	insert_date					= @mindate,
	update_date					= @mindate,
	datasource_update_date		= @mindate,
	is_deleted					= 0,
	is_log_out_state			= 0
WHERE NOT EXISTS (SELECT * FROM mart.dim_state_group where state_group_id = -1)

SET IDENTITY_INSERT mart.dim_state_group OFF

--------------------
-- get new and updated from stage (normal ELT)
--------------------
UPDATE mart.dim_state_group
SET 
	state_group_name			= s.state_group_name,
	datasource_update_date		= s.datasource_update_date,
	business_unit_id			= ISNULL(bu.business_unit_id,-1),
	datasource_id				= s.datasource_id,
	is_deleted					= s.is_deleted,
	update_date					= getdate(),
	is_log_out_state			= s.is_log_out_state
FROM stage.stg_state_group s
LEFT JOIN mart.dim_business_unit bu
	ON bu.business_unit_code = s.business_unit_code
WHERE 
	s.state_group_code	= mart.dim_state_group.state_group_code
AND --they are changed in any way
	(
		s.state_group_name <> mart.dim_state_group.state_group_name
		OR
		s.is_deleted <> mart.dim_state_group.is_deleted
		OR
		s.datasource_update_date <> mart.dim_state_group.datasource_update_date
	)

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
	state_group_code			= s.state_group_code, 
	state_group_name			= s.state_group_name, 
	business_unit_id			= bu.business_unit_id,
	datasource_id				= s.datasource_id,
	insert_date					= getdate(),
	update_date					= getdate(),
	datasource_update_date		= getdate(),
	is_deleted					= s.is_deleted,
	is_log_out_state			= s.is_log_out_state
FROM stage.stg_state_group s
LEFT JOIN mart.dim_business_unit bu
	ON bu.business_unit_code = s.business_unit_code
WHERE 
	NOT EXISTS (SELECT state_group_id FROM mart.dim_state_group d WHERE d.state_group_code = s.state_group_code and d.datasource_id = s.datasource_id)
GO