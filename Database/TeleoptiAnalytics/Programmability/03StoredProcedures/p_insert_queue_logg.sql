IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_insert_queue_logg]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_insert_queue_logg]
GO


/*****************************************/
/* Created by Anders F 20020605  */
/*****************************************/
CREATE PROCEDURE [dbo].[p_insert_queue_logg]
/*

				
Existing convertions	     	30 minute intervals to 10 minute logging
				30 minute intervals to 10 minute logging

Micke D 2004-02-27 Added 10 minutes intervals on 30 minutes logging
Micke D 2004-08-19 Added 10 minute intervals on 60 minute logging
Micke D 2004-08-19 Added 15 minute intervals on 60 minute logging
henrikl 2009-04-16 Various bug fixes (some not tested), debug mode for simple 
	error checking of aggregated data etc. Changes marked with: --090416

magnus K 2011-01-19 remove score card
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
@default_short_call_treshold int,
@default_service_level_sec int,
@int_per_day int
SET NOCOUNT ON

/*
@start_date anvõnds om man vill agga frÕn specifikt datum
@end_date anvõnds om man vill agga till ett specifikt datum
@rel_start_date anvõnds om man t ex vill agg bakÕt en dag hela tiden. Skickar dÕ in -1
@rel_start_int anvõnds om man t ex vill agg bakÕt fyra intervall hela tiden. Skickar dÕ in -4
*/
/********************************************************/
/* Fetch latest log date and interval                   */
/********************************************************/
SELECT @last_logg_date = date_value , @last_logg_interval = int_value
FROM log_object_detail 
WHERE log_object_id = @log_object_id
AND detail_id = 1

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


/* 

Hõr blir det lite nytt  

*/
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
			/* Hõr ligger intervallet fortfarande inom dagen sÕ det õr bara att plussa */
			SELECT @last_logg_interval = @last_logg_interval + @rel_start_int
		END
		ELSE IF  (@rel_start_int + @last_logg_interval >= @int_per_day)
		BEGIN
			/* Hõr har intervallet gÕtt ÷ver till nõsta dag vi ÷kar pÕ dagen en dag och õndrar intervallet */
			SELECT @last_logg_date = dateadd ( day,1,@last_logg_date)
			SELECT @last_logg_interval = (@last_logg_interval + @rel_start_int) - (@int_per_day - 1)
		END 
	END 
	ELSE
	BEGIN
		/* Hõr õr intervallet igÕr */
		SELECT @last_logg_date = dateadd ( day,-1,@last_logg_date)
		SELECT @last_logg_interval = (@int_per_day - 1) - (@last_logg_interval - @rel_start_int)
	END 
END


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


SELECT @acd_type=acd_type_id, @default_service_level_sec  = isnull(default_service_level_sec,0), @default_short_call_treshold = isnull(default_short_call_treshold,0)
FROM log_object WHERE log_object_id = @log_object_id

/*********************************************************************************/
/* Adjust for different time zones between log_object and T-CCC?      */
/* If so, last logg interval might need adjustment.                      */
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

/***************************************************************/
/* Create a temporary table wich we will add data to   */
/***************************************************************/
CREATE TABLE #tmp_queue_logg (
	queue int NOT NULL,
	date_from smalldatetime NOT NULL,
	interval int NOT NULL,
	offd_direct_call_cnt int NULL,
	overflow_in_call_cnt int NULL,
	aband_call_cnt int NULL,
	overflow_out_call_cnt int NULL,
	answ_call_cnt int NULL,
	queued_and_answ_call_dur int NULL,
	queued_and_aband_call_dur int NULL,
	talking_call_dur int NULL,
	wrap_up_dur int NULL,
	queued_answ_longest_que_dur int NULL,
	queued_aband_longest_que_dur int NULL,
	avg_avail_member_cnt int NULL,
	ans_servicelevel_cnt int NULL,
	wait_dur int NULL,
	aband_short_call_cnt int NULL,
	aband_within_sl_cnt int NULL)




/***************************************************************/
/* Execute the correct procedure for this log object	   */
/***************************************************************/
DECLARE @txt varchar(8000)
SELECT @txt = (SELECT logdb_name FROM log_object WHERE log_object_id = @log_object_id)+'..'+
	(SELECT proc_name FROM log_object_detail WHERE log_object_id = @log_object_id AND detail_id = 1)+'
