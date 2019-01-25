IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_insert_agent_logg]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_insert_agent_logg]
GO


/*************************************/
/* Modified by David P 020130 */
/*************************************/
CREATE PROCEDURE [dbo].[p_insert_agent_logg]
/*
Micke D 2004-02-27 Added 10 minutes intervals on 30 minutes logging
Micke D 2004-08-19 Added 10 minute intervals on 60 minute logging
Micke D 2004-08-19 Added 15 minute intervals on 60 minute logging
henrikl 2009-04-16 Various bug fixes (some not tested), debug mode for simple 
	error checking of aggregated data etc. Changes marked with: --090416
Magnus K 2011-01-19 removed scorecard
Magnus K 2014-07-23 Added 10 minutes intervals on 15 mins logging
*/
@log_object_id int,
@start_date smalldatetime = '1900-01-01',
@end_date smalldatetime = '1900-01-01',
@rel_start_date int =0,
@rel_start_int int=0,
--090416
@debug int=0
AS

DECLARE @last_logg_date smalldatetime,
@last_logg_interval int,
@minutes_per_interval int,
@interval_per_hour int,
@CTI_interval_per_hour int,
@CTI_minutes_per_interval int,
@last_logg_date_time datetime,
@stop_date smalldatetime,
@acd_type int,
@int_per_day int

SET NOCOUNT ON

/*
@start_date used to re-aggregte _from_ a specific date
@end_date used to re-aggregte _to_ a specific date
@rel_start_date used when you need to pickup a date window backwards in time. e.g use -1 for going back 1 day
@rel_start_int used when you need a slinding interval windows backwards in time. e.g use -4 to step back 4 instervals
*/
/********************************************************/
/* Fetch latest log date and interval                   */
/********************************************************/
SELECT @last_logg_date = date_value , @last_logg_interval = int_value
FROM log_object_detail 
WHERE log_object_id = @log_object_id
AND detail_id = 2

--090416
IF NOT EXISTS (SELECT 1 FROM ccc_system_info)
BEGIN
	SELECT 'V7: TeleoptiCCCAgg..ccc_system_info.int_value needs to be updated with the same value as
	intervals_per_day in log_object.'
	INSERT INTO ccc_system_info (id, [desc])
	VALUES (1, 'CCC intervals per day')
END

SELECT @stop_date = dateadd(month,1,@last_logg_date)
SELECT @int_per_day= int_value
FROM ccc_system_info 
WHERE [id]=1

/* Calculate @minutes_per_interval and @interval_per_hour*/
SELECT @minutes_per_interval = (1440/int_value) 
FROM ccc_system_info 
WHERE [id]=1

SELECT @interval_per_hour = 60/@minutes_per_interval

SELECT @CTI_minutes_per_interval = (1440/intervals_per_day)
FROM log_object 
WHERE log_object_id = @log_object_id

SELECT @CTI_interval_per_hour = 60/@CTI_minutes_per_interval
SELECT @last_logg_date_time = dateadd(minute,@last_logg_interval*@minutes_per_interval,@last_logg_date)


IF ( @start_date > '1970-01-01' ) 
BEGIN
	SELECT @last_logg_date = @start_date
	SELECT @last_logg_interval = 0
	IF ( @end_date > '1970-01-01' )
	BEGIN
		SELECT @stop_date = @end_date
	END
END
IF ( @rel_start_date <> 0 )
BEGIN
	SELECT @last_logg_date = dateadd ( day,@rel_start_date,@last_logg_date)
END
IF ( @rel_start_int <> 0 )
BEGIN
	IF (@rel_start_int + @last_logg_interval >= 0)
	BEGIN

		IF  (@rel_start_int + @last_logg_interval < @int_per_day - 1)
		BEGIN
			/* the interval is within the same day, just add */
			SELECT @last_logg_interval = @last_logg_interval + @rel_start_int
		END
		ELSE IF  (@rel_start_int + @last_logg_interval >= @int_per_day)
		BEGIN
			/* the interval is on the next date. Add one day, switch interval value */
			SELECT @last_logg_date = dateadd ( day,1,@last_logg_date)
			SELECT @last_logg_interval = (@last_logg_interval + @rel_start_int) - (@int_per_day - 1)
		END 
	END 
	ELSE
	BEGIN
		/* The interval is on "yesterday" */
		declare @currentLogtime smalldatetime
		select @currentLogtime=DATEADD(minute,@CTI_minutes_per_interval*@last_logg_interval,@last_logg_date)
		
		select @last_logg_date=dateadd(MINUTE,@CTI_minutes_per_interval*@rel_start_int,@currentLogtime)
		select @last_logg_interval=DATEPART(HOUR,@last_logg_date)*@CTI_interval_per_hour+DATEPART(MINUTE,@last_logg_date)/@CTI_minutes_per_interval
		
		select @last_logg_date=CONVERT(varchar(10),@last_logg_date,120)
		
		
		--SELECT @last_logg_date = dateadd ( day,-1,@last_logg_date)
		--SELECT @last_logg_interval = (@int_per_day - 1) - (@last_logg_interval - @rel_start_int)
	END 
END

--reset the @last_logg_date_time according to the above #48758
SELECT @last_logg_date_time = dateadd(minute,@last_logg_interval*@minutes_per_interval,@last_logg_date)

SELECT @acd_type=acd_type_id FROM log_object WHERE log_object_id = @log_object_id
/*********************************************************************************/
/* Adjust for different time zones between log_object and T-CCC?      */
/* If so, last logg interval might need adjustment.                                 */
/*********************************************************************************/
DECLARE @max_add_hours int

SELECT @max_add_hours=max(add_hours) FROM log_object_add_hours
WHERE log_object_id = @log_object_id

