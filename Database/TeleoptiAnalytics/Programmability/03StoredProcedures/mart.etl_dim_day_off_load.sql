/****** Object:  StoredProcedure [mart].[etl_dim_day_off_load]    Script Date: 10/03/2008 17:18:23 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_day_off_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_day_off_load]
GO

-- =============================================
-- Author:		KJ
-- Create date: 2008-10-03
-- Description:	Loads day off from stg_day_off to dim_day_off.
-- Update date log
----------------------------------
-- When			Who What
----------------------------------
-- 2009-02-11	KJ	New mart schema 
-- 2008-10-03		Day Off not available yet. Loads only -1 Not Defined until then.
-- 2008-10-24	KJ	Load one Day Off per BU.
-- 2010-01-14	DJ	Adding DayOff but with Day_Off_Name as PK (Not Day_Off_code as original DW designed)
-- 2011-02-08	DJ	#13471 Days off with same name on two BU does not work in analytics
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_day_off_load]
@business_unit_code uniqueidentifier	
AS
-----------------------------------------------------
-- Not Defined
SET IDENTITY_INSERT mart.dim_day_off ON

INSERT INTO mart.dim_day_off
	(
	day_off_id,
	day_off_name, 
	display_color,
	business_unit_id,
	datasource_id
	)
SELECT 
	day_off_id			=-1, 
	day_off_name		='Not Defined', 
	display_color		= -1,
	business_unit_id	=-1,
	datasource_id		= -1
WHERE NOT EXISTS (SELECT * FROM mart.dim_day_off where day_off_id = -1)

SET IDENTITY_INSERT mart.dim_day_off OFF
-----------------------------------------------------
-- update changes
UPDATE mart.dim_day_off
SET 
	day_off_code	= s.day_off_code,
	display_color	= s.display_color
FROM
	[stage].[stg_day_off] s
INNER JOIN [mart].[dim_business_unit] bu
ON s.business_unit_code = bu.business_unit_code
WHERE 
	s.day_off_name = mart.dim_day_off.day_off_name
AND
	bu.business_unit_id = mart.dim_day_off.business_unit_id
	
-- Insert new 
INSERT INTO mart.dim_day_off
	(
	day_off_code, 
	day_off_name, 
	display_color,
	business_unit_id,
	datasource_id
	)
SELECT 
	day_off_code		= s.day_off_code,
	day_off_name		= s.day_off_name, --This is part of the PK 
	display_color		= s.display_color,
	business_unit_id	= bu.business_unit_id, --This is part of the PK
	datasource_id		= 1
FROM
	[stage].[stg_day_off] s	
INNER JOIN
	[mart].[dim_business_unit] bu
ON
	s.business_unit_code =bu.business_unit_code	
WHERE 
	NOT EXISTS (SELECT day_off_id
				FROM mart.dim_day_off d
				WHERE d.day_off_name = s.day_off_name
				AND d.business_unit_id = bu.business_unit_id
				AND d.datasource_id=1)
-------------------------------------------------

GO