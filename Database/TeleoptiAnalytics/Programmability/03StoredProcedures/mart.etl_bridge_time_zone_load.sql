IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_time_zone_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_time_zone_load]
GO


-- =============================================
-- Author:		ChLu
-- Create date: 2008-01-30
-- Update date:2009-02-11
-- 2009-02-11 New Mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- Description:	Loads time zones from stg_time_zone_bridge 
--				to bridge_time_zone
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_time_zone_load] 
	
@start_date smalldatetime,
@end_date smalldatetime
	
AS
----------------------------------------------------------------------------------
DECLARE @start_date_id	INT
DECLARE @end_date_id	INT

DECLARE @max_date smalldatetime
DECLARE @min_date smalldatetime

SELECT  
	@max_date= max(date),
	@min_date= min(date)
FROM
	Stage.stg_time_zone_bridge
 
SET	@start_date = CASE WHEN @min_date > @start_date THEN @min_date ELSE @start_date END
SET	@end_date	= CASE WHEN @max_date < @end_date	THEN @max_date ELSE @end_date	END

SET	@start_date = convert(smalldatetime,floor(convert(decimal(18,8),@start_date )))
SET @end_date	= convert(smalldatetime,floor(convert(decimal(18,8),@end_date )))

SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @start_date = date_date)
SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @end_date = date_date)

IF @end_date_id is null
BEGIN
SET @end_date_id = (SELECT max(date_id) FROM mart.dim_date)
END


-----------------------------------------------------------------------------------
-- Delete rows

DELETE FROM mart.bridge_time_zone  WHERE date_id between @start_date_id AND @end_date_id


----------------------------------------------------------------------------
-- insert into bridge_time_zone

INSERT INTO mart.bridge_time_zone
	(
	date_id, 
	interval_id, 
	time_zone_id, 
	local_date_id, 
	local_interval_id, 
	datasource_id, 
	insert_date, 
	update_date
	)
SELECT
	date_id				= d.date_id, 
	interval_id			= stg.interval_id, 
	time_zone_id		= tz.time_zone_id, 
	local_date_id		= ld.date_id,
	local_interval_id	= stg.local_interval_id, 
	datasource_id		= stg.datasource_id, 
	insert_date			= getdate(), 
	update_date			= getdate()
FROM
	Stage.stg_time_zone_bridge stg
JOIN
	mart.dim_date d
ON
	stg.date = d.date_date
JOIN
	mart.dim_date ld
ON
	stg.local_date = ld.date_date
JOIN
	mart.dim_time_zone tz
ON
	stg.time_zone_code = tz.time_zone_code
WHERE 
	d.date_id BETWEEN @start_date_id and @end_date_id

GO

