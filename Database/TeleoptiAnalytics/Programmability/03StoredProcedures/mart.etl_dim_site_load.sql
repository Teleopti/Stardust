IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_site_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_site_load]
GO


-- =============================================
-- Author:		ChLu
-- Create date: 2008-01-30
-- Description:	Loads persons from stg_person to dim_site.
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_site_load] 
WITH EXECUTE AS OWNER	
AS


--------------------------------------------------------------------------
-- Not Defined site
SET IDENTITY_INSERT mart.dim_site ON

INSERT INTO mart.dim_site
	(
	site_id,
	site_name, 
	business_unit_id,
	datasource_id
	)
SELECT 
	site_id		= -1,
	site_name		= 'Not Defined',
	business_unit_id		= -1,
	datasource_id			= -1
WHERE
	NOT EXISTS (SELECT d.site_id FROM mart.dim_site d WHERE d.site_id=-1)

-- insert all, used in reports 
INSERT INTO mart.dim_site
	(
	site_id,
	site_name, 
	business_unit_id,
	datasource_id
	)
SELECT 
	site_id		= -2,
	site_name		= 'All',
	business_unit_id	= -1,
	datasource_id			= -1
WHERE
	NOT EXISTS (SELECT d.site_id FROM mart.dim_site d WHERE d.site_id=-2)

SET IDENTITY_INSERT mart.dim_site OFF

---------------------------------------------------------------------------
-- update changes on person
UPDATE mart.dim_site
SET 
	site_name			= sub.site_name,
	business_unit_id	= sub.business_unit_id
FROM
( 	
SELECT 
	s.site_code,
	s.datasource_id,
	business_unit_id=isnull(business_unit_id,-1),
	site_name=max(s.site_name)
FROM
	Stage.stg_person s
LEFT JOIN
	mart.dim_business_unit bu
ON
	bu.business_unit_code	= s.business_unit_code
GROUP BY 
	s.datasource_id,
	s.site_code,
	isnull(business_unit_id,-1)	
)sub
WHERE 
	sub.site_code		= mart.dim_site.site_code	AND
	sub.datasource_id	= mart.dim_site.datasource_id

-- Insert new site
INSERT INTO mart.dim_site
	(
	site_code,
	site_name,
	business_unit_id,
	datasource_id
	)
SELECT DISTINCT
	site_code			= s.site_code,
	site_name			= s.site_name,
	business_unit_id	= isnull(bu.business_unit_id,-1),
	datasource_id		= s.datasource_id
FROM
	Stage.stg_person s
LEFT JOIN
	mart.dim_business_unit bu
ON
	s.business_unit_code = bu.business_unit_code
WHERE 
	NOT EXISTS (SELECT site_code
				FROM mart.dim_site p 
				WHERE
						s.site_code	= p.site_code	AND
						s.datasource_id			= p.datasource_id
				)

GO

