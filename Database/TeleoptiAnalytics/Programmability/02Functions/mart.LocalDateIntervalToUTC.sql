/****** Object:  UserDefinedFunction [mart].[LocalDateIntervalToUTC]    Script Date: 10/27/2009 14:43:46 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[LocalDateIntervalToUTC]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[LocalDateIntervalToUTC]
GO



/****** Object:  UserDefinedFunction [mart].[LocalDateIntervalToUTC]    Script Date: 10/27/2009 14:43:46 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		DJ
-- Create date: 2009-10-22
-- Description: Converts a FromDate + ToDate to valid UTC date IDs + interval IDs
-- Change log:
--				20xx-xx-xx
-- =============================================


CREATE FUNCTION [mart].[LocalDateIntervalToUTC](@local_date_from smalldatetime, @local_date_to smalldatetime, @time_zone_id int)
RETURNS @LocalDateIntervalToUTC TABLE (
	from_date_id		int NOT NULL,
	from_interval_id	int NOT NULL,
	to_date_id		int NOT NULL,
	to_interval_id	int NOT NULL
	)
AS
BEGIN	


	/*
	declare @local_date_from smalldatetime,@local_date_to smalldatetime, @time_zone_id int
	set @local_date_from = '2003-10-05'
	set @local_date_to = '2010-12-22'
	set @time_zone_id = 2
	SELECT MAX(date_id) FROM mart.bridge_time_zone
	SELECT * from mart.dim_date where date_id=4009
	SELECT @local_date_to 

	*/

	--------
	--DECLARE
	--------
	DECLARE @local_start_interval int
	DECLARE @local_stop_interval int

	DECLARE @from_date_id int
	DECLARE @from_interval_id int
	DECLARE @to_date_id int
	DECLARE @to_interval_id int

	--------
	--INIT
	--------
	--Include all intervals from midnight on the the start date 
	SET @local_date_from = @local_date_from
	SET @local_start_interval = 0

	--If @local_date_to is too far in the future, use max from mart.dim_date
	SELECT @local_date_to = CASE WHEN MAX(dd.date_date) < @local_date_to THEN MAX(dd.date_date) ELSE @local_date_to END FROM mart.dim_date dd WHERE date_id <> -2

	--Include all intervals up until midnight on the end date
	SELECT @local_stop_interval=MAX(di.interval_id) FROM mart.dim_interval di

	--------
	--CODE
	--------
	--Get Start date and interval (UTC)
	SELECT @from_date_id = btz.date_id, @from_interval_id = btz.interval_id
	FROM mart.dim_date dd
	INNER JOIN mart.bridge_time_zone btz
	ON dd.date_id = btz.local_date_id
	INNER JOIN mart.dim_interval di
	ON di.interval_id = btz.local_interval_id
	WHERE (dd.date_date = @local_date_from AND di.interval_id = @local_start_interval)
	AND btz.time_zone_id = @time_zone_id

	--Get End date and interval (UTC)
	SELECT @to_date_id = btz.date_id, @to_interval_id = btz.interval_id
	--SELECT btz.*,dd.*
	FROM mart.dim_date dd
	INNER JOIN mart.bridge_time_zone btz
	ON dd.date_id = btz.local_date_id
	INNER JOIN mart.dim_interval di
	ON di.interval_id = btz.local_interval_id
	WHERE (dd.date_date = @local_date_to AND di.interval_id = @local_stop_interval)
	AND btz.time_zone_id = @time_zone_id

	--------
	--Return (UTC)
	--------
	INSERT INTO @LocalDateIntervalToUTC
	SELECT @from_date_id, @from_interval_id, @to_date_id, @to_interval_id

	RETURN
END

GO


