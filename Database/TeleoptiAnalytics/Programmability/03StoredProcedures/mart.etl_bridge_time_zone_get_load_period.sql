IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_time_zone_get_load_period]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_time_zone_get_load_period]
GO

-- =============================================
-- Author:		JN
-- Create date: 2008-09-17
-- Update date:
--				2009-02-11 KJ: New Mart schema
--				2010-06-11 DJ: Added exclusion; Do not check consistence for first+last day
-- Description:	Return the date period that should be loaded into 
--              stg_time_zone_bridge and bridge_time_zone.
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_time_zone_get_load_period] 	
AS
SET NOCOUNT ON

DECLARE @period_start_date smalldatetime, @period_end_date smalldatetime
DECLARE @intervals_per_day INT

CREATE TABLE #bridge_time_zone_period(
		time_zone_id int, 
		time_zone_code nvarchar(50),
		period_start_date smalldatetime,
		period_end_date smalldatetime
		)

CREATE TABLE #max_date(
			time_zone_id int, 
			max_date smalldatetime)

CREATE TABLE #incons(
			intervals int, 
			time_zone_id int, 
			date_id int)

/*Get all time_zones*/
INSERT #bridge_time_zone_period(time_zone_id,time_zone_code,period_start_date,period_end_date)
SELECT dt.time_zone_id, dt.time_zone_code, NULL ,NULL
FROM 
	mart.dim_time_zone dt
WHERE dt.to_be_deleted = 0

/*Check min and max in dim_date*/
SELECT
	@period_start_date = min(date_date),
	@period_end_date = max(date_date)
FROM
	mart.dim_date
WHERE
	date_id > -1

/*Set last date in bridge as start_date and last date in dim_date as end_date*/
INSERT #max_date(time_zone_id,max_date)
SELECT btz.time_zone_id as time_zone_id,max(dd.date_date)
FROM mart.bridge_time_zone btz
INNER JOIN mart.dim_date dd
ON btz.date_id = dd.date_id
GROUP BY btz.time_zone_id

UPDATE #bridge_time_zone_period
SET period_start_date= #max_date.max_date, period_end_date=@period_end_date
FROM #bridge_time_zone_period 
INNER JOIN #max_date 
ON #max_date.time_zone_id=#bridge_time_zone_period.time_zone_id

/*Check for inconsistent time_zones*/
SELECT @intervals_per_day = COUNT(*) FROM mart.dim_interval

INSERT #incons(intervals,time_zone_id,date_id)
SELECT COUNT(date_id) as intervals,time_zone_id,date_id
FROM mart.bridge_time_zone
WHERE date_id <> (SELECT MAX(date_id) FROM mart.bridge_time_zone) --do not care about last day
AND date_id <> (SELECT MIN(date_id) FROM mart.bridge_time_zone) --Do not care about first day
GROUP BY time_zone_id,date_id
HAVING COUNT(date_id) <> @intervals_per_day

UPDATE #bridge_time_zone_period
SET period_start_date=NULL,
	period_end_date=NULL
FROM #bridge_time_zone_period
INNER JOIN #incons 
	ON #incons.time_zone_id=#bridge_time_zone_period.time_zone_id

/*Reload all new and inconsistent time_zones*/
UPDATE #bridge_time_zone_period
SET period_start_date=@period_start_date, period_end_date=@period_end_date
WHERE period_start_date IS NULL

/*Return all timezone with diff on date*/
SELECT time_zone_code, period_start_date, period_end_date 
FROM #bridge_time_zone_period
WHERE period_start_date<>period_end_date





GO