'+convert(varchar(5),@log_object_id)+',
'+''''+convert(varchar(10),@last_logg_date,120)+''''+',
'+convert(varchar(5),@last_logg_interval)+',
'+convert(varchar(5),@minutes_per_interval)+',
'+convert(varchar(5),@interval_per_hour)+',
'+convert(varchar(5),@CTI_interval_per_hour)+',
'+convert(varchar(5),@CTI_minutes_per_interval)+',
'+''''+convert(varchar(20),@last_logg_date_time,120)+''''+',
'+''''+convert(varchar(10),@stop_date,120)+''''+',
'+convert(varchar(5),@acd_type)+',
'+convert(varchar(5),@default_short_call_treshold)+',
'+convert(varchar(5),@default_service_level_sec)
--SELECT @txt
--RETURN
EXECUTE(@txt)



/*********************************************************************************/
/* Adjust for different time zones between log_object and T-CCC?      */
/* If so, interval and date_from might need adjustment.                       */
/*********************************************************************************/
IF @max_add_hours IS NOT NULL
BEGIN
DELETE FROM #tmp_queue_logg
FROM #tmp_queue_logg a
WHERE NOT EXISTS (SELECT 1 FROM log_object_add_hours b
		WHERE dateadd(minute,@minutes_per_interval*a.interval,a.date_from)
			BETWEEN b.datetime_from AND b.datetime_to
		AND b.log_object_id = @log_object_id)

UPDATE #tmp_queue_logg SET
	date_from = convert(smalldatetime,convert(varchar(10),dateadd(minute,@minutes_per_interval*(a.interval+b.add_hours*@interval_per_hour),a.date_from),120)),
	interval = @interval_per_hour*datepart(hour,dateadd(minute,@minutes_per_interval*(a.interval+b.add_hours*@interval_per_hour),'1971-01-01'))+
	datepart(minute,dateadd(minute,@minutes_per_interval*(a.interval+b.add_hours*@interval_per_hour),'1971-01-01'))/@minutes_per_interval
FROM #tmp_queue_logg a INNER JOIN log_object_add_hours b ON
	dateadd(minute,a.interval*@minutes_per_interval,a.date_from) BETWEEN b.datetime_from AND b.datetime_to
END

/****************************************************************************/
/*  Delete the lastest interval in queue_logg, and everything later */

/****************************************************************************/
BEGIN TRANSACTION
/*
	Hõr blir det nytt
	
*/

DECLARE @mindate smalldatetime, @mininterval int
DECLARE @maxdate smalldatetime, @maxinterval int

SELECT @mindate=MIN(date_from) from #tmp_queue_logg
SELECT @mininterval=MIN(interval) FROM #tmp_queue_logg
WHERE date_from = @mindate

SELECT @maxdate=MAX(date_from) from #tmp_queue_logg
SELECT @maxinterval=MAX(interval) FROM #tmp_queue_logg
WHERE date_from = @maxdate



IF @maxdate > @stop_date
BEGIN
	SELECT @maxdate = @stop_date
	--090416
	SELECT @maxinterval=MAX(interval) FROM #tmp_queue_logg
	WHERE date_from = @maxdate
END

/*Hantera konverteringar mellan olika intervall*/
SELECT @maxinterval = @maxinterval + (@interval_per_hour / @CTI_interval_per_hour ) - 1

--090416
--Fix problem when add_hours gives us data for the day after @stop_date
DELETE FROM #tmp_queue_logg
WHERE date_from > @maxdate


IF  @mindate IS not NULL
BEGIN

	--090416 MK's performance fix
	DELETE FROM queue_logg
	WHERE date_From >@mindate and date_from <@maxdate
	AND exists 	(SELECT 1 FROM queues queues
			WHERE	queues.queue = queue_logg.queue
			AND	queues.log_object_id = @log_object_id)
	
	DELETE FROM queue_logg
	WHERE date_from =@mindate
	AND interval >=@mininterval
		AND exists 	(SELECT 1 FROM queues queues
				WHERE	queues.queue = queue_logg.queue
				AND	queues.log_object_id = @log_object_id)
	
	IF @maxdate = @mindate
	BEGIN
		DELETE FROM queue_logg
		WHERE date_from = @maxdate
		AND interval <= @maxinterval
		AND interval >=@mininterval
			AND exists 	(SELECT 1 FROM queues queues
					WHERE	queues.queue = queue_logg.queue
					AND	queues.log_object_id = @log_object_id)

	END
	ELSE
	BEGIN
		DELETE FROM queue_logg
		WHERE date_from = @maxdate
		AND interval <= @maxinterval
			AND exists 	(SELECT 1 FROM queues queues
					WHERE	queues.queue = queue_logg.queue
					AND	queues.log_object_id = @log_object_id)
	END

