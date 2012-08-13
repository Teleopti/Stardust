IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_workload_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_workload_load]
GO


-- =============================================
-- Author:		ChLu
-- Create date: 2008-01-30
-- Description:	Loads workloads from stg_sworkload to dim_workload.
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 2010-07-05 Added percentages for calculated calls
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_workload_load] 
WITH EXECUTE AS OWNER	
AS
--------------------------------------------------------------------------
-- Not Defined workload
SET IDENTITY_INSERT mart.dim_workload ON

INSERT INTO mart.dim_workload
	(
	workload_id,
	workload_name,
	skill_name, 
	time_zone_id, 
	forecast_method_name, 
	business_unit_id,
	datasource_id
	)
SELECT 
	workload_id			= -1,
	workload_name		= 'Not Defined',
	skill_name			= 'Not Defined', 
	time_zone_id		= -1, 
	forecast_method_name= 'Not Defined', 
	business_unit_id	= -1,
	datasource_id		= -1	
WHERE NOT EXISTS (SELECT * FROM mart.dim_workload where workload_id = -1)

SET IDENTITY_INSERT mart.dim_workload OFF


---------------------------------------------------------------------------
-- update changes on workload
UPDATE mart.dim_workload
SET 
	workload_name								= s.workload_name,
	skill_code									= s.skill_code,
	skill_name									= s.skill_name,	
	time_zone_id								= z.time_zone_id,
	forecast_method_code						= s.forecast_method_code,
	forecast_method_name						= s.forecast_method_name,
	percentage_offered							= s.percentage_offered,
	percentage_overflow_in						= s.percentage_overflow_in,
	percentage_overflow_out						= s.percentage_overflow_out,
	percentage_abandoned						= s.percentage_abandoned,
	percentage_abandoned_short					= s.percentage_abandoned_short,
	percentage_abandoned_within_service_level	= s.percentage_abandoned_within_service_level,
	percentage_abandoned_after_service_level	= s.percentage_abandoned_after_service_level,
	update_date				= getdate(),
	datasource_update_date	= s.datasource_update_date,
	is_deleted				= s.is_deleted
FROM
	Stage.stg_workload s
JOIN
	mart.dim_time_zone z
ON
	s.time_zone_code = z.time_zone_code
WHERE 
	s.workload_code = mart.dim_workload.workload_code

-- Insert new workload
INSERT INTO mart.dim_workload
	(
	workload_code,
	workload_name,
	skill_code, 
	skill_name, 
	time_zone_id, 
	forecast_method_code, 
	forecast_method_name, 
	percentage_offered,
	percentage_overflow_in,
	percentage_overflow_out,
	percentage_abandoned,
	percentage_abandoned_short,
	percentage_abandoned_within_service_level,
	percentage_abandoned_after_service_level,
	business_unit_id,
	datasource_id, 
	datasource_update_date,
	is_deleted
	)
SELECT DISTINCT 
	workload_code								= s.workload_code,
	workload_name								= s.workload_name,
	skill_code									= s.skill_code, 
	skill_name									= s.skill_name, 
	time_zone_id								= z.time_zone_id, 
	forecast_method_code						= s.forecast_method_code, 
	forecast_method_name						= s.forecast_method_name,	 
	percentage_offered							= s.percentage_offered,
	percentage_overflow_in						= s.percentage_overflow_in,
	percentage_overflow_out						= s.percentage_overflow_out,
	percentage_abandoned						= s.percentage_abandoned,
	percentage_abandoned_short					= s.percentage_abandoned_short,
	percentage_abandoned_within_service_level	= s.percentage_abandoned_within_service_level,
	percentage_abandoned_after_service_level	= s.percentage_abandoned_after_service_level,
	business_unit_id							= bu.business_unit_id,
	datasource_id								= 1, 
	datasource_update_date						= s.datasource_update_date,
	is_deleted									= s.is_deleted
FROM
	Stage.stg_workload s
JOIN 
	mart.dim_business_unit bu
ON
	s.business_unit_code = bu.business_unit_code
JOIN
	mart.dim_time_zone	z
ON
	s.time_zone_code = z.time_zone_code
WHERE 
	NOT EXISTS (SELECT workload_code FROM mart.dim_workload d WHERE d.workload_code = s.workload_code and d.datasource_id=1)


---------------------------------------------------------------------------
-- insert from stg_forecast_workload

/*
--Create system mindate
DECLARE @mindate as smalldatetime
SELECT @mindate=CAST('19000101' as smalldatetime)

INSERT INTO mart.dim_workload
	(
	workload_code, 
	datasource_id,
	datasource_update_date
	)
SELECT 
	workload_code			= s.workload_code,
	datasource_id			= 1,
	datasource_update_date	= isnull(s.datasource_update_date, @mindate)
FROM
	(SELECT workload_code,datasource_update_date=max(datasource_update_date) FROM v_stg_forecast_workload GROUP BY workload_code) s
WHERE 
	NOT EXISTS (SELECT workload_id FROM dim_workload d WHERE d.workload_code = s.workload_code and d.datasource_id=1)
*/
UPDATE mart.dim_workload
SET 
	skill_id= s.skill_id
FROM mart.dim_skill s
WHERE mart.dim_workload.skill_code= s.skill_code

GO

