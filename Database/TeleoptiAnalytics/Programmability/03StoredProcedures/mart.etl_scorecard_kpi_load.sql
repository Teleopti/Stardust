IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_scorecard_kpi_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_scorecard_kpi_load]
GO


-- =============================================
-- Author:		KJ
-- Create date: 2008-04-18
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- Description:	Loads scorecard_kpi connections from stg_scorecard_kpi to scorecard_kpi.
-- =============================================
CREATE PROCEDURE [mart].[etl_scorecard_kpi_load] 
@business_unit_code uniqueidentifier

AS
---------------------------------------------------------------------------
--Delete current connections scorecard<->kpi
DECLARE @business_unit_id int
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

DELETE FROM mart.scorecard_kpi
WHERE business_unit_id = @business_unit_id

/*
DELETE FROM mart.scorecard_kpi
WHERE business_unit_id = 
	(
		SELECT DISTINCT
			bu.business_unit_id
		FROM 
			Stage.stg_scorecard_kpi sk
		INNER JOIN
			mart.dim_business_unit bu
		ON
			sk.business_unit_code = bu.business_unit_code
	)
*/
----------------------------------------------------------------------------
-- Insert new kpis
INSERT INTO mart.scorecard_kpi
	(
	scorecard_id, 
	kpi_id, 
	business_unit_id,
	datasource_id,
	datasource_update_date
	)
SELECT 
	scorecard_id			= ds.scorecard_id, 
	kpi_name				= dk.kpi_id, 
	business_unit_id		= bu.business_unit_id,
	datasource_id			= sk.datasource_id,
	datasource_update_date	= sk.datasource_update_date
FROM
	Stage.stg_scorecard_kpi sk
INNER JOIN 
	mart.dim_scorecard ds 
ON
	ds.scorecard_code = sk.scorecard_code
INNER JOIN 
	mart.dim_kpi dk 
ON
	dk.kpi_code = sk.kpi_code
INNER JOIN
	mart.dim_business_unit bu
ON
	sk.business_unit_code = bu.business_unit_code
ORDER BY ds.scorecard_id, dk.kpi_id

GO