/*
	DELETE FROM queue_logg
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
			WHERE queues.queue = queue_logg.queue
			AND	queues.log_object_id = @log_object_id)
*/


END

IF EXISTS (SELECT 1 FROM #tmp_queue_logg t
						INNER JOIN queue_logg al ON al.queue = t.queue 
								AND al.date_from = t.date_from AND al.interval = t.interval)
BEGIN
	SELECT 'There is an error in p_insert_queue_logg. The aggregation fails because this data cannot be inserted to queue_logg:'
	SELECT * 
	FROM #tmp_queue_logg t
	INNER JOIN queue_logg al ON al.queue = t.queue AND al.date_from = t.date_from AND al.interval = t.interval
END

/**************************************************
090416 Simple error checking of aggregated data	
**************************************************/
IF @debug = 1
BEGIN

	SELECT 'Debug mode - Queue stats: Errors will be displayed below if found. Just a few checks are done.'

	--offd+overflin != answ+aband+overflout
	IF EXISTS (SELECT 1	FROM #tmp_queue_logg
		GROUP BY queue,date_from
		HAVING sum(offd_direct_call_cnt)+sum(overflow_in_call_cnt) <> sum(answ_call_cnt) + sum(aband_call_cnt) + sum(overflow_out_call_cnt))
	BEGIN
		SELECT 'Error: Offered + Overflow in is not equal to Answered + Abandoned + Overflow out'
		SELECT queue, date_from, sum(offd_direct_call_cnt)offd_direct_call_cnt,sum(overflow_in_call_cnt)overflow_in_call_cnt, sum(answ_call_cnt)answ_call_cnt,sum(aband_call_cnt)aband_call_cnt,sum(overflow_out_call_cnt)overflow_out_call_cnt
		FROM #tmp_queue_logg
		GROUP BY queue,date_from
		HAVING sum(offd_direct_call_cnt)+sum(overflow_in_call_cnt) <> sum(answ_call_cnt) + sum(aband_call_cnt) + sum(overflow_out_call_cnt)

	END

	--answ/aband within sl > answ/aband cnt
	IF EXISTS (SELECT 1 FROM #tmp_queue_logg t WHERE ans_servicelevel_cnt > answ_call_cnt
					OR aband_short_call_cnt + aband_within_sl_cnt > aband_call_cnt)
	BEGIN 
		SELECT 'Error: 1. Answ within SL cannot be higher than total answered. 2. Aband short calls + aband within SL cannot be higher than total abandoned.'
		SELECT * FROM #tmp_queue_logg t WHERE ans_servicelevel_cnt > answ_call_cnt
					OR aband_short_call_cnt + aband_within_sl_cnt > aband_call_cnt
	END

	--negative values
	IF EXISTS (SELECT 1 FROM #tmp_queue_logg t WHERE [offd_direct_call_cnt]<0 or [overflow_in_call_cnt]<0 or 
      [aband_call_cnt]<0 or [overflow_out_call_cnt]<0 or [answ_call_cnt]<0 or [queued_and_answ_call_dur]<0 or 
      [queued_and_aband_call_dur]<0 or [talking_call_dur]<0 or [wrap_up_dur]<0 or [queued_answ_longest_que_dur]<0 or 
      [queued_aband_longest_que_dur]<0 or [avg_avail_member_cnt]<0 or [ans_servicelevel_cnt]<0 or [wait_dur]<0 or 
      [aband_short_call_cnt]<0 or [aband_within_sl_cnt]<0)
	BEGIN 
		SELECT 'Error: Negative values:'
		SELECT * FROM #tmp_queue_logg t WHERE [offd_direct_call_cnt]<0 or [overflow_in_call_cnt]<0 or 
		  [aband_call_cnt]<0 or [overflow_out_call_cnt]<0 or [answ_call_cnt]<0 or [queued_and_answ_call_dur]<0 or 
		  [queued_and_aband_call_dur]<0 or [talking_call_dur]<0 or [wrap_up_dur]<0 or [queued_answ_longest_que_dur]<0 or 
		  [queued_aband_longest_que_dur]<0 or [avg_avail_member_cnt]<0 or [ans_servicelevel_cnt]<0 or [wait_dur]<0 or 
		  [aband_short_call_cnt]<0 or [aband_within_sl_cnt]<0
	END

