IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_overtime_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_overtime_load]
GO

-- =============================================
-- Author:		Mattias E
-- Create date: 2011-02-25
-- Description:	Loads overtime from stg_overtime to dim_overtime.
-- Update date: 2011-03-02 Added -1, 'Not Defined' Mattias E
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_overtime_load] 
	
WITH EXECUTE AS OWNER
AS
----------------------------------------------------------------------------
-- Not Defined overtime
SET IDENTITY_INSERT mart.dim_overtime ON

INSERT INTO mart.dim_overtime
	(
	overtime_id,
	overtime_code,
	overtime_name,
	business_unit_id,
	datasource_id
	)
SELECT
	overtime_id			= -1,
	overtime_code		= '00000000-0000-0000-0000-000000000000',
	overtime_name		= 'Not Defined',
	business_unit_id	= -1,
	datasource_id		= -1
WHERE NOT EXISTS (SELECT * FROM mart.dim_overtime where overtime_id = -1)

SET IDENTITY_INSERT mart.dim_overtime OFF


---------------------------------------------------------------------------
-- update changes on overtimes
UPDATE mart.dim_overtime
SET 
	overtime_code			= s.overtime_code,
	overtime_name			= s.overtime_name,
	update_date				= GETDATE(),
	datasource_update_date	= s.update_date,
	is_deleted				= s.is_deleted
FROM
	Stage.stg_overtime s
WHERE s.overtime_code = mart.dim_overtime.overtime_code

-- Insert new overtimes
INSERT INTO mart.dim_overtime
	(
	overtime_code,
	overtime_name,
	business_unit_id,
	datasource_id,
	datasource_update_date,
	is_deleted
	)
SELECT DISTINCT 
	multiplicator_definition_set_code	= s.overtime_code,
	multiplicator_definition_set_name	= s.overtime_name,
	business_unit_id					= bu.business_unit_id,
	datasource_id						= 1,
	datasource_update_date				= MAX(s.datasource_update_date),
	is_deleted							= s.is_deleted
FROM
	Stage.stg_overtime s
JOIN
	mart.dim_business_unit	bu
ON
	s.business_unit_code	= bu.business_unit_code
WHERE 
	NOT EXISTS (SELECT overtime_code FROM mart.dim_overtime d WHERE d.overtime_code = s.overtime_code and d.datasource_id=1)
GROUP BY 
	overtime_code,
	overtime_name,
	bu.business_unit_id,
	s.is_deleted