IF @max_add_hours IS NOT NULL
BEGIN
	SELECT @last_logg_date = convert(smalldatetime,	convert(varchar(10), dateadd(minute,@minutes_per_interval*(@last_logg_interval-@max_add_hours*@interval_per_hour),@last_logg_date),120))
	SELECT @last_logg_interval = @interval_per_hour*datepart(hour,dateadd(minute,@minutes_per_interval*(@last_logg_interval-@max_add_hours*@interval_per_hour),'1971-01-01'))
					+datepart(minute,dateadd(minute,@minutes_per_interval*(@last_logg_interval-@max_add_hours*@interval_per_hour),'1971-01-01'))/@minutes_per_interval

	SELECT @last_logg_date_time = dateadd(minute,@last_logg_interval*@minutes_per_interval,@last_logg_date)
END
/**********************************************************************/
/* Create a temporary table wich we will add data to	*/
/**********************************************************************/
CREATE TABLE #tmp_alogg (
	queue int NOT NULL,
	date_from smalldatetime NOT NULL ,
	interval int NOT NULL,
	agent_id int NOT NULL, 
	agent_name nvarchar(100) NULL,
	avail_dur int NULL,
	tot_work_dur int NULL,
	talking_call_dur int NULL,	
	pause_dur int NULL,
	wait_dur int NULL,
	wrap_up_dur int NULL,
	answ_call_cnt int NULL,
	direct_out_call_cnt int NULL,
	direct_out_call_dur int NULL,
	direct_in_call_cnt int NULL,
	direct_in_call_dur int NULL,
	transfer_out_call_cnt int NULL,
	admin_dur int NULL)



/***************************************************************/
/* Execute the correct procedure for this log object	   */
/***************************************************************/
DECLARE @schema varchar(100)
DECLARE @proc_name varchar(100)
DECLARE @logdb_name varchar(100)

SELECT
	@logdb_name=logdb_name
FROM log_object
WHERE log_object_id = @log_object_id

SELECT
	@schema=SUBSTRING(proc_name, 1, CASE CHARINDEX('.', proc_name) WHEN 0 THEN LEN(proc_name) ELSE CHARINDEX('.', proc_name)-1 END),
	@proc_name=SUBSTRING(proc_name, CASE CHARINDEX('.', proc_name) WHEN 0 THEN LEN(proc_name)+1 ELSE CHARINDEX('.', proc_name)+1 END, 1000)
FROM log_object_detail
WHERE log_object_id = @log_object_id
AND detail_id = 2

If @proc_name='' --no schema provided in log_object_detail
BEGIN
	SET @proc_name=@schema
	SET @schema='dbo'
END

DECLARE @txt varchar(8000)
SELECT @txt = '['+@logdb_name+'].['+@schema+'].['+@proc_name+']
'+convert(varchar(5),@log_object_id)+',
'+''''+convert(varchar(10),@last_logg_date,120)+''''+',
'+convert(varchar(5),@last_logg_interval)+',
'+convert(varchar(5),@minutes_per_interval)+',
'+convert(varchar(5),@interval_per_hour)+',
'+convert(varchar(5),@CTI_interval_per_hour)+',
'+convert(varchar(5),@CTI_minutes_per_interval)+',

'+''''+convert(varchar(20),@last_logg_date_time,120)+''''+',
'+''''+convert(varchar(10),@stop_date,120)+''''+',
'+convert(varchar(5),@acd_type)

EXECUTE(@txt)


delete from #tmp_alogg
where 
                coalesce(avail_dur,0) = 0 and
                coalesce(tot_work_dur,0) = 0 and
                coalesce(talking_call_dur,0) = 0 and
                coalesce(pause_dur,0) = 0 and
                coalesce(wait_dur,0) = 0 and
                coalesce(wrap_up_dur,0) = 0 and
                coalesce(answ_call_cnt,0) = 0 and
                coalesce(direct_out_call_cnt,0) = 0 and
                coalesce(direct_out_call_dur,0) = 0 and
                coalesce(direct_in_call_cnt,0) = 0 and
                coalesce(direct_in_call_dur,0) = 0 and
                coalesce(transfer_out_call_cnt,0) = 0 and
				coalesce(admin_dur,0) = 0


/*********************************************************************************/
/* Adjust for different time zones between log_object and T-CCC?      */
/* If so, interval and date_from might need adjustment.                       */
/*********************************************************************************/
IF @max_add_hours IS NOT NULL
BEGIN
DELETE FROM #tmp_alogg
FROM #tmp_alogg a
WHERE NOT EXISTS (SELECT 1 FROM log_object_add_hours b
		WHERE dateadd(minute,@minutes_per_interval*a.interval,a.date_from)
			BETWEEN b.datetime_from AND b.datetime_to
		AND b.log_object_id = @log_object_id)


UPDATE #tmp_alogg SET
	date_from = convert(smalldatetime,convert(varchar(10),dateadd(minute,@minutes_per_interval*(a.interval+b.add_hours*@interval_per_hour),a.date_from),120)),
	interval = @interval_per_hour*datepart(hour,dateadd(minute,@minutes_per_interval*(a.interval+b.add_hours*@interval_per_hour),'1971-01-01'))+
	datepart(minute,dateadd(minute,@minutes_per_interval*(a.interval+b.add_hours*@interval_per_hour),'1971-01-01'))/@minutes_per_interval
FROM #tmp_alogg a INNER JOIN log_object_add_hours b ON
	dateadd(minute,a.interval*@minutes_per_interval,a.date_from) BETWEEN b.datetime_from AND b.datetime_to
END

/**********************************************************************/
/*    This is where the actual insert to the real tables start!	*/
/**********************************************************************/
BEGIN TRANSACTION 
/**********************************************************************/
/*   Remove the latest logg interval in agent_Logg	*/
/**********************************************************************/

DECLARE @mindate smalldatetime, @mininterval int
DECLARE @maxdate smalldatetime, @maxinterval int


SELECT @mindate=MIN(date_from) from #tmp_alogg
SELECT @mininterval=MIN(interval) FROM #tmp_alogg
WHERE date_from = @mindate