END

IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION
	RETURN
END
/**********************************************************************/
/* Add data from #tmp_queue_logg into queue_logg	*/
/**********************************************************************/


IF (@CTI_interval_per_hour*4=@interval_per_hour)
BEGIN
select '@CTI_interval_per_hour*4=@interval_per_hour'

--091013 incorrect split
	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval,
		offd_direct_call_cnt/4+offd_direct_call_cnt%4,
		overflow_in_call_cnt/4+overflow_in_call_cnt%4,
		aband_call_cnt/4+aband_call_cnt%4,
		overflow_out_call_cnt/4+overflow_out_call_cnt%4,
		answ_call_cnt/4+answ_call_cnt%4,
		queued_and_answ_call_dur/4+queued_and_answ_call_dur%4,
		queued_and_aband_call_dur/4+queued_and_aband_call_dur%4,
		talking_call_dur/4+talking_call_dur%4,
		wrap_up_dur/4+wrap_up_dur%4,
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		ans_servicelevel_cnt/4+ans_servicelevel_cnt%4,
		wait_dur/4+wait_dur%4,
		aband_short_call_cnt/4+aband_short_call_cnt%4,
		aband_within_sl_cnt/4+aband_within_sl_cnt%4
				
		FROM #tmp_queue_logg

	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval+1,
		offd_direct_call_cnt/4,
		overflow_in_call_cnt/4,
		aband_call_cnt/4,
		overflow_out_call_cnt/4,
		answ_call_cnt/4,
		queued_and_answ_call_dur/4,
		queued_and_aband_call_dur/4,
		talking_call_dur/4,
		wrap_up_dur/4,
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		ans_servicelevel_cnt/4,
		wait_dur/4,
		aband_short_call_cnt/4,
		aband_within_sl_cnt/4
				
		FROM #tmp_queue_logg

	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval+2,
		offd_direct_call_cnt/4,
		overflow_in_call_cnt/4,
		aband_call_cnt/4,
		overflow_out_call_cnt/4,
		answ_call_cnt/4,
		queued_and_answ_call_dur/4,
		queued_and_aband_call_dur/4,
		talking_call_dur/4,
		wrap_up_dur/4,
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		ans_servicelevel_cnt/4,
		wait_dur/4,
		aband_short_call_cnt/4,
		aband_within_sl_cnt/4
				
		FROM #tmp_queue_logg

	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval+3,
		offd_direct_call_cnt/4,
		overflow_in_call_cnt/4,
		aband_call_cnt/4,
		overflow_out_call_cnt/4,
		answ_call_cnt/4,
		queued_and_answ_call_dur/4,
		queued_and_aband_call_dur/4,
		talking_call_dur/4,
		wrap_up_dur/4,
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		ans_servicelevel_cnt/4,
		wait_dur/4,
		aband_short_call_cnt/4,
		aband_within_sl_cnt/4
				
		FROM #tmp_queue_logg



END

