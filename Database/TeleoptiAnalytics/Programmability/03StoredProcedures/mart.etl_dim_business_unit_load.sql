IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_business_unit_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_business_unit_load]
GO


-- =============================================
-- Author:		ChLu
-- Create date: 2008-01-30
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 2008-09-15 Loads from stg_business_unit instead KJ
-- Description:	Loads business_units from stg_person to dim_business_unit.

-- =============================================
CREATE PROCEDURE [mart].[etl_dim_business_unit_load] 
@business_unit_code uniqueidentifier		
AS

--------------------------------------------------------------------------
-- Not Defined business_unit
SET IDENTITY_INSERT mart.dim_business_unit ON

INSERT INTO mart.dim_business_unit
	(
	business_unit_id,
	business_unit_name, 
	datasource_id
	)
SELECT 
	business_unit_id		= -1,
	business_unit_name		= 'Not Defined',
	datasource_id			= -1
WHERE
	NOT EXISTS (SELECT d.business_unit_id FROM mart.dim_business_unit d WHERE d.business_unit_id=-1)

SET IDENTITY_INSERT mart.dim_business_unit OFF

UPDATE mart.dim_business_unit
SET 
	business_unit_name = sub.business_unit_name
FROM
( 	
SELECT 
	s.business_unit_code,
	s.datasource_id,
	business_unit_name=max(s.business_unit_name)
FROM
	Stage.stg_business_unit s
GROUP BY 
	s.datasource_id,
	s.business_unit_code
)sub
WHERE 
	sub.business_unit_code		= mart.dim_business_unit.business_unit_code	AND
	sub.datasource_id			= mart.dim_business_unit.datasource_id

-- Insert new business_unit
INSERT INTO mart.dim_business_unit
	(
	business_unit_code,
	business_unit_name,
	datasource_id
	)
SELECT DISTINCT
	business_unit_code		= business_unit_code,
	business_unit_name		= business_unit_name,
	datasource_id			= datasource_id
FROM
	Stage.stg_business_unit s
WHERE 
	NOT EXISTS (SELECT business_unit_code
				FROM mart.dim_business_unit p 
				WHERE
						s.business_unit_code	= p.business_unit_code	AND
						s.datasource_id			= p.datasource_id
				)
---------------------------------------------------------------------------
-- update changes 
/*
UPDATE dim_business_unit
SET 
	business_unit_name = sub.business_unit_name
FROM
( 	
SELECT 
	s.business_unit_code,
	s.datasource_id,
	business_unit_name=max(s.business_unit_name)
FROM
	v_stg_person s
GROUP BY 
	s.datasource_id,
	s.business_unit_code
)sub
WHERE 
	sub.business_unit_code		= dim_business_unit.business_unit_code	AND
	sub.datasource_id			= dim_business_unit.datasource_id

-- Insert new business_unit
INSERT INTO dim_business_unit
	(
	business_unit_code,
	business_unit_name,
	datasource_id
	)
SELECT DISTINCT
	business_unit_code		= business_unit_code,
	business_unit_name		= business_unit_name,
	datasource_id			= datasource_id
FROM
	v_stg_person s
WHERE 
	NOT EXISTS (SELECT business_unit_code
				FROM dim_business_unit p 
				WHERE
						s.business_unit_code	= p.business_unit_code	AND
						s.datasource_id			= p.datasource_id
				)
*/
GO

