IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_preference_intraday_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_preference_intraday_load]
GO

-- =============================================
-- Author:		David J
-- Create date: 2013-04-26
-- Description:	Write schedule preferences from staging table 'stg_schedule_preference'
--				to data mart table 'fact_schedule_preference'. Used only by ETL.Intrday
-- =============================================
--exec mart.etl_fact_schedule_preference_intraday_load '493E828B-D416-4628-AB10-990E7D268DB9','493E828B-D416-4628-AB10-990E7D268DB9'

CREATE PROCEDURE [mart].[etl_fact_schedule_preference_intraday_load]
@business_unit_code uniqueidentifier,
@scenario_code uniqueidentifier
AS
SET NOCOUNT ON

if (select count(*)
	from mart.dim_scenario
	where business_unit_code = @business_unit_code
	and scenario_code = @scenario_code
	) <> 1
BEGIN
	DECLARE @ErrorMsg nvarchar(4000)
	SELECT @ErrorMsg  = 'This is not a default scenario, or muliple default scenarios exists!'
	RAISERROR (@ErrorMsg,16,1)
	RETURN 0
END

DECLARE @business_unit_id int
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

DECLARE @scenario_id int
SELECT @scenario_id = scenario_id FROM mart.dim_scenario WHERE scenario_code= @scenario_code
 
-- Delete rows from stage
DELETE f
FROM Stage.stg_schedule_preference stg
INNER JOIN
	mart.dim_person		dp
ON
	stg.person_code		=			dp.person_code
	AND --trim persons
		(
				(stg.restriction_date	>= dp.valid_from_date)

			AND
				(stg.restriction_date <= dp.valid_to_date)
		)
INNER JOIN mart.dim_date dd
	ON dd.date_date = stg.restriction_date
INNER JOIN mart.dim_scenario ds
	ON stg.scenario_code = ds.scenario_code
	AND stg.scenario_code = @scenario_code
INNER JOIN mart.fact_schedule_preference f
	ON f.date_id = dd.date_id
	AND f.interval_id = stg.interval_id
	AND f.scenario_id = ds.scenario_id
WHERE stg.business_unit_code = @business_unit_code

INSERT INTO mart.fact_schedule_preference
	(
	date_id, 
	interval_id, 
	person_id, 
	scenario_id, 
	preference_type_id, 
	shift_category_id, 
	day_off_id, 
	preferences_requested, 
	preferences_fulfilled, 
	preferences_unfulfilled, 
	business_unit_id, 
	datasource_id, 
	datasource_update_date, 
	must_haves,
	absence_id
	)
	
-----------------------------------------------------
-- <Quick fix for #12055>
-----------------------------------------------------
--Duplicate prefs exist on some agents. Differs only on MustHave (rows for both 0 AND 1 exists)
--Added: DISTINCT
--By design; it seems possible to have multple preferences per day
--ETL-tool can't handle this!

SELECT DISTINCT
	date_id						= dsd.date_id, 
	interval_id					= di.interval_id, 
	person_id					= dp.person_id, 
	scenario_id					= ds.scenario_id, 
	preference_type_id			= CASE 
										--Shift Category (standard) Preference
										WHEN ISNULL(f.StartTimeMinimum,'') + ISNULL(f.EndTimeMinimum,'') + ISNULL(f.StartTimeMaximum,'') + ISNULL(f.EndTimeMaximum,'') +  ISNULL(f.WorkTimeMinimum,'') + ISNULL(f.WorkTimeMaximum,'') = '' AND f.shift_category_code IS NOT NULL AND f.activity_code IS NULL THEN 1
										--Day Off Preference
										WHEN ISNULL(f.StartTimeMinimum,'') + ISNULL(f.EndTimeMinimum,'') + ISNULL(f.StartTimeMaximum,'') + ISNULL(f.EndTimeMaximum,'') +  ISNULL(f.WorkTimeMinimum,'') + ISNULL(f.WorkTimeMaximum,'') = '' AND f.day_off_name IS NOT NULL AND f.activity_code IS NULL THEN 2
										--Extended Preference
										WHEN f.StartTimeMinimum IS NOT NULL OR f.EndTimeMinimum IS NOT NULL OR f.StartTimeMaximum IS NOT NULL OR f.EndTimeMaximum IS NOT NULL OR f.WorkTimeMinimum IS NOT NULL OR f.WorkTimeMaximum IS NOT NULL OR f.activity_code IS NOT NULL THEN 3
										--Absence Preference
										WHEN ISNULL(f.StartTimeMinimum,'') + ISNULL(f.EndTimeMinimum,'') + ISNULL(f.StartTimeMaximum,'') + ISNULL(f.EndTimeMaximum,'') +  ISNULL(f.WorkTimeMinimum,'') + ISNULL(f.WorkTimeMaximum,'') = '' AND f.absence_code IS NOT NULL AND f.activity_code IS NULL THEN 4
								  END,
	shift_category_id			= isnull(sc.shift_category_id,-1), 
	day_off_id					= isnull(ddo.day_off_id,-1),
	preferences_requested	= 1, 
	preferences_fulfilled	= f.preference_fulfilled, --kolla hur vi gör här
	preferences_unfulfilled	= f.preference_unfulfilled,  --kolla hur vi gör här
	business_unit_id			= dp.business_unit_id, 
	datasource_id				= f.datasource_id, 
	datasource_update_date		= f.datasource_update_date, 
	must_haves					= ISNULL(must_have,0),
	dim_absence					= ISNULL(ab.absence_id,-1)
FROM 
	(
		SELECT * FROM Stage.stg_schedule_preference
	) AS f
JOIN
	mart.dim_person		dp
ON
	f.person_code		=			dp.person_code	AND
	f.restriction_date		BETWEEN		dp.valid_from_date	AND dp.valid_to_date  --Is person valid in this range
LEFT JOIN
	mart.dim_date		dsd
ON
	f.restriction_date	= dsd.date_date
LEFT JOIN				--kommer denna att ta bort sen och bli dateonly i agentens tidszon???
	mart.dim_interval	di
ON
	f.interval_id = di.interval_id  --Fix By David: start_interval_id => interval_id
LEFT JOIN
	mart.dim_scenario	ds
ON
	f.scenario_code = ds.scenario_code
	and ds.scenario_id=@scenario_id
LEFT JOIN
	mart.dim_shift_category sc
ON
	f.shift_category_code = sc.shift_category_code
INNER JOIN 
	mart.dim_business_unit bu
ON
	dp.business_unit_code = bu.business_unit_code
LEFT JOIN				--vi kör tills vidare på day_off_name som "primary key"
	mart.dim_day_off ddo
ON
	f.day_off_name = ddo.day_off_name  AND ddo.business_unit_id = @business_unit_id
LEFT JOIN
	mart.dim_absence ab
ON
	f.absence_code = ab.absence_code

GO
