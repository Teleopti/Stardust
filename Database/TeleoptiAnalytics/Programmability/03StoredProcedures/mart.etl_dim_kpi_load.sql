IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_kpi_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_kpi_load]
GO


-- =============================================
-- Author:		KJ
-- Create date: 2008-04-18
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- 2008-11-24 Added column business_unit_id KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 2009-04-28 Renmoved business_unit from load KJ
-- Description:	Loads kpis from stg_kpi to dim_kpi.

-- =============================================
CREATE PROCEDURE [mart].[etl_dim_kpi_load] 
	
AS
---------------------------------------------------------------------------
-- update changes on kpi
UPDATE mart.dim_kpi
SET 
	kpi_name				= s.kpi_name,
	resource_key			=s.resource_key,--ska denna hÃ¤mtas frÃ¥n raptor och anvÃ¤nda samma??
	target_value_type		=s.target_value_type,
	default_target_value	=s.default_target_value,
	default_min_value		=s.default_min_value,
	default_max_value		=s.default_max_value,
	default_between_color	=s.default_between_color,
	default_lower_than_min_color=s.default_lower_than_min_color, 
	default_higher_than_max_color=s.default_higher_than_max_color, 
	--business_unit_id			= bu.business_unit_id,
	datasource_id=s.datasource_id, 
	datasource_update_date=s.datasource_update_date
FROM
	Stage.stg_kpi s
--JOIN
--	mart.dim_business_unit bu
--ON
--	s.business_unit_code	= bu.business_unit_code
WHERE 
	s.kpi_code = dim_kpi.kpi_code

-- Insert new kpis
INSERT INTO mart.dim_kpi
	(
	kpi_code, 
	kpi_name, 
	resource_key, 
	target_value_type, 
	default_target_value, 
	default_min_value, 
	default_max_value, 
	default_between_color, 
	default_lower_than_min_color, 
	default_higher_than_max_color, 
--	business_unit_id,
	datasource_id, 
	datasource_update_date
	)
SELECT 
	kpi_code			= s.kpi_code, 
	kpi_name			= s.kpi_name, 
	resource_key		=s.resource_key, --ska denna hÃ¤mtas frÃ¥n raptor och anvÃ¤nda samma??
	target_value_type	=s.target_value_type, 
	default_target_value=s.default_target_value, 
	default_min_value	=s.default_min_value, 
	default_max_value	=s.default_max_value, 
	default_between_color=s.default_between_color, 
	default_lower_than_min_color=s.default_lower_than_min_color, 
	default_higher_than_max_color=s.default_higher_than_max_color,
--	business_unit_id	= bu.business_unit_id,
	datasource_id		= s.datasource_id,
	datasource_update_date =s.datasource_update_date
FROM
	Stage.stg_kpi s
--JOIN
--	mart.dim_business_unit bu
--ON
--	s.business_unit_code	= bu.business_unit_code
WHERE 
	NOT EXISTS (SELECT kpi_id FROM mart.dim_kpi d WHERE d.kpi_code = s.kpi_code and d.datasource_id=s.datasource_id)


GO