IF (@CTI_interval_per_hour*6=@interval_per_hour)
BEGIN
select '@CTI_interval_per_hour*6=@interval_per_hour'
	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval,
		round(convert(real,offd_direct_call_cnt)/6.0,0),
		round(convert(real,overflow_in_call_cnt)/6.0,0),
		round(convert(real,aband_call_cnt)/6.0,0),
		round(convert(real,overflow_out_call_cnt)/6.0,0),
		round(convert(real,answ_call_cnt)/6.0,0),
		round(convert(real,queued_and_answ_call_dur)/6.0,0),
		round(convert(real,queued_and_aband_call_dur)/6.0,0),
		round(convert(real,talking_call_dur)/6.0,0),
		round(convert(real,wrap_up_dur)/6.0,0),
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		round(convert(real,ans_servicelevel_cnt)/6.0,0),
		round(convert(real,wait_dur)/6.0,0),
		round(convert(real,aband_short_call_cnt)/6.0,0),
		round(convert(real,aband_within_sl_cnt)/6.0,0)
				
		FROM #tmp_queue_logg

	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval + 1,
		round(convert(real,offd_direct_call_cnt)/6.0,0),
		round(convert(real,overflow_in_call_cnt)/6.0,0),
		round(convert(real,aband_call_cnt)/6.0,0),
		round(convert(real,overflow_out_call_cnt)/6.0,0),
		round(convert(real,answ_call_cnt)/6.0,0),
		round(convert(real,queued_and_answ_call_dur)/6.0,0),
		round(convert(real,queued_and_aband_call_dur)/6.0,0),
		round(convert(real,talking_call_dur)/6.0,0),
		round(convert(real,wrap_up_dur)/6.0,0),
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		round(convert(real,ans_servicelevel_cnt)/6.0,0),
		round(convert(real,wait_dur)/6.0,0),
		round(convert(real,aband_short_call_cnt)/6.0,0),
		round(convert(real,aband_within_sl_cnt)/6.0,0)
				
		FROM #tmp_queue_logg

	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval + 2,
		round(convert(real,offd_direct_call_cnt)/6.0,0),
		round(convert(real,overflow_in_call_cnt)/6.0,0),
		round(convert(real,aband_call_cnt)/6.0,0),
		round(convert(real,overflow_out_call_cnt)/6.0,0),
		round(convert(real,answ_call_cnt)/6.0,0),
		round(convert(real,queued_and_answ_call_dur)/6.0,0),
		round(convert(real,queued_and_aband_call_dur)/6.0,0),
		round(convert(real,talking_call_dur)/6.0,0),
		round(convert(real,wrap_up_dur)/6.0,0),
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		round(convert(real,ans_servicelevel_cnt)/6.0,0),
		round(convert(real,wait_dur)/6.0,0),
		round(convert(real,aband_short_call_cnt)/6.0,0),
		round(convert(real,aband_within_sl_cnt)/6.0,0)
				
		FROM #tmp_queue_logg

	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval + 3,
		round(convert(real,offd_direct_call_cnt)/6.0,0),
		round(convert(real,overflow_in_call_cnt)/6.0,0),
		round(convert(real,aband_call_cnt)/6.0,0),
		round(convert(real,overflow_out_call_cnt)/6.0,0),
		round(convert(real,answ_call_cnt)/6.0,0),
		round(convert(real,queued_and_answ_call_dur)/6.0,0),
		round(convert(real,queued_and_aband_call_dur)/6.0,0),
		round(convert(real,talking_call_dur)/6.0,0),
		round(convert(real,wrap_up_dur)/6.0,0),
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		round(convert(real,ans_servicelevel_cnt)/6.0,0),
		round(convert(real,wait_dur)/6.0,0),
		round(convert(real,aband_short_call_cnt)/6.0,0),
		round(convert(real,aband_within_sl_cnt)/6.0,0)
				
		FROM #tmp_queue_logg


	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval + 4,
		ceiling(convert(real,offd_direct_call_cnt)/6.0),
		ceiling(convert(real,overflow_in_call_cnt)/6.0),
		ceiling(convert(real,aband_call_cnt)/6.0),
		ceiling(convert(real,overflow_out_call_cnt)/6.0),
		ceiling(convert(real,answ_call_cnt)/6.0),
		ceiling(convert(real,queued_and_answ_call_dur)/6.0),
		ceiling(convert(real,queued_and_aband_call_dur)/6.0),
		ceiling(convert(real,talking_call_dur)/6.0),
		ceiling(convert(real,wrap_up_dur)/6.0),
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		ceiling(convert(real,ans_servicelevel_cnt)/6.0),
		ceiling(convert(real,wait_dur)/6.0),
		ceiling(convert(real,aband_short_call_cnt)/6.0),
		ceiling(convert(real,aband_within_sl_cnt)/6.0)

		FROM #tmp_queue_logg

	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval + 5,
		offd_direct_call_cnt/6,
		overflow_in_call_cnt/6,
		aband_call_cnt/6,
		overflow_out_call_cnt/6,
		answ_call_cnt/6,
		queued_and_answ_call_dur/6,
		queued_and_aband_call_dur/6,
		talking_call_dur/6,
		wrap_up_dur/6,
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		ans_servicelevel_cnt/6,
		wait_dur/6,
		aband_short_call_cnt/6,
		aband_within_sl_cnt/6
				
		FROM #tmp_queue_logg
END