SELECT @maxdate=MAX(date_from) from #tmp_alogg
SELECT @maxinterval=MAX(interval) FROM #tmp_alogg
WHERE date_from = @maxdate


IF @maxdate > @stop_date
BEGIN
	SELECT @maxdate = @stop_date
	--090416
	SELECT @maxinterval=MAX(interval) FROM #tmp_alogg
	WHERE date_from = @maxdate
END

/*Hantera konverteringar mellan olika intervall*/
IF @CTI_interval_per_hour/2=@interval_per_hour
BEGIN
	SET @mininterval = @mininterval/2
	SET @maxinterval = @maxinterval/2
END
ELSE
BEGIN
	SELECT @maxinterval = @maxinterval + (@interval_per_hour / @CTI_interval_per_hour ) - 1
END


--090416
--Fix problem when add_hours gives us data for the day after @stop_date
DELETE FROM #tmp_alogg
WHERE date_from > @maxdate


IF  @mindate IS not NULL
BEGIN
	--090416 MK's performance fix
	DELETE FROM agent_logg
	WHERE date_From >@mindate and date_from <@maxdate
	AND exists 	(SELECT 1 FROM queues queues
			WHERE	queues.queue = agent_logg.queue
			AND	queues.log_object_id = @log_object_id)
	
	DELETE FROM agent_logg
	WHERE date_from =@mindate
	AND interval >=@mininterval
		AND exists 	(SELECT 1 FROM queues queues
				WHERE	queues.queue = agent_logg.queue
				AND	queues.log_object_id = @log_object_id)
	
	IF @maxdate = @mindate
	BEGIN
		DELETE FROM agent_logg
		WHERE date_from = @maxdate
		AND interval <= @maxinterval
		AND interval >=@mininterval
			AND exists 	(SELECT 1 FROM queues queues
					WHERE	queues.queue = agent_logg.queue
					AND	queues.log_object_id = @log_object_id)

	END
	ELSE
	BEGIN
		DELETE FROM agent_logg
		WHERE date_from = @maxdate
		AND interval <= @maxinterval
			AND exists 	(SELECT 1 FROM queues queues
					WHERE	queues.queue = agent_logg.queue
					AND	queues.log_object_id = @log_object_id)
	END


	/*
	DELETE FROM agent_logg
	WHERE (	
			( date_from > @mindate )
		OR 	
			( date_from = @mindate AND interval >= @mininterval )
		)
		AND
		(	
			( date_from < @maxdate )
		OR 	
			( date_from = @maxdate AND interval <= @maxinterval )
		)
	AND exists 	(SELECT 1 FROM queues queues
			WHERE	queues.queue = agent_logg.queue
			AND	queues.log_object_id = @log_object_id)
	*/

END

IF EXISTS (SELECT 1 FROM #tmp_alogg t
						INNER JOIN agent_logg al ON al.agent_id = t.agent_id AND al.queue = t.queue 
								AND al.date_from = t.date_from AND al.interval = t.interval)
BEGIN
	SELECT 'There is an error in p_insert_agent_agent_logg. The aggregation fails because this data cannot be inserted to agent_logg:'
	SELECT * 
	FROM #tmp_alogg t
	INNER JOIN agent_logg al ON al.agent_id = t.agent_id
		AND al.queue = t.queue AND al.date_from = t.date_from AND al.interval = t.interval
END

/**************************************************
090416 Simple error checking of aggregated data	
**************************************************/
IF @debug = 1
BEGIN

	SELECT 'Debug mode - Agent stats: Errors will be displayed below if found. Just a few checks are done.'

	DECLARE @sec_per_interval int
	SET @sec_per_interval = @minutes_per_interval*60

	--temp table, whole days summarized
	SELECT [queue],[date_from],[agent_id],max([agent_name])[agent_name],sum([avail_dur])[avail_dur],
		sum([tot_work_dur])[tot_work_dur],sum([talking_call_dur])[talking_call_dur],
		sum([pause_dur])[pause_dur],sum([wait_dur])[wait_dur],sum([wrap_up_dur])[wrap_up_dur],
		sum([answ_call_cnt])[answ_call_cnt],sum([direct_out_call_cnt])[direct_out_call_cnt],
		sum([direct_out_call_dur])[direct_out_call_dur],sum([direct_in_call_cnt])[direct_in_call_cnt],
		sum([direct_in_call_dur])[direct_in_call_dur],sum([transfer_out_call_cnt])[transfer_out_call_cnt],
		sum([admin_dur])[admin_dur]
	INTO #day_sum
	FROM #tmp_alogg
	GROUP BY [queue],[date_from],[agent_id]
	

	--avail/work/pause duration length > interval length
	IF EXISTS (SELECT 1 FROM #tmp_alogg
		WHERE tot_work_dur > @sec_per_interval
			OR avail_dur + pause_dur > @sec_per_interval)
	BEGIN
		SELECT 'Error. Duration cannot be longer than the interval length:'
		SELECT queue, date_from, interval, agent_id, agent_name, tot_work_dur, avail_dur, pause_dur
		FROM #tmp_alogg
		WHERE tot_work_dur > @sec_per_interval
			OR avail_dur + pause_dur > @sec_per_interval
	END
	
	--avail + pause != work
	IF EXISTS (SELECT 1 FROM #day_sum WHERE avail_dur + pause_dur <> tot_work_dur)
	BEGIN
		SELECT 'Pause + avail should be equal to work dur:'
		SELECT * FROM #day_sum WHERE avail_dur + pause_dur <> tot_work_dur
	END

	--negative values
	IF EXISTS (SELECT 1 FROM #tmp_alogg WHERE [avail_dur] < 0 or [tot_work_dur]<0 or [talking_call_dur]<0
      or [pause_dur]<0 or [wait_dur]<0 or [wrap_up_dur]<0 or [answ_call_cnt]<0 or [direct_out_call_cnt]<0
      or [direct_out_call_dur]<0 or [direct_in_call_cnt]<0 or [direct_in_call_dur]<0 or [transfer_out_call_cnt]<0 or [admin_dur]<0)
	BEGIN
		SELECT 'Error: Negative values:'
		SELECT * FROM #tmp_alogg WHERE [avail_dur] < 0 or [tot_work_dur]<0 or [talking_call_dur]<0
			or [pause_dur]<0 or [wait_dur]<0 or [wrap_up_dur]<0 or [answ_call_cnt]<0 or [direct_out_call_cnt]<0
			or [direct_out_call_dur]<0 or [direct_in_call_cnt]<0 or [direct_in_call_dur]<0 or [transfer_out_call_cnt]<0 or [admin_dur]<0
	END
	
	
END

/**************************************************/

/*       insert into agent_logg		*/	
/**************************************************/
-- CTI 60 minuter AGG 15 minuter
IF (@CTI_interval_per_hour*4=@interval_per_hour)
BEGIN
select '@CTI_interval_per_hour*4=@interval_per_hour'

--091013 incorrect split

INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur)
	SELECT
	queue,
	date_from,
	interval,
	agent_id, 
	max(agent_name),
	sum(avail_dur)/4+sum(avail_dur)%4,
	sum(tot_work_dur)/4+sum(tot_work_dur)%4,
	sum(talking_call_dur)/4+sum(talking_call_dur)%4,	
	sum(pause_dur)/4+sum(pause_dur)%4,
	sum(wait_dur)/4+sum(wait_dur)%4,
	sum(wrap_up_dur)/4+sum(wrap_up_dur)%4,
	sum(answ_call_cnt)/4+sum(answ_call_cnt)%4,
	sum(direct_out_call_cnt)/4+sum(direct_out_call_cnt)%4,
	sum(direct_out_call_dur)/4+sum(direct_out_call_dur)%4,
	sum(direct_in_call_cnt)/4+sum(direct_in_call_cnt)%4,
	sum(direct_in_call_dur)/4+sum(direct_in_call_dur)%4,
	sum(transfer_out_call_cnt)/4+sum(transfer_out_call_cnt)%4,
	sum(admin_dur)/4+sum(admin_dur)%4
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name

INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur)
	SELECT
	queue,
	date_from,
	interval + 1,
	agent_id, 
	max(agent_name),
	sum(avail_dur)/4,
	sum(tot_work_dur)/4,
	sum(talking_call_dur)/4,	
	sum(pause_dur)/4,
	sum(wait_dur)/4,
	sum(wrap_up_dur)/4,
	sum(answ_call_cnt)/4,
	sum(direct_out_call_cnt)/4,
	sum(direct_out_call_dur)/4,
	sum(direct_in_call_cnt)/4,
	sum(direct_in_call_dur)/4,
	sum(transfer_out_call_cnt)/4,
	sum(admin_dur)/4
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name
INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur)
	SELECT
	queue,
	date_from,
	interval + 2,
	agent_id, 
	max(agent_name),
	sum(avail_dur)/4,
	sum(tot_work_dur)/4,
	sum(talking_call_dur)/4,	
	sum(pause_dur)/4,
	sum(wait_dur)/4,
	sum(wrap_up_dur)/4,
	sum(answ_call_cnt)/4,
	sum(direct_out_call_cnt)/4,
	sum(direct_out_call_dur)/4,
	sum(direct_in_call_cnt)/4,
	sum(direct_in_call_dur)/4,
	sum(transfer_out_call_cnt)/4,
	sum(admin_dur)/4
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name
INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur)
	SELECT
	queue,
	date_from,
	interval + 3,
	agent_id, 
	max(agent_name),
	sum(avail_dur)/4,
	sum(tot_work_dur)/4,
	sum(talking_call_dur)/4,	
	sum(pause_dur)/4,
	sum(wait_dur)/4,
	sum(wrap_up_dur)/4,
	sum(answ_call_cnt)/4,
	sum(direct_out_call_cnt)/4,
	sum(direct_out_call_dur)/4,
	sum(direct_in_call_cnt)/4,
	sum(direct_in_call_dur)/4,
	sum(transfer_out_call_cnt)/4,
	sum(admin_dur)/4
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name

