IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_hourly_availability_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_hourly_availability_load]
GO

-- =============================================
-- Author:		<KJ>
-- Create date: <2013-05-20>
-- Description:	<Load from [stage].[stg_hourly_availability]>
-- =============================================
-- exec mart.etl_fact_hourly_availability_load '2013-01-01','2013-02-28', '928DD0BC-BF40-412E-B970-9B5E015AADEA'
CREATE PROCEDURE [mart].[etl_fact_hourly_availability_load]
@start_date smalldatetime,
@end_date smalldatetime,
@business_unit_code uniqueidentifier
	
AS
BEGIN
SET NOCOUNT ON
DECLARE @business_unit_id int
DECLARE @start_date_id int
DECLARE @end_date_id int

SET @start_date = CONVERT(smalldatetime,CONVERT(nvarchar(30), @start_date, 112)) --ISO yyyymmdd
SET @end_date	= CONVERT(smalldatetime,CONVERT(nvarchar(30), @end_date, 112))

SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @start_date = date_date)
SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @end_date = date_date)
/*Get business unit id*/
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

SET NOCOUNT OFF
----------------
--Remove old data
----------------
DELETE mart.fact_hourly_availability
WHERE business_unit_id = @business_unit_id
AND date_id BETWEEN @start_date_id AND @end_date_id

INSERT INTO mart.fact_hourly_availability
SELECT
date_id				= dlocal.date_id,
person_id			= ISNULL(dp.person_id,-1),
scenario_id			= ds.scenario_id,
available_time_m	= stg.available_time_m,
available_days		= 1,
scheduled_time_m	= scheduled_time_m,
scheduled_days		= scheduled,
business_unit_id	= bu.business_unit_id,
datasource_id		= stg.datasource_id
FROM stage.stg_hourly_availability stg
INNER JOIN mart.dim_date dLocal
	ON dLocal.date_date = CONVERT(smalldatetime,CONVERT(nvarchar(30), stg.restriction_date, 112))
INNER JOIN mart.dim_business_unit bu
	ON bu.business_unit_code = stg.business_unit_code
INNER JOIN mart.dim_scenario ds
	ON stg.scenario_code = ds.scenario_code
LEFT JOIN mart.dim_person dp WITH (NOLOCK)
ON
	stg.person_code	= dp.person_code
	AND	stg.restriction_date BETWEEN dp.valid_from_date AND dp.valid_to_date  --Is person valid in this range
	AND --trim
		(
				(stg.restriction_date	>= dp.valid_from_date)

			AND
				(stg.restriction_date < dp.valid_to_date)
		)
WHERE stg.restriction_date BETWEEN @start_date AND @end_date
AND dp.to_be_deleted = 0

END


GO