IF (@CTI_interval_per_hour*2=@interval_per_hour)
BEGIN
select '@CTI_interval_per_hour*2=@interval_per_hour'
	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval,
		ceiling(convert(real,offd_direct_call_cnt)/2.0),
		ceiling(convert(real,overflow_in_call_cnt)/2.0),
		ceiling(convert(real,aband_call_cnt)/2),
		ceiling(convert(real,overflow_out_call_cnt)/2.0),
		CASE WHEN answ_call_cnt = 1 THEN
			answ_call_cnt
		ELSE
			ceiling(convert(real,answ_call_cnt)/2.0)
		END,
		--ceiling(convert(real,answ_call_cnt)/2.0),
		ceiling(convert(real,queued_and_answ_call_dur)/2.0),
		ceiling(convert(real,queued_and_aband_call_dur)/2.0),
		CASE WHEN answ_call_cnt = 1 THEN
			talking_call_dur
		ELSE
			ceiling(convert(real,talking_call_dur)/2.0)
		END,
		--ceiling(convert(real,talking_call_dur)/2.0),
		CASE WHEN answ_call_cnt = 1 THEN
			wrap_up_dur
		ELSE
			ceiling(convert(real,wrap_up_dur)/2.0)
		END,
		--ceiling(convert(real,wrap_up_dur)/2.0),
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		ceiling(convert(real,ans_servicelevel_cnt)/2.0),
		ceiling(convert(real,wait_dur)/2.0),
		ceiling(convert(real,aband_short_call_cnt)/2.0),
		ceiling(convert(real,aband_within_sl_cnt)/2.0)

		FROM #tmp_queue_logg

	INSERT INTO queue_logg 
	SELECT

		queue,
		date_from,
		interval+1,
		offd_direct_call_cnt/2,
		overflow_in_call_cnt/2,
		aband_call_cnt/2,
		overflow_out_call_cnt/2,
		CASE WHEN answ_call_cnt = 1 THEN
			0
		ELSE
			answ_call_cnt/2
		END,
		--answ_call_cnt/2,
		queued_and_answ_call_dur/2,
		queued_and_aband_call_dur/2,
		CASE WHEN answ_call_cnt = 1 THEN
			0
		ELSE
			talking_call_dur/2
		END,
		--talking_call_dur/2,
		CASE WHEN answ_call_cnt = 1 THEN
			0
		ELSE
			wrap_up_dur/2
		END,
		--wrap_up_dur/2,
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		ans_servicelevel_cnt/2,
		wait_dur/2,
		aband_short_call_cnt/2,
		aband_within_sl_cnt/2
				
		FROM #tmp_queue_logg
END

IF (@CTI_interval_per_hour*3=@interval_per_hour)
BEGIN
select '@CTI_interval_per_hour*3=@interval_per_hour'
	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval,
		round(convert(real,offd_direct_call_cnt)/3.0,0),
		round(convert(real,overflow_in_call_cnt)/3.0,0),
		round(convert(real,aband_call_cnt)/3.0,0),
		round(convert(real,overflow_out_call_cnt)/3.0,0),
		round(convert(real,answ_call_cnt)/3.0,0),
		round(convert(real,queued_and_answ_call_dur)/3.0,0),
		round(convert(real,queued_and_aband_call_dur)/3.0,0),
		round(convert(real,talking_call_dur)/3.0,0),
		round(convert(real,wrap_up_dur)/3.0,0),
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		round(convert(real,ans_servicelevel_cnt)/3.0,0),
		round(convert(real,wait_dur)/3.0,0),
		round(convert(real,aband_short_call_cnt)/3.0,0),
		round(convert(real,aband_within_sl_cnt)/3.0,0)
				
		FROM #tmp_queue_logg

	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval + 1,
		ceiling(convert(real,offd_direct_call_cnt)/3.0),
		ceiling(convert(real,overflow_in_call_cnt)/3.0),
		ceiling(convert(real,aband_call_cnt)/3.0),
		ceiling(convert(real,overflow_out_call_cnt)/3.0),
		ceiling(convert(real,answ_call_cnt)/3.0),
		ceiling(convert(real,queued_and_answ_call_dur)/3.0),
		ceiling(convert(real,queued_and_aband_call_dur)/3.0),
		ceiling(convert(real,talking_call_dur)/3.0),
		ceiling(convert(real,wrap_up_dur)/3.0),
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		ceiling(convert(real,ans_servicelevel_cnt)/3.0),
		ceiling(convert(real,wait_dur)/3.0),
		ceiling(convert(real,aband_short_call_cnt)/3.0),
		ceiling(convert(real,aband_within_sl_cnt)/3.0)

		FROM #tmp_queue_logg

	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval + 2,
		offd_direct_call_cnt/3,
		overflow_in_call_cnt/3,
		aband_call_cnt/3,
		overflow_out_call_cnt/3,
		answ_call_cnt/3,
		queued_and_answ_call_dur/3,
		queued_and_aband_call_dur/3,
		talking_call_dur/3,
		wrap_up_dur/3,
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		ans_servicelevel_cnt/3,
		wait_dur/3,
		aband_short_call_cnt/3,
		aband_within_sl_cnt/3
				
		FROM #tmp_queue_logg