/*
INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,

	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
	SELECT
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	round(convert(real,sum(avail_dur))/4.0,0),
	round(convert(real,sum(tot_work_dur))/4.0,0),
	round(convert(real,sum(talking_call_dur))/4.0,0),	
	round(convert(real,sum(pause_dur))/4.0,0),
	round(convert(real,sum(wait_dur))/4.0,0),
	round(convert(real,sum(wrap_up_dur))/4.0,0),
	round(convert(real,sum(answ_call_cnt))/4.0,0),
	round(convert(real,sum(direct_out_call_cnt))/4.0,0),
	round(convert(real,sum(direct_out_call_dur))/4.0,0),
	round(convert(real,sum(direct_in_call_cnt))/4.0,0),
	round(convert(real,sum(direct_in_call_dur))/4.0,0),
	round(convert(real,sum(transfer_out_call_cnt))/4.0,0),
	round(convert(real,sum(admin_dur))/4.0,0)
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id,agent_name

INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,

	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
	SELECT
	queue,
	date_from,
	interval + 1,
	agent_id, 
	agent_name,
	round(convert(real,sum(avail_dur))/4.0,0),
	round(convert(real,sum(tot_work_dur))/4.0,0),
	round(convert(real,sum(talking_call_dur))/4.0,0),	
	round(convert(real,sum(pause_dur))/4.0,0),
	round(convert(real,sum(wait_dur))/4.0,0),
	round(convert(real,sum(wrap_up_dur))/4.0,0),
	round(convert(real,sum(answ_call_cnt))/4.0,0),
	round(convert(real,sum(direct_out_call_cnt))/4.0,0),
	round(convert(real,sum(direct_out_call_dur))/4.0,0),
	round(convert(real,sum(direct_in_call_cnt))/4.0,0),
	round(convert(real,sum(direct_in_call_dur))/4.0,0),
	round(convert(real,sum(transfer_out_call_cnt))/4.0,0),
	round(convert(real,sum(admin_dur))/4.0,0)
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id,agent_name

INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt ,
	admin_dur)
	SELECT
	queue,
	date_from,
	interval + 2,
	agent_id, 
	agent_name,
	ceiling(convert(real,sum(avail_dur))/4.0),
	ceiling(convert(real,sum(tot_work_dur))/4.0),
	ceiling(convert(real,sum(talking_call_dur))/4.0),	
	ceiling(convert(real,sum(pause_dur))/4.0),
	ceiling(convert(real,sum(wait_dur))/4.0),
	ceiling(convert(real,sum(wrap_up_dur))/4.0),
	ceiling(convert(real,sum(answ_call_cnt))/4.0),
	ceiling(convert(real,sum(direct_out_call_cnt))/4.0),
	ceiling(convert(real,sum(direct_out_call_dur))/4.0),
	ceiling(convert(real,sum(direct_in_call_cnt))/4.0),
	ceiling(convert(real,sum(direct_in_call_dur))/4.0),
	ceiling(convert(real,sum(transfer_out_call_cnt))/4.0),
	ceiling(convert(real,sum(admin_dur))/4.0)
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id,agent_name

INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur)
	SELECT
	queue,
	date_from,
	interval + 3,
	agent_id, 
	agent_name,
	sum(avail_dur)/4,
	sum(tot_work_dur)/4,
	sum(talking_call_dur)/4,	
	sum(pause_dur)/4,
	sum(wait_dur)/4,
	sum(wrap_up_dur)/4,
	sum(answ_call_cnt)/4,
	sum(direct_out_call_cnt)/4,
	sum(direct_out_call_dur)/4,
	sum(direct_in_call_cnt)/4,
	sum(direct_in_call_dur)/4,
	sum(transfer_out_call_cnt)/4,
	sum(admin_dur)/4
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id,agent_name
*/


END

-- CTI 60 minuter AGG 10 minuter
IF (@CTI_interval_per_hour*6=@interval_per_hour)
BEGIN
select '@CTI_interval_per_hour*6=@interval_per_hour'
INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
	SELECT
	queue,
	date_from,
	interval,
	agent_id, 
--090416
	max(agent_name),--agent_name,
	round(convert(real,sum(avail_dur))/6.0,0),
	round(convert(real,sum(tot_work_dur))/6.0,0),
	round(convert(real,sum(talking_call_dur))/6.0,0),	
	round(convert(real,sum(pause_dur))/6.0,0),
	round(convert(real,sum(wait_dur))/6.0,0),
	round(convert(real,sum(wrap_up_dur))/6.0,0),
	round(convert(real,sum(answ_call_cnt))/6.0,0),
	round(convert(real,sum(direct_out_call_cnt))/6.0,0),
	round(convert(real,sum(direct_out_call_dur))/6.0,0),
	round(convert(real,sum(direct_in_call_cnt))/6.0,0),
	round(convert(real,sum(direct_in_call_dur))/6.0,0),
	round(convert(real,sum(transfer_out_call_cnt))/6.0,0),
	round(convert(real,sum(admin_dur))/6.0,0)

	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name

INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
	SELECT
	queue,
	date_from,
	interval + 1,
	agent_id, 
--090416
	max(agent_name),--agent_name,
	round(convert(real,sum(avail_dur))/6.0,0),
	round(convert(real,sum(tot_work_dur))/6.0,0),
	round(convert(real,sum(talking_call_dur))/6.0,0),	
	round(convert(real,sum(pause_dur))/6.0,0),
	round(convert(real,sum(wait_dur))/6.0,0),
	round(convert(real,sum(wrap_up_dur))/6.0,0),
	round(convert(real,sum(answ_call_cnt))/6.0,0),
	round(convert(real,sum(direct_out_call_cnt))/6.0,0),
	round(convert(real,sum(direct_out_call_dur))/6.0,0),
	round(convert(real,sum(direct_in_call_cnt))/6.0,0),
	round(convert(real,sum(direct_in_call_dur))/6.0,0),
	round(convert(real,sum(transfer_out_call_cnt))/6.0,0),
	round(convert(real,sum(admin_dur))/6.0,0)
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name

INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
admin_dur )
	SELECT
	queue,
	date_from,
	interval + 2,
	agent_id, 
--090416
	max(agent_name),--agent_name,
	round(convert(real,sum(avail_dur))/6.0,0),
	round(convert(real,sum(tot_work_dur))/6.0,0),
	round(convert(real,sum(talking_call_dur))/6.0,0),	
	round(convert(real,sum(pause_dur))/6.0,0),
	round(convert(real,sum(wait_dur))/6.0,0),
	round(convert(real,sum(wrap_up_dur))/6.0,0),
	round(convert(real,sum(answ_call_cnt))/6.0,0),
	round(convert(real,sum(direct_out_call_cnt))/6.0,0),
	round(convert(real,sum(direct_out_call_dur))/6.0,0),
	round(convert(real,sum(direct_in_call_cnt))/6.0,0),
	round(convert(real,sum(direct_in_call_dur))/6.0,0),
	round(convert(real,sum(transfer_out_call_cnt))/6.0,0),
	round(convert(real,sum(admin_dur))/6.0,0)
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name

INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
	SELECT
	queue,
	date_from,
	interval + 3,
	agent_id, 
