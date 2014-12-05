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
AS
SET NOCOUNT ON
IF (select count(*) from stage.stg_time_zone_bridge)=0
	return 0

DELETE mart
FROM  mart.bridge_time_zone mart
JOIN
(SELECT d.date_id, stg.interval_id,tz.time_zone_id
FROM stage.stg_time_zone_bridge stg
INNER JOIN
	mart.dim_date d
ON
	stg.date = d.date_date
INNER JOIN
	mart.dim_time_zone tz
ON 
	stg.time_zone_code=tz.time_zone_code)stage
	ON stage.date_id=mart.date_id 
	AND stage.interval_id=mart.interval_id 
	AND stage.time_zone_id=mart.time_zone_id


----------------------------------------------------------------------------
/*insert bridge_time_zone*/
SET NOCOUNT OFF
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

GO