END



IF (@CTI_interval_per_hour*1.5=@interval_per_hour)
BEGIN
select '@CTI_interval_per_hour*1.5=@interval_per_hour'



CREATE TABLE #tmp_queue_logg2 (
	queue int NOT NULL,
	date_from smalldatetime NOT NULL,
	interval int NOT NULL,
	offd_direct_call_cnt int NULL,
	overflow_in_call_cnt int NULL,
	aband_call_cnt int NULL,
	overflow_out_call_cnt int NULL,
	answ_call_cnt int NULL,
	queued_and_answ_call_dur int NULL,
	queued_and_aband_call_dur int NULL,
	talking_call_dur int NULL,
	wrap_up_dur int NULL,
	queued_answ_longest_que_dur int NULL,
	queued_aband_longest_que_dur int NULL,
	avg_avail_member_cnt int NULL,
	ans_servicelevel_cnt int NULL,
	wait_dur int NULL,
	aband_short_call_cnt int NULL,
	aband_within_sl_cnt int NULL)

	INSERT INTO #tmp_queue_logg2
	SELECT
		queue,
		date_from,
		interval*3,
		round(convert(real,offd_direct_call_cnt)/3.0,0),
		round(convert(real,overflow_in_call_cnt)/3.0,0),
		round(convert(real,aband_call_cnt)/3.0,0),
		round(convert(real,overflow_out_call_cnt)/3.0,0),
		round(convert(real,answ_call_cnt)/3.0,0),
		round(convert(real,queued_and_answ_call_dur)/3.0,0),
		round(convert(real,queued_and_aband_call_dur)/3.0,0),
		round(convert(real,talking_call_dur)/3.0,0),
		round(convert(real,wrap_up_dur)/3.0,0),
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		round(convert(real,ans_servicelevel_cnt)/3.0,0),
		round(convert(real,wait_dur)/3.0,0),
		round(convert(real,aband_short_call_cnt)/3.0,0),
		round(convert(real,aband_within_sl_cnt)/3.0,0)
				
		FROM #tmp_queue_logg

	INSERT INTO #tmp_queue_logg2 
	SELECT
		queue,
		date_from,
		interval*3 + 1,
		ceiling(convert(real,offd_direct_call_cnt)/3.0),
		ceiling(convert(real,overflow_in_call_cnt)/3.0),
		ceiling(convert(real,aband_call_cnt)/3.0),
		ceiling(convert(real,overflow_out_call_cnt)/3.0),
		ceiling(convert(real,answ_call_cnt)/3.0),
		ceiling(convert(real,queued_and_answ_call_dur)/3.0),
		ceiling(convert(real,queued_and_aband_call_dur)/3.0),
		ceiling(convert(real,talking_call_dur)/3.0),
		ceiling(convert(real,wrap_up_dur)/3.0),
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		ceiling(convert(real,ans_servicelevel_cnt)/3.0),
		ceiling(convert(real,wait_dur)/3.0),
		ceiling(convert(real,aband_short_call_cnt)/3.0),
		ceiling(convert(real,aband_within_sl_cnt)/3.0)

		FROM #tmp_queue_logg

	INSERT INTO #tmp_queue_logg2 
	SELECT
		queue,
		date_from,
		interval * 3 + 2,
		offd_direct_call_cnt/3,
		overflow_in_call_cnt/3,
		aband_call_cnt/3,
		overflow_out_call_cnt/3,
		answ_call_cnt/3,
		queued_and_answ_call_dur/3,
		queued_and_aband_call_dur/3,
		talking_call_dur/3,
		wrap_up_dur/3,
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		ans_servicelevel_cnt/3,
		wait_dur/3,
		aband_short_call_cnt/3,
		aband_within_sl_cnt/3
				
		FROM #tmp_queue_logg