--090416
	max(agent_name),--agent_name,
	round(convert(real,sum(avail_dur))/6.0,0),
	round(convert(real,sum(tot_work_dur))/6.0,0),
	round(convert(real,sum(talking_call_dur))/6.0,0),	
	round(convert(real,sum(pause_dur))/6.0,0),
	round(convert(real,sum(wait_dur))/6.0,0),
	round(convert(real,sum(wrap_up_dur))/6.0,0),
	round(convert(real,sum(answ_call_cnt))/6.0,0),
	round(convert(real,sum(direct_out_call_cnt))/6.0,0),
	round(convert(real,sum(direct_out_call_dur))/6.0,0),
	round(convert(real,sum(direct_in_call_cnt))/6.0,0),
	round(convert(real,sum(direct_in_call_dur))/6.0,0),
	round(convert(real,sum(transfer_out_call_cnt))/6.0,0),
	round(convert(real,sum(admin_dur))/6.0,0)
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name

INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
	SELECT
	queue,
	date_from,
	interval + 4,
	agent_id, 
--090416
	max(agent_name),--agent_name,
	ceiling(convert(real,sum(avail_dur))/6.0),
	ceiling(convert(real,sum(tot_work_dur))/6.0),
	ceiling(convert(real,sum(talking_call_dur))/6.0),	
	ceiling(convert(real,sum(pause_dur))/6.0),
	ceiling(convert(real,sum(wait_dur))/6.0),
	ceiling(convert(real,sum(wrap_up_dur))/6.0),
	ceiling(convert(real,sum(answ_call_cnt))/6.0),
	ceiling(convert(real,sum(direct_out_call_cnt))/6.0),
	ceiling(convert(real,sum(direct_out_call_dur))/6.0),
	ceiling(convert(real,sum(direct_in_call_cnt))/6.0),
	ceiling(convert(real,sum(direct_in_call_dur))/6.0),
	ceiling(convert(real,sum(transfer_out_call_cnt))/6.0),
	ceiling(convert(real,sum(admin_dur))/6.0)
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name

INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
	SELECT
	queue,
	date_from,
	interval + 5,
	agent_id, 
--090416
	max(agent_name),--agent_name,
	sum(avail_dur)/6,
	sum(tot_work_dur)/6,
	sum(talking_call_dur)/6,	
	sum(pause_dur)/6,
	sum(wait_dur)/6,
	sum(wrap_up_dur)/6,
	sum(answ_call_cnt)/6,
	sum(direct_out_call_cnt)/6,
	sum(direct_out_call_dur)/6,
	sum(direct_in_call_cnt)/6,
	sum(direct_in_call_dur)/6,
	sum(transfer_out_call_cnt)/6,
	sum(admin_dur)/6
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name
END


IF (@CTI_interval_per_hour*2=@interval_per_hour)
BEGIN
select '@CTI_interval_per_hour*2=@interval_per_hour'
	INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,


	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
	SELECT
	queue,
	date_from,
	interval,
	agent_id, 
--090416
	max(agent_name),--agent_name,
	ceiling(convert(real,sum(avail_dur))/2.0),
	ceiling(convert(real,sum(tot_work_dur))/2.0),
	ceiling(convert(real,sum(talking_call_dur))/2.0),	
	ceiling(convert(real,sum(pause_dur))/2.0),
	ceiling(convert(real,sum(wait_dur))/2.0),
	ceiling(convert(real,sum(wrap_up_dur))/2.0),
	ceiling(convert(real,sum(answ_call_cnt))/2.0),
	ceiling(convert(real,sum(direct_out_call_cnt))/2.0),
	ceiling(convert(real,sum(direct_out_call_dur))/2.0),
	ceiling(convert(real,sum(direct_in_call_cnt))/2.0),
	ceiling(convert(real,sum(direct_in_call_dur))/2.0),
	ceiling(convert(real,sum(transfer_out_call_cnt))/2.0),
	ceiling(convert(real,sum(admin_dur))/2.0)
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name
	INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
	SELECT
	queue,
	date_from,
	interval+1,
	agent_id, 
--090416
	max(agent_name),--agent_name,
	sum(avail_dur)/2,
	sum(tot_work_dur)/2,
	sum(talking_call_dur)/2,	
	sum(pause_dur)/2,
	sum(wait_dur)/2,
	sum(wrap_up_dur)/2,
	sum(answ_call_cnt)/2,
	sum(direct_out_call_cnt)/2,
	sum(direct_out_call_dur)/2,
	sum(direct_in_call_cnt)/2,
	sum(direct_in_call_dur)/2,
	sum(transfer_out_call_cnt)/2,
	sum(admin_dur)/2
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name
END

IF (@CTI_interval_per_hour*3 = @interval_per_hour)
BEGIN
select '@CTI_interval_per_hour*3=@interval_per_hour'
	INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
	SELECT
	queue,
	date_from,
	interval,
	agent_id, 
--090416
	max(agent_name),--agent_name,
	round(convert(real,sum(avail_dur))/3.0,0),
	round(convert(real,sum(tot_work_dur))/3.0,0),
	round(convert(real,sum(talking_call_dur))/3.0,0),	
	round(convert(real,sum(pause_dur))/3.0,0),
	round(convert(real,sum(wait_dur))/3.0,0),
	round(convert(real,sum(wrap_up_dur))/3.0,0),
	round(convert(real,sum(answ_call_cnt))/3.0,0),
	round(convert(real,sum(direct_out_call_cnt))/3.0,0),
	round(convert(real,sum(direct_out_call_dur))/3.0,0),
	round(convert(real,sum(direct_in_call_cnt))/3.0,0),
	round(convert(real,sum(direct_in_call_dur))/3.0,0),
	round(convert(real,sum(transfer_out_call_cnt))/3.0,0),
	round(convert(real,sum(admin_dur))/3.0,0)

	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name

	INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
	SELECT
	queue,
	date_from,
	interval + 1,
	agent_id, 
