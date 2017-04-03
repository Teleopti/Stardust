IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_group_page_person_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_group_page_person_load]
GO


-- =============================================
-- Author:		DJ
-- Create date: 2010-08-30
-- Description: Loads bridge table (grouping+group+person)
--
-- Change Log:
-- When			Who What
-- 2011-02-17	DJ	Delete only current BU

-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_group_page_person_load] 
@business_unit_code uniqueidentifier
AS
---------------------------------------------------------------------------

DECLARE @empty_guid uniqueidentifier
SET @empty_guid = '00000000-0000-0000-0000-000000000000'

-- Delete persons in bridge that belongs to this BU
DELETE bridge
FROM 
	[mart].[bridge_group_page_person] bridge
INNER JOIN 
	mart.dim_person p WITH (NOLOCK)
ON 
	p.person_id = bridge.person_id
WHERE 
	p.business_unit_code = @business_unit_code
	
	
-- Insert persons for the user defined group pages
INSERT INTO [mart].[bridge_group_page_person]
SELECT 
	group_page_id	= g.group_page_id,
	person_id		= p.person_id,
	datasource_id	= g.datasource_id,
	insert_date		= getdate()
FROM 
	stage.stg_group_page_person s
INNER JOIN 
	mart.dim_person p  WITH (NOLOCK)
ON 
	p.person_code = s.person_code
	AND p.business_unit_code = s.business_unit_code
INNER JOIN 
	mart.dim_group_page g
ON 
	s.group_page_code = g.group_page_code 
	AND s.group_code = g.group_code
WHERE s.group_is_custom = 1
	AND g.business_unit_code = @business_unit_code
	AND s.group_code <> @empty_guid
	
-- Insert persons for the dynamic group pages (excluding Note)
INSERT INTO [mart].[bridge_group_page_person]
SELECT 
	group_page_id	= g.group_page_id,
	person_id		= p.person_id,
	datasource_id	= g.datasource_id,
	insert_date		= getdate()
FROM 
	stage.stg_group_page_person s
INNER JOIN 
	mart.dim_person p WITH (NOLOCK)
ON 
	p.person_code = s.person_code
	AND p.business_unit_code = s.business_unit_code
INNER JOIN 
	mart.dim_group_page g
ON 
	s.group_page_name_resource_key = g.group_page_name_resource_key
	AND s.group_code = g.group_code
WHERE s.group_is_custom = 0
	AND g.business_unit_code = @business_unit_code
	AND s.group_code <> @empty_guid
	

-- Insert persons for the dynamic group page Note
INSERT INTO [mart].[bridge_group_page_person]
SELECT 
	group_page_id	= g.group_page_id,
	person_id		= p.person_id,
	datasource_id	= g.datasource_id,
	insert_date		= getdate()
FROM 
	stage.stg_group_page_person s
INNER JOIN 
	mart.dim_person p  WITH (NOLOCK)
ON 
	p.person_code = s.person_code
	AND p.business_unit_code = s.business_unit_code
INNER JOIN 
	mart.dim_group_page g
ON 
	s.group_page_name_resource_key = g.group_page_name_resource_key
	AND s.group_name COLLATE Latin1_General_CS_AS = g.group_name COLLATE Latin1_General_CS_AS 
WHERE
	s.group_is_custom = 0
	AND g.business_unit_code = @business_unit_code
	AND s.group_code = @empty_guid