IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_agent_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_agent_load]
GO


-- =============================================
-- Author:		KJ
-- Create date: 2008-04-09
-- Description:	Loads fact_agent from agent_logg. 
-- Agent values not bound to queue.
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 2009-05-25 Changed from MIN to MAX on values from agent_logg KJ
-- 2011-01-29
-- 2011-07-20 Dual Agg DJ
-- =============================================
--EXEC [mart].[etl_fact_agent_load] '2009-01-01','2009-03-01',1
CREATE PROCEDURE [mart].[etl_fact_agent_load] 
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
		EXEC [mart].[etl_fact_agent_load] @start_date, @end_date, @datasource_id
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
	----------------------------------------------------------------------------------
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
	DELETE FROM mart.fact_agent  WHERE local_date_id between @start_date_id AND @end_date_id and datasource_id = @datasource_id

	--------------
	-- Insert rows
	--------------
	-- Same agent_id can occur on several intervals bound to different queues. 
	-- Important is to get the distinct(min/max) value.
	
	--perpare
	SELECT @sqlstring = 'INSERT INTO mart.fact_agent
		(
		date_id, 
		interval_id, 
		acd_login_id, 
		local_date_id, 
		local_interval_id, 
		ready_time_s,
		logged_in_time_s,
		not_ready_time_s,
		idle_time_s,
		direct_outbound_calls,
		direct_outbound_talk_time_s,
		direct_incoming_calls,
		direct_incoming_calls_talk_time_s,
		admin_time_s,
		datasource_id, 
		insert_date, 
		update_date, 
		datasource_update_date
		)
	SELECT
		date_id						= MIN(bridge.date_id), 
		interval_id					= MIN(bridge.interval_id), 
		acd_login_id				= a.acd_login_id, 
		local_date_id				= d.date_id,
		local_interval_id			= stg.interval, 
		ready_time_s				= MAX(avail_dur),
		logged_in_time_s			= MAX(tot_work_dur),
		not_ready_time_s			= MAX(pause_dur),
		idle_time_s					= MAX(wait_dur),
		direct_outbound_calls		= SUM(direct_out_call_cnt),
		direct_outbound_talk_time_s = SUM(direct_out_call_dur),
		direct_incoming_calls		= SUM(direct_in_call_cnt),
		direct_incoming_calls_talk_time_s = SUM(direct_in_call_dur),
		admin_time_s				= MAX(admin_dur),
		datasource_id				= a.datasource_id, 
		insert_date					= getdate(), 
		update_date					= getdate(), 
		datasource_update_date		= '''+ CAST(@UtcNow as nvarchar(20))+'''
	FROM '+ 
		CASE @internal
			WHEN 0 THEN '	mart.v_agent_logg stg'
			WHEN 1 THEN '	dbo.agent_logg stg'
			ELSE NULL --Fail fast
		END
		+ ' 
	INNER JOIN 
		mart.dim_date d 
	ON 
		stg.date_from	= d.date_date
	INNER JOIN 
		mart.bridge_time_zone bridge 
	ON 
		d.date_id	= bridge.local_date_id	
		AND	stg.interval= bridge.local_interval_id
	INNER JOIN 
		mart.dim_acd_login a 
	ON 
		a.acd_login_agg_id = stg.agent_id
	WHERE bridge.time_zone_id = '+CAST(@time_zone_id as nvarchar(10))+'
	AND date_from between '''+ CAST(@start_date as nvarchar(20))+''' and '''+ CAST(@end_date as nvarchar(20))+'''
	AND a.datasource_id = ' + CAST(@datasource_id as nvarchar(10)) +'
	GROUP BY a.acd_login_id,d.date_id,stg.interval,a.datasource_id'

	--Exec
	EXEC sp_executesql @sqlstring

END

GO