--INSERT INTO queue_logg
--select 'logg', * from #tmp_queue_logg
--	select 'logg2',* from #tmp_queue_logg2	
		
		INSERT INTO queue_logg
		SELECT 	queue,
		date_from,
		interval /2,

		SUM(offd_direct_call_cnt),
		SUM(overflow_in_call_cnt),
		SUM(aband_call_cnt),
		SUM(overflow_out_call_cnt),
		SUM(answ_call_cnt),
		SUM(queued_and_answ_call_dur),
		SUM(queued_and_aband_call_dur),
		SUM(talking_call_dur),
		SUM(wrap_up_dur),
		MAX(queued_answ_longest_que_dur),
		MAX(queued_aband_longest_que_dur),
		MAX(avg_avail_member_cnt),
		SUM(ans_servicelevel_cnt),
		SUM(wait_dur),
		SUM(aband_short_call_cnt),
		SUM(aband_within_sl_cnt)
				
		FROM #tmp_queue_logg2
		GROUP BY queue,
		date_from,
		interval /2

order by queue,interval/2



END

--merge 2:1
IF (@CTI_interval_per_hour/2=@interval_per_hour)
BEGIN
select '@CTI_interval_per_hour/2=@interval_per_hour'
	INSERT INTO queue_logg
	SELECT      queue,
	date_from,
	interval/2,
	SUM(offd_direct_call_cnt),
	SUM(overflow_in_call_cnt),
	SUM(aband_call_cnt),
	SUM(overflow_out_call_cnt),
	SUM(answ_call_cnt),
	SUM(queued_and_answ_call_dur),
	SUM(queued_and_aband_call_dur),
	SUM(talking_call_dur),
	SUM(wrap_up_dur),
	MAX(queued_answ_longest_que_dur),
	MAX(queued_aband_longest_que_dur),
	MAX(avg_avail_member_cnt),
	SUM(ans_servicelevel_cnt),
	SUM(wait_dur),
	SUM(aband_short_call_cnt),
	SUM(aband_within_sl_cnt)
	FROM #tmp_queue_logg
	GROUP BY queue,
	date_from,
	interval/2
END


IF (@CTI_interval_per_hour=@interval_per_hour)
BEGIN
select '@CTI_interval_per_hour=@interval_per_hour'
	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval,
		offd_direct_call_cnt,
		overflow_in_call_cnt,
		aband_call_cnt,
		overflow_out_call_cnt,
		answ_call_cnt,
		queued_and_answ_call_dur,
		queued_and_aband_call_dur,
		talking_call_dur,
		wrap_up_dur,
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		ans_servicelevel_cnt,
		wait_dur,
		aband_short_call_cnt,
		aband_within_sl_cnt
				
		FROM #tmp_queue_logg
END
IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION
	RETURN
END
/********************************************************/
/* Update log_object_detail	                      */
/********************************************************/
--DECLARE @maxdate smalldatetime, @maxinterval int
IF (@start_date < '1970-01-01') AND (@end_date < '1970-01-01')
BEGIN
	SELECT @maxdate = MAX(date_from) FROM #tmp_queue_logg
	SELECT @maxinterval = MAX(interval) FROM #tmp_queue_logg
	WHERE date_from = @maxdate
	IF  @maxdate IS not NULL
	BEGIN
   		UPDATE log_object_detail
		SET date_value = @maxdate,
	            int_value = @maxinterval 
		WHERE log_object_id = @log_object_id
		AND detail_id = 1
	
	END
END
ELSE
BEGIN
	SELECT @maxdate = MAX(date_from) FROM queue_logg
	SELECT @maxinterval = MAX(interval) FROM queue_logg
	WHERE date_from = @maxdate
	IF  @maxdate IS not NULL
	BEGIN
   		UPDATE log_object_detail
		SET date_value = @maxdate,
	            int_value = @maxinterval 
		WHERE log_object_id = @log_object_id
		AND detail_id = 1
	
	END    
END

--090416
IF @maxdate IS NULL
BEGIN 
	SELECT 'No queue_logg stats received from TeleoptiLogDb from time set in log_object_detail and one month forward.'	
END


IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION
	RETURN
END

COMMIT TRANSACTION
RETURN

SET NOCOUNT OFF


GO