--090416
	max(agent_name),--agent_name,
	ceiling(convert(real,sum(avail_dur))/3.0),
	ceiling(convert(real,sum(tot_work_dur))/3.0),
	ceiling(convert(real,sum(talking_call_dur))/3.0),	
	ceiling(convert(real,sum(pause_dur))/3.0),
	ceiling(convert(real,sum(wait_dur))/3.0),
	ceiling(convert(real,sum(wrap_up_dur))/3.0),
	ceiling(convert(real,sum(answ_call_cnt))/3.0),
	ceiling(convert(real,sum(direct_out_call_cnt))/3.0),
	ceiling(convert(real,sum(direct_out_call_dur))/3.0),
	ceiling(convert(real,sum(direct_in_call_cnt))/3.0),
	ceiling(convert(real,sum(direct_in_call_dur))/3.0),
	ceiling(convert(real,sum(transfer_out_call_cnt))/3.0),
	ceiling(convert(real,sum(admin_dur))/3.0)
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name

	INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
	SELECT
	queue,
	date_from,
	interval+2,
	agent_id, 
--090416
	max(agent_name),--agent_name,
	sum(avail_dur)/3,
	sum(tot_work_dur)/3,
	sum(talking_call_dur)/3,	
	sum(pause_dur)/3,
	sum(wait_dur)/3,
	sum(wrap_up_dur)/3,
	sum(answ_call_cnt)/3,
	sum(direct_out_call_cnt)/3,
	sum(direct_out_call_dur)/3,
	sum(direct_in_call_cnt)/3,
	sum(direct_in_call_dur)/3,
	sum(transfer_out_call_cnt)/3,
	sum(admin_dur)/3
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name
END

-------------------
IF (@CTI_interval_per_hour*1.5 = @interval_per_hour)
BEGIN

select '@CTI_interval_per_hour*1.5=@interval_per_hour'
	CREATE table #tmpagent_logg2 (
	queue int NOT NULL,
	date_from smalldatetime NOT NULL,
	interval int NOT NULL,
	agent_id int NOT NULL, 
	agent_name nvarchar(100) NOT NULL,
	avail_dur int  NULL,
	tot_work_dur int  NULL,
	talking_call_dur int  NULL,	
	pause_dur int  NULL,
	wait_dur int  NULL,
	wrap_up_dur int  NULL,
	answ_call_cnt int  NULL,
	direct_out_call_cnt int  NULL,
	direct_out_call_dur int  NULL,
	direct_in_call_cnt int  NULL,
	direct_in_call_dur int  NULL,
	transfer_out_call_cnt int  NULL,
	admin_dur int  NULL )
	
--	insert into #tmpagent_logg2
--	SELECT
--	queue,
--	date_from,
--	interval/2,
--	agent_id, 
----090416
--	max(agent_name),--agent_name,
--	sum(avail_dur),
--	sum(tot_work_dur),
--	sum(talking_call_dur),	
--	sum(pause_dur),
--	sum(wait_dur),
--	sum(wrap_up_dur),
--	sum(answ_call_cnt),
--	sum(direct_out_call_cnt),
--	sum(direct_out_call_dur),
--	sum(direct_in_call_cnt),
--	sum(direct_in_call_dur),
--	sum(transfer_out_call_cnt),
--	sum(admin_dur)

	--FROM #tmp_alogg
	--GROUP BY queue,date_from,interval,agent_id--,agent_name

	INSERT #tmpagent_logg2 (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
	SELECT
	queue,
	date_from,
	interval*3,
	agent_id, 
--090416
	max(agent_name),--agent_name,
	round(convert(real,sum(avail_dur))/3.0,0),
	round(convert(real,sum(tot_work_dur))/3.0,0),
	round(convert(real,sum(talking_call_dur))/3.0,0),	
	round(convert(real,sum(pause_dur))/3.0,0),
	round(convert(real,sum(wait_dur))/3.0,0),
	round(convert(real,sum(wrap_up_dur))/3.0,0),
	round(convert(real,sum(answ_call_cnt))/3.0,0),
	round(convert(real,sum(direct_out_call_cnt))/3.0,0),
	round(convert(real,sum(direct_out_call_dur))/3.0,0),
	round(convert(real,sum(direct_in_call_cnt))/3.0,0),
	round(convert(real,sum(direct_in_call_dur))/3.0,0),
	round(convert(real,sum(transfer_out_call_cnt))/3.0,0),
	round(convert(real,sum(admin_dur))/3.0,0)

	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name

	INSERT #tmpagent_logg2 (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
	SELECT
	queue,
	date_from,
	interval*3 + 1,
	agent_id, 
--090416
	max(agent_name),--agent_name,
	ceiling(convert(real,sum(avail_dur))/3.0),
	ceiling(convert(real,sum(tot_work_dur))/3.0),
	ceiling(convert(real,sum(talking_call_dur))/3.0),	
	ceiling(convert(real,sum(pause_dur))/3.0),
	ceiling(convert(real,sum(wait_dur))/3.0),
	ceiling(convert(real,sum(wrap_up_dur))/3.0),
	ceiling(convert(real,sum(answ_call_cnt))/3.0),
	ceiling(convert(real,sum(direct_out_call_cnt))/3.0),
	ceiling(convert(real,sum(direct_out_call_dur))/3.0),
	ceiling(convert(real,sum(direct_in_call_cnt))/3.0),
	ceiling(convert(real,sum(direct_in_call_dur))/3.0),
	ceiling(convert(real,sum(transfer_out_call_cnt))/3.0),
	ceiling(convert(real,sum(admin_dur))/3.0)
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name
	

	INSERT #tmpagent_logg2 (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
	SELECT
	queue,
	date_from,
	interval*3+2,
	agent_id, 
--090416
	max(agent_name),--agent_name,
	sum(avail_dur)/3,
	sum(tot_work_dur)/3,
	sum(talking_call_dur)/3,	
	sum(pause_dur)/3,
	sum(wait_dur)/3,
	sum(wrap_up_dur)/3,
	sum(answ_call_cnt)/3,
	sum(direct_out_call_cnt)/3,
	sum(direct_out_call_dur)/3,
	sum(direct_in_call_cnt)/3,
	sum(direct_in_call_dur)/3,
	sum(transfer_out_call_cnt)/3,
	sum(admin_dur)/3
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name
	
	--select * from #tmpagent_logg2
	insert into agent_logg 
		SELECT
	queue,
	date_from,
	interval/2,
	agent_id, 
