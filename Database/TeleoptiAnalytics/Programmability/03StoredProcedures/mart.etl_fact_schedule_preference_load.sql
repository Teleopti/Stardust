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
--				2013-08-14 Change to use local agent date (instead of UTC)
--				2013-09-20  Removed check on min/maxdate in stage
-- Interface:	smalldatetime, with only datepart! No time allowed
-- =============================================
--exec mart.etl_fact_schedule_preference_load '2009-02-01','2009-02-17'

CREATE PROCEDURE [mart].[etl_fact_schedule_preference_load] 
@start_date smalldatetime,
@end_date smalldatetime,
@business_unit_code uniqueidentifier,
@debug bit = 0
AS

--enable if needed. see #27933
declare @OnlyDefaultScenario bit
set @OnlyDefaultScenario=0

--debug
declare @timeStat table (step int,thisstep_ms int,totalTime int)
declare @startTime datetime
declare @lastStep datetime
set @lastStep=getdate()
set @startTime=getdate()
declare @step int
set @step=0

if @debug=1
begin
	insert into @timeStat(step,totalTime,thisstep_ms)
	select @step,datediff(ms,@startTime,getdate()),datediff(ms,@lastStep,getdate())
	set @lastStep=getdate()
	set @step=@step+1
end

DECLARE @start_date_id	INT
DECLARE @end_date_id	INT

--There must not be any timevalue on the interface values, since that will mess things up around midnight!
--Consider: DECLARE @end_date smalldatetime;SET @end_date = '2006-01-31 23:59:30';SELECT @end_date
SET @start_date = CONVERT(smalldatetime,CONVERT(nvarchar(30), @start_date, 112)) --ISO yyyymmdd
SET @end_date	= CONVERT(smalldatetime,CONVERT(nvarchar(30), @end_date, 112))

--Not currently needed since we now delete on shift_starttime (instead of _id)
SET @start_date_id =	(SELECT date_id FROM dim_date WHERE @start_date = date_date)
SET @end_date_id	 =	(SELECT date_id FROM dim_date WHERE @end_date = date_date)

DECLARE @business_unit_id int
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

if @debug=1
begin
	insert into @timeStat(step,totalTime,thisstep_ms)
	select @step,datediff(ms,@startTime,getdate()),datediff(ms,@lastStep,getdate())
	set @lastStep=getdate()
	set @step=@step+1
end

DELETE FROM mart.fact_schedule_preference
WHERE date_id between @start_date_id AND @end_date_id
AND business_unit_id = @business_unit_id

if @debug=1
begin
	insert into @timeStat(step,totalTime,thisstep_ms)
	select @step,datediff(ms,@startTime,getdate()),datediff(ms,@lastStep,getdate())
	set @lastStep=getdate()
	set @step=@step+1
end

--Enable this section if Customer are OK with just default scenario
If @OnlyDefaultScenario=1
DELETE stg
FROM mart.dim_scenario s
INNER JOIN Stage.stg_schedule_preference stg
	ON s.scenario_code = stg.scenario_code
WHERE s.default_scenario <> 1

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
	interval_id					= 0, --we keep the column
	person_id					= p.person_id, 
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
	business_unit_id			= p.business_unit_id, 
	datasource_id				= f.datasource_id, 
	datasource_update_date		= MAX(f.datasource_update_date), 
	must_haves					= MAX(ISNULL(must_have,0)),
	dim_absence					= ISNULL(ab.absence_id,-1)
FROM 
	(
		SELECT * FROM Stage.stg_schedule_preference WHERE convert(smalldatetime,floor(convert(decimal(18,8),restriction_date ))) between @start_date AND @end_date
	) AS f
INNER JOIN mart.dim_person p
	on p.person_code = f.person_code
INNER JOIN mart.DimPersonLocalized(@start_date,@end_date)		dp
	ON	dp.person_id = p.person_id
	AND	f.restriction_date		BETWEEN		dp.valid_from_date_local	AND dp.valid_to_date_local --Is person valid in this range
LEFT JOIN
	mart.dim_date		dsd
ON
	f.restriction_date	= dsd.date_date
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
	p.business_unit_code = bu.business_unit_code
LEFT JOIN				--vi kör tills vidare på day_off_name som "primary key"
	mart.dim_day_off ddo
ON
	f.day_off_name = ddo.day_off_name  AND ddo.business_unit_id = @business_unit_id
LEFT JOIN
	mart.dim_absence ab
ON
	f.absence_code = ab.absence_code
GROUP BY dsd.date_id,p.person_id,ds.scenario_id, CASE 
										--Shift Category (standard) Preference
										WHEN ISNULL(f.StartTimeMinimum,'') + ISNULL(f.EndTimeMinimum,'') + ISNULL(f.StartTimeMaximum,'') + ISNULL(f.EndTimeMaximum,'') +  ISNULL(f.WorkTimeMinimum,'') + ISNULL(f.WorkTimeMaximum,'') = '' AND f.shift_category_code IS NOT NULL AND f.activity_code IS NULL THEN 1
										--Day Off Preference
										WHEN ISNULL(f.StartTimeMinimum,'') + ISNULL(f.EndTimeMinimum,'') + ISNULL(f.StartTimeMaximum,'') + ISNULL(f.EndTimeMaximum,'') +  ISNULL(f.WorkTimeMinimum,'') + ISNULL(f.WorkTimeMaximum,'') = '' AND f.day_off_name IS NOT NULL AND f.activity_code IS NULL THEN 2
										--Extended Preference
										WHEN f.StartTimeMinimum IS NOT NULL OR f.EndTimeMinimum IS NOT NULL OR f.StartTimeMaximum IS NOT NULL OR f.EndTimeMaximum IS NOT NULL OR f.WorkTimeMinimum IS NOT NULL OR f.WorkTimeMaximum IS NOT NULL OR f.activity_code IS NOT NULL THEN 3
										--Absence Preference
										WHEN ISNULL(f.StartTimeMinimum,'') + ISNULL(f.EndTimeMinimum,'') + ISNULL(f.StartTimeMaximum,'') + ISNULL(f.EndTimeMaximum,'') +  ISNULL(f.WorkTimeMinimum,'') + ISNULL(f.WorkTimeMaximum,'') = '' AND f.absence_code IS NOT NULL AND f.activity_code IS NULL THEN 4
								  END,
								   sc.shift_category_id, ddo.day_off_id,
	 f.preference_fulfilled, 	 f.preference_unfulfilled, p.business_unit_id, f.datasource_id, ab.absence_id

if @debug=1
begin
	insert into @timeStat(step,totalTime,thisstep_ms)
	select @step,datediff(ms,@startTime,getdate()),datediff(ms,@lastStep,getdate())
	set @lastStep=getdate()
	set @step=@step+1
end

if @debug=1
	select * from @timeStat order by step

GO
