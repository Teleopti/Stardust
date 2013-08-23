IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_agent_queue_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_agent_queue_load]
GO


-- =============================================
-- Author:		KJ
-- Create date: 2008-04-09
-- Description:	Loads fact_agent_queue. Obs! Interval = dim_interval_id.
-- Last Modified: 20080414 Added column workload_id KJ
-- Last Modified: 20080905 Changed LEFT to INNER JOIN ON dim_acd_login
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 2009-02-11 New mart schema KJ
-- 2009-09-01 Load only known queues (INNER JOIN dim_queue) DJ
-- 2010-03-31 Added @datasource_id as where filter on Time_zone.get DJ
-- 2011-01-29 Performance DJ
-- 2011-07-20 Dual Agg DJ
-- =============================================
--EXEC [mart].[etl_fact_agent_queue_load]  '2009-01-01','2009-03-01',2
CREATE PROCEDURE [mart].[etl_fact_agent_queue_load] 
@start_date smalldatetime,
@end_date smalldatetime,
@datasource_id int
	
AS
DECLARE @internal bit
DECLARE @sqlstring nvarchar(4000)
SET @sqlstring = ''

--------------------------------------------------------------------------
--If we get All = -2 loop existing log objects and call this SP in a cursor for each log object
--------------------------------------------------------------------------
IF @datasource_id = -2 --All
BEGIN
	DECLARE DataSouceCursor CURSOR FOR
	SELECT datasource_id FROM mart.sys_datasource WHERE datasource_id NOT IN (-1,1) AND time_zone_id IS NOT NULL AND inactive = 0 
	OPEN DataSouceCursor

	FETCH NEXT FROM DataSouceCursor INTO @datasource_id
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXEC [mart].[etl_fact_agent_queue_load] @start_date, @end_date, @datasource_id
		FETCH NEXT FROM DataSouceCursor INTO @datasource_id
	END
	CLOSE DataSouceCursor
	DEALLOCATE DataSouceCursor
END
ELSE
BEGIN
	--is this an internal or external datasource?
	SELECT @internal = internal FROM mart.sys_datasource WHERE datasource_id = @datasource_id

	--Create system mindate
	DECLARE @UtcNow as smalldatetime
	SELECT @UtcNow=CAST(getutcdate() as smalldatetime)

	DECLARE @start_date_id	INT
	DECLARE @end_date_id	INT

	DECLARE @max_date smalldatetime
	DECLARE @min_date smalldatetime

	DECLARE @time_zone_id smallint
	SELECT 
		@time_zone_id = ds.time_zone_id
	FROM
		Mart.sys_datasource ds
	WHERE 
		ds.datasource_id= @datasource_id

	SET	@start_date = convert(smalldatetime,floor(convert(decimal(18,4),@start_date )))
	SET @end_date	= convert(smalldatetime,floor(convert(decimal(18,4),@end_date )))

	SET @start_date_id	=	(SELECT date_id FROM mart.dim_date WHERE @start_date = date_date)
	SET @end_date_id	=	(SELECT date_id FROM mart.dim_date WHERE @end_date = date_date)


	--------------
	-- Delete rows
	--------------
	DELETE FROM mart.fact_agent_queue  WHERE local_date_id between @start_date_id AND @end_date_id and datasource_id = @datasource_id

	--------------
	-- Insert rows
	--------------
	--perpare
	SELECT @sqlstring = 'INSERT INTO mart.fact_agent_queue
		(
		date_id, 
		interval_id, 
		queue_id, 
		acd_login_id, 
		local_date_id, 
		local_interval_id, 
		talk_time_s, 
		after_call_work_time_s, 
		answered_calls, 
		transfered_calls, 
		datasource_id, 
		insert_date, 
		update_date, 
		datasource_update_date
		)
	SELECT
		date_id					= bridge.date_id, 
		interval_id				= bridge.interval_id, 
		queue_id				= q.queue_id, 
		acd_login_id			= a.acd_login_id, 
		local_date_id			= d.date_id,
		local_interval_id		= stg.interval, 
		talk_time_s				= talking_call_dur, 
		after_call_work_time_s	= wrap_up_dur, 
		answered_calls			= answ_call_cnt, 
		transfered_calls		= transfer_out_call_cnt,
		datasource_id			= a.datasource_id, 
		insert_date				= getdate(), 
		update_date				= getdate(), 
		datasource_update_date	= '''+ CAST(@UtcNow as nvarchar(20))+'''
		FROM
		(SELECT * FROM '+ 
		CASE @internal
			WHEN 0 THEN '	mart.v_agent_logg agg'
			WHEN 1 THEN '	dbo.agent_logg agg'
			ELSE NULL --Fail fast
		END
		+ ' WHERE date_from between '''+ CAST(@start_date as nvarchar(20))+''' and '''+ CAST(@end_date as nvarchar(20))+''') stg
	INNER JOIN
		mart.dim_date		d
	ON
		stg.date_from	= d.date_date
	JOIN
		(
	SELECT
		date_id		= min(date_id),
		interval_id	= min(interval_id),
		time_zone_id, 
		local_date_id,
		local_interval_id
	FROM 
		mart.bridge_time_zone 
	WHERE
		time_zone_id	= '+CAST(@time_zone_id as nvarchar(10))+'
	GROUP BY
		time_zone_id, 
		local_date_id,
		local_interval_id
	) bridge
	ON
		d.date_id		= bridge.local_date_id		AND
		stg.interval	= bridge.local_interval_id	
	INNER JOIN
		mart.dim_queue q
	ON
		q.queue_agg_id= stg.queue AND q.datasource_id = ' + CAST(@datasource_id as nvarchar(10)) +'
	JOIN 
		mart.dim_acd_login a
	ON a.acd_login_agg_id = stg.agent_id AND a.datasource_id = ' + CAST(@datasource_id as nvarchar(10))
	
	--Exec
	EXEC sp_executesql @sqlstring

END

GO


