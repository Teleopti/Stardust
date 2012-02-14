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
DECLARE @new_time_zone_found bit
DECLARE @period_start_date smalldatetime, @period_end_date smalldatetime, @tz_period_end_date smalldatetime
DECLARE @intervals_per_day INT, @is_inconsistent BIT

SELECT 
	@new_time_zone_found = count(time_zone_code )
FROM 
	mart.dim_time_zone
WHERE time_zone_id NOT IN	(
							SELECT 
								DISTINCT time_zone_id 
							FROM 
								mart.bridge_time_zone
							)

SELECT
	@period_start_date = min(date_date),
	@period_end_date = max(date_date)
FROM
	mart.dim_date
WHERE
	date_id > -1
	

SELECT @intervals_per_day = COUNT(*) FROM mart.dim_interval

IF EXISTS (
			SELECT COUNT(date_id) as Intervals,time_zone_id,date_id
			FROM mart.bridge_time_zone
			WHERE date_id <> (SELECT MAX(date_id) FROM mart.bridge_time_zone) --do not care about last day
			AND date_id <> (SELECT MIN(date_id) FROM mart.bridge_time_zone) --Do not care about first day
			GROUP BY time_zone_id,date_id
			HAVING COUNT(date_id) <> @intervals_per_day
			)
	SET @is_inconsistent = 1
ELSE
	SET @is_inconsistent = 0


-- When no new time zone was found and data is consistent we need to adapt the start of the period.
IF (@new_time_zone_found = 0) AND (@is_inconsistent = 0)
BEGIN
	SELECT
		@tz_period_end_date = max(dd.date_date)
	FROM
		mart.bridge_time_zone btz
	INNER JOIN
		mart.dim_date dd
	ON
		btz.date_id = dd.date_id

--select @tz_period_end_date, @period_end_date

	IF @tz_period_end_date IS NOT NULL
	BEGIN
		--IF @tz_period_end_date < @period_end_date
		--	SET @tz_period_end_date = dateadd(d, 1, @tz_period_end_date)
		SET @period_start_date = @tz_period_end_date
		IF @period_start_date = @period_end_date
		BEGIN
			SELECT @period_start_date = null, @period_end_date = null
		END
	END
END

SELECT @period_start_date as period_start_date, @period_end_date as period_end_date

GO

