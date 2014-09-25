IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_hourly_availability_intraday_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_hourly_availability_intraday_load]
GO

-- =============================================
-- Author:		<Ola>
-- Create date: <2013-05-21>
-- Description:	<Load from [stage].[stg_hourly_availability]>
-- =============================================
-- exec mart.[etl_fact_hourly_availability_intraday_load] '928DD0BC-BF40-412E-B970-9B5E015AADEA'
CREATE PROCEDURE [mart].[etl_fact_hourly_availability_intraday_load]
@business_unit_code uniqueidentifier,
@scenario_code uniqueidentifier
	
AS
SET NOCOUNT ON
DECLARE @business_unit_id int

if (select count(*)
	from mart.dim_scenario
	where business_unit_code = @business_unit_code
	and scenario_code = @scenario_code
	) <> 1
BEGIN
	DECLARE @ErrorMsg nvarchar(4000)
	SELECT @ErrorMsg  = 'This is not a default scenario, or multiple default scenarios exists!'
	RAISERROR (@ErrorMsg,16,1)
	RETURN 0
END

SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

SET NOCOUNT OFF
----------------
--Remove old data
----------------

-- Delete rows from stage
DELETE f
FROM Stage.stg_hourly_availability stg
INNER JOIN
	mart.dim_person		dp
ON
	stg.person_code		=			dp.person_code
	AND --trim persons
		(
				(stg.restriction_date	>= dp.valid_from_date)

			AND
				(stg.restriction_date < dp.valid_to_date)
		)
INNER JOIN mart.dim_date dd
	ON dd.date_date = stg.restriction_date
INNER JOIN mart.dim_scenario ds
	ON stg.scenario_code = ds.scenario_code
	AND stg.scenario_code = @scenario_code
INNER JOIN mart.fact_hourly_availability f
	ON f.date_id = dd.date_id
	AND f.scenario_id = ds.scenario_id
	AND f.person_id = dp.person_id
WHERE stg.business_unit_code = @business_unit_code


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
LEFT JOIN mart.dim_person dp
ON
	stg.person_code	= dp.person_code
	AND	stg.restriction_date BETWEEN dp.valid_from_date AND dp.valid_to_date  --Is person valid in this range
	AND --trim
		(
				(stg.restriction_date	>= dp.valid_from_date)

			AND
				(stg.restriction_date < dp.valid_to_date)
		)
 
WHERE dp.to_be_deleted = 0


GO


