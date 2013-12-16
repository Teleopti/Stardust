IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_shift_category_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_shift_category_load]
GO


-- =============================================
-- Author:		ChLu
-- Create date: 2008-01-31
-- Description:	
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 2009-04-27 Change mindate format DJ
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_shift_category_load]
WITH EXECUTE AS OWNER	
AS

--Create mindate
DECLARE @mindate as smalldatetime
SELECT @mindate=CAST('19000101' as smalldatetime)

--------------------------------------------------------------------------
-- Not Defined shift category
SET IDENTITY_INSERT mart.dim_shift_category ON

INSERT INTO mart.dim_shift_category
	(
	shift_category_id,
	shift_category_code,
	shift_category_name, 
	shift_category_shortname, 
	display_color, 
	business_unit_id,
	datasource_id,  
	datasource_update_date,
	is_deleted
	)
SELECT 
	shift_category_id			= -1, 
	shift_category_code			= '00000000-0000-0000-0000-000000000000', 
	shift_category_name			= 'Not Defined', 
	shift_category_shortname	= 'Not Defined', 
	business_unit_id			= -1,
	display_color				= -1, 
	datasource_id				= -1,  
	datasource_update_date		= @mindate,
	is_deleted					= 0
WHERE NOT EXISTS (SELECT * FROM mart.dim_shift_category where shift_category_id = -1)

SET IDENTITY_INSERT mart.dim_shift_category OFF

---------------------------------------------------------------------------
-- update changes on shift_category
UPDATE mart.dim_shift_category
SET 
	shift_category_name			= s.shift_category_name, 
	shift_category_shortname	= s.shift_category_shortname, 
	display_color				= s.display_color, 
	update_date					= getdate(), 
	datasource_update_date		= s.datasource_update_date,
	is_deleted					= s.is_deleted
FROM
	Stage.stg_shift_category s
WHERE 
	s.shift_category_code	= mart.dim_shift_category.shift_category_code AND
	s.datasource_id			= mart.dim_shift_category.datasource_id

-- Insert new shift categories
INSERT INTO mart.dim_shift_category
	(
	shift_category_code, 
	shift_category_name, 
	shift_category_shortname, 
	display_color,
	business_unit_id, 
	datasource_id,
	datasource_update_date,
	is_deleted
	)
SELECT 
	shift_category_code			= s.shift_category_code, 
	shift_category_name			= s.shift_category_name, 
	shift_category_shortname	= s.shift_category_shortname, 
	display_color				= s.display_color, 
	business_unit_id			= bu.business_unit_id,
	datasource_id				= 1,
	datasource_update_date		= s.datasource_update_date,
	is_deleted					= s.is_deleted
FROM
	Stage.stg_shift_category s
JOIN
	mart.dim_business_unit bu
ON
	s.business_unit_code	= bu.business_unit_code
WHERE 
	NOT EXISTS (SELECT shift_category_id FROM mart.dim_shift_category d WHERE d.shift_category_code = s.shift_category_code and datasource_id = 1)



---------------------------------------------------------------------------
-- insert from stg_schedule_
-- Varför hämtar vi shift_categories den här vägen?! //DJ, 2010-07-26
-- Lämnar den utan is_deleted tills vidare, då lär vi ju märka om den används ;-) //DJ, 2010-07-26
INSERT INTO mart.dim_shift_category
	(
	shift_category_code, 
	datasource_id,
	datasource_update_date
	)
SELECT 
	shift_category_code		= s.shift_category_code,
	datasource_id			= 1,
	datasource_update_date	= s.datasource_update_date
FROM
	(
		SELECT
			shift_category_code,
			datasource_update_date=max(datasource_update_date)
		FROM Stage.stg_schedule
		WHERE NOT shift_category_code IS NULL
		GROUP BY shift_category_code
	) s
WHERE 
	NOT EXISTS
		(
			SELECT shift_category_id
			FROM mart.dim_shift_category d
			WHERE d.shift_category_code = s.shift_category_code
			and d.datasource_id=1
		)
GO