--090416
	max(agent_name),--agent_name,
	sum(avail_dur),
	sum(tot_work_dur),
	sum(talking_call_dur),	
	sum(pause_dur),
	sum(wait_dur),
	sum(wrap_up_dur),
	sum(answ_call_cnt),
	sum(direct_out_call_cnt),
	sum(direct_out_call_dur),
	sum(direct_in_call_cnt),
	sum(direct_in_call_dur),
	sum(transfer_out_call_cnt),
	sum(admin_dur)
	FROM #tmpagent_logg2 t2
	where not exists(select 1 from agent_logg	a	
					where t2.date_from =a.date_from and t2.interval/2 =a.interval 
					and t2.agent_id =a.agent_id and t2.queue =a.queue )
	GROUP BY queue,date_from,interval/2,agent_id
END
---------------------------------------

--Merge 2:1
IF (@CTI_interval_per_hour/2=@interval_per_hour)
BEGIN
select '@CTI_interval_per_hour/2=@interval_per_hour'
INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
SELECT
	queue,
	date_from,
	interval/2,
	agent_id, 
	max(agent_name),
	sum(avail_dur),
	sum(tot_work_dur),
	sum(talking_call_dur),	
	sum(pause_dur),
	sum(wait_dur),
	sum(wrap_up_dur),
	sum(answ_call_cnt),
	sum(direct_out_call_cnt),
	sum(direct_out_call_dur),
	sum(direct_in_call_cnt),
	sum(direct_in_call_dur),
	sum(transfer_out_call_cnt),
	sum(admin_dur)
	FROM #tmp_alogg
	GROUP BY
		queue,
		date_from,
		interval/2,
		agent_id
END

IF (@CTI_interval_per_hour = @interval_per_hour) 
BEGIN
--090416
--select 'vanlig'
SELECT '@CTI_interval_per_hour=@interval_per_hour'
INSERT agent_logg (
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,
	tot_work_dur,
	talking_call_dur,	
	pause_dur,
	wait_dur,
	wrap_up_dur,
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,
	direct_in_call_cnt,
	direct_in_call_dur,
	transfer_out_call_cnt,
	admin_dur )
SELECT
	queue,
	date_from,
	interval,
	agent_id, 
--090416
	max(agent_name),--agent_name,
	sum(avail_dur),
	sum(tot_work_dur),
	sum(talking_call_dur),	
	sum(pause_dur),
	sum(wait_dur),
	sum(wrap_up_dur),
	sum(answ_call_cnt),
	sum(direct_out_call_cnt),
	sum(direct_out_call_dur),
	sum(direct_in_call_cnt),
	sum(direct_in_call_dur),
	sum(transfer_out_call_cnt),
	sum(admin_dur)
	FROM #tmp_alogg
	GROUP BY queue,date_from,interval,agent_id--,agent_name
END
IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION 
	RETURN
END
/********************************************************/
/* Uppdatera last_logg					*/
/********************************************************/
--DECLARE @maxdate smalldatetime, @maxinterval int

IF (@start_date < '1970-01-01') AND (@end_date < '1970-01-01')
BEGIN

	--2016-04-04, removed:
	--SELECT @maxdate = MAX(date_from) FROM #tmp_alogg
	--SELECT @maxinterval = MAX(interval) FROM #tmp_alogg
	--WHERE date_from = @maxdate

	--2016-04-04, added:
	IF @CTI_interval_per_hour > @interval_per_hour
	BEGIN
		IF @CTI_interval_per_hour/2=@interval_per_hour
		BEGIN
			SELECT @maxdate = MAX(date_from) FROM #tmp_alogg
			SELECT @maxinterval = MAX(interval)/2 FROM #tmp_alogg
			WHERE date_from = @maxdate
		END
		ELSE
		BEGIN
			SELECT 'OBSERVE! You need to make sure the new merge updates log_object_detail correctly!'
		END
	END
		ELSE
	BEGIN
		SELECT @maxdate = MAX(date_from) FROM #tmp_alogg
		SELECT @maxinterval = MAX(interval) FROM #tmp_alogg
		WHERE date_from = @maxdate
	END
	
	IF  @maxdate IS not NULL
	BEGIN
   		UPDATE log_object_detail
		SET date_value = @maxdate,
	            int_value = @maxinterval 
		WHERE log_object_id = @log_object_id
		AND detail_id = 2
	
	END
END
ELSE
BEGIN
	
	--2016-04-04, removed:
	--SELECT @maxdate = MAX(date_from) FROM agent_logg
	--SELECT @maxinterval = MAX(interval) FROM agent_logg
	--WHERE date_from = @maxdate
	
	--2016-04-04 added, not an issue now but might be
	SELECT @maxdate = MAX(date_from) 
	FROM agent_logg AS al 
	INNER JOIN queues q ON al.queue = q.queue 
	WHERE q.log_object_id = @log_object_id
	
	SELECT @maxinterval = MAX(interval) 
	FROM agent_logg AS al 
	INNER JOIN queues q ON al.queue = q.queue
	WHERE date_from = @maxdate
		AND q.log_object_id = @log_object_id

	IF  @maxdate IS not NULL
	BEGIN
		UPDATE log_object_detail
		SET date_value = @maxdate,
            		int_value = @maxinterval 
		WHERE log_object_id = @log_object_id
		AND detail_id = 2
	END    
END

--090416
IF @maxdate IS NULL
BEGIN 
	SELECT 'No agent_logg stats received from TeleoptiLogDb from time set in log_object_detail and one month forward.'	
END

IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION 
	RETURN
END

COMMIT TRANSACTION
SET NOCOUNT OFF

GO

