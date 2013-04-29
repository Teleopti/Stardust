IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_preference_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_preference_load]
GO

-- =============================================
-- Author:		KJ
-- Create date: 2008-11-19
-- Description:	Write schedule preferences from staging table 'stg_schedule_preference'
--				to data mart table 'fact_schedule_preference'.
-- Updates:		2009-01-16
--				2009-02-09 Stage moved to mart db, removed view KJ
--				2009-01-16 Changed fields in stg table KJ
--				2008-12-01 Changed Delete statement for multi BU. KJ
--				2009-12-09 Some intermediate hardcoded stuff on day_off_id, Henry Greijer and Jonas Nordh.
--				2010-10-12 #12055 - ETL - cant load preferences
--				2011-09-27 Fix start/end times = 0
--				2012-11-25 #19854 - PBI to add Shortname for DayOff.
--				2013-04-29 Added absence_id and must_haves in load KJ
-- Interface:	smalldatetime, with only datepart! No time allowed
-- =============================================
--exec mart.etl_fact_schedule_preference_load '2009-02-01','2009-02-17'

CREATE PROCEDURE [mart].[etl_fact_schedule_preference_load] 
@start_date smalldatetime,
@end_date smalldatetime,
@business_unit_code uniqueidentifier
WITH EXECUTE AS OWNER	
AS

DECLARE @start_date_id	INT
DECLARE @end_date_id	INT

--Declare
DECLARE @max_start_date smalldatetime
DECLARE @min_start_date smalldatetime

--init
SELECT  
	@max_start_date= max(restriction_date),
	@min_start_date= min(restriction_date)
FROM
	Stage.stg_schedule_preference
--select * from v_stg_schedule_preference
--Reset @start_date, @end_date to 
SET	@start_date = CASE WHEN @min_start_date > @start_date THEN @min_start_date ELSE @start_date END
SET	@end_date	= CASE WHEN @max_start_date < @end_date THEN @max_start_date ELSE @end_date	END

--There must not be any timevalue on the interface values, since that will mess things up around midnight!
--Consider: DECLARE @end_date smalldatetime;SET @end_date = '2006-01-31 23:59:30';SELECT @end_date
SET @start_date = CONVERT(smalldatetime,CONVERT(nvarchar(30), @start_date, 112)) --ISO yyyymmdd
SET @end_date	= CONVERT(smalldatetime,CONVERT(nvarchar(30), @end_date, 112))

--Not currently needed since we now delete on shift_starttime (instead of _id)
SET @start_date_id =	(SELECT date_id FROM dim_date WHERE @start_date = date_date)
SET @end_date_id	 =	(SELECT date_id FROM dim_date WHERE @end_date = date_date)

DECLARE @business_unit_id int
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

DELETE FROM mart.fact_schedule_preference
WHERE date_id between @start_date_id AND @end_date_id
AND business_unit_id = @business_unit_id


/*
DELETE FROM mart.fact_schedule_preference
WHERE date_id between @start_date_id AND @end_date_id
	AND business_unit_id = 
	(
		SELECT DISTINCT
			bu.business_unit_id
		FROM 
			Stage.stg_schedule_preference s
		INNER JOIN
			mart.dim_business_unit bu
		ON
			s.business_unit_code = bu.business_unit_code
	)
*/
--DELETE FROM mart.fact_schedule_preference WHERE date_id between @start_date_id AND @end_date_id

-----------------------------------------------------------------------------------
-- Insert rows

INSERT INTO mart.fact_schedule_preference
	(
	date_id, 
	interval_id, 
	person_id, 
	scenario_id, 
	preference_type_id, 
	shift_category_id, 
	day_off_id, 
	preferences_requested_count, 
	preferences_accepted_count, 
	preferences_declined_count, 
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
	preferences_requested_count	= 1, 
	preferences_accepted_count	= f.preference_accepted, --kolla hur vi g�r h�r
	preferences_declined_count	= f.preference_declined,  --kolla hur vi g�r h�r
	business_unit_id			= dp.business_unit_id, 
	datasource_id				= f.datasource_id, 
	datasource_update_date		= f.datasource_update_date, 
	must_haves					= ISNULL(must_have,0),
	dim_absence					= ISNULL(ab.absence_id,-1)
FROM 
	(
		SELECT * FROM Stage.stg_schedule_preference WHERE convert(smalldatetime,floor(convert(decimal(18,8),restriction_date ))) between @start_date AND @end_date
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
LEFT JOIN
	mart.dim_shift_category sc
ON
	f.shift_category_code = sc.shift_category_code
INNER JOIN 
	mart.dim_business_unit bu
ON
	dp.business_unit_code = bu.business_unit_code
LEFT JOIN				--vi k�r tills vidare p� day_off_name som "primary key"
	mart.dim_day_off ddo
ON
	f.day_off_name = ddo.day_off_name  AND ddo.business_unit_id = @business_unit_id
LEFT JOIN
	mart.dim_absence ab
ON
	f.absence_code = ab.absence_code

--LEFT JOIN				--beh�ver inte denna om de s�tts h�rt
--	dim_preference_type dpt
--ON
--	dpt.preference_type_name=f.preference_type_name

GO
