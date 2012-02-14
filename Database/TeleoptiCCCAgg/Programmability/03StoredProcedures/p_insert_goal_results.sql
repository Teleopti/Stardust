IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_insert_goal_results]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_insert_goal_results]
GO

/***************************************/
/* Modified by DavidP 020130 */
/*****************************/
CREATE PROCEDURE [dbo].[p_insert_goal_results]
/*
Micke D 2004-02-27 Added 10 minutes intervals on 30 minutes logging
Micke D 2004-08-19 Added 10 minute intervals on 60 minute logging
Micke D 2004-08-19 Added 15 minute intervals on 60 minute logging
henrikl 2009-04-16 Various bug fixes (some not tested), debug mode for simple 
	error checking of aggregated data etc. Changes marked with: --090416
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
AND detail_id = 3

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
/* Create a temporary table wich we will add data to	              */
/**********************************************************************/
CREATE TABLE #tmp_goal_results (
	queue int NOT NULL ,
	date_from smalldatetime NOT NULL ,
	interval int NOT NULL ,
	goal_id int NULL ,
	answ_call_cnt int NULL ,
	aband_call_cnt int NULL)


/***************************************************************/
/* Execute the correct procedure for this log object. 	   */
/***************************************************************/
DECLARE @txt varchar(8000)
SELECT @txt = (SELECT logdb_name FROM log_object WHERE log_object_id = @log_object_id)+'..'+
	(SELECT proc_name FROM log_object_detail WHERE log_object_id = @log_object_id AND detail_id = 3)+'
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
--SELECT @txt
--RETURN
EXECUTE(@txt)
/*********************************************************************************/
/* Adjust for different time zones between log_object and T-CCC?      */
/* If so, interval and date_from might need adjustment.                       */
/*********************************************************************************/
IF @max_add_hours IS NOT NULL
BEGIN
DELETE FROM #tmp_goal_results
FROM #tmp_goal_results a
WHERE NOT EXISTS (SELECT 1 FROM log_object_add_hours b
		WHERE dateadd(minute,@minutes_per_interval*a.interval,a.date_from)
			BETWEEN b.datetime_from AND b.datetime_to
		AND b.log_object_id = @log_object_id)
UPDATE #tmp_goal_results SET
	date_from = convert(smalldatetime,convert(varchar(10),dateadd(minute,@minutes_per_interval*(a.interval+b.add_hours*@interval_per_hour),a.date_from),120)),
	interval = @interval_per_hour*datepart(hour,dateadd(minute,@minutes_per_interval*(a.interval+b.add_hours*@interval_per_hour),'1971-01-01'))+
	datepart(minute,dateadd(minute,@minutes_per_interval*(a.interval+b.add_hours*@interval_per_hour),'1971-01-01'))/@minutes_per_interval
FROM #tmp_goal_results a INNER JOIN log_object_add_hours b ON
	dateadd(minute,a.interval*@minutes_per_interval,a.date_from) BETWEEN b.datetime_from AND b.datetime_to
END
/************************************************************************/
/*  Delete the latest interval in goal_results, and everything later	*/
/************************************************************************/

BEGIN TRANSACTION InsertLoggDP
/*
	Hõr blir det nytt
	
*/


DECLARE @mindate smalldatetime, @mininterval int
DECLARE @maxdate smalldatetime, @maxinterval int

SELECT @mindate=MIN(date_from) from #tmp_goal_results
SELECT @mininterval=MIN(interval) FROM #tmp_goal_results
WHERE date_from = @mindate

SELECT @maxdate=MAX(date_from) from #tmp_goal_results
SELECT @maxinterval=MAX(interval) FROM #tmp_goal_results
WHERE date_from = @maxdate




IF @maxdate > @stop_date
BEGIN
	SELECT @maxdate = @stop_date
	--090416
	SELECT @maxinterval=MAX(interval) FROM #tmp_goal_results
	WHERE date_from = @maxdate
END

/*Hantera konverteringar mellan olika intervall*/
SELECT @maxinterval = @maxinterval + (@interval_per_hour / @CTI_interval_per_hour ) - 1

--090416
--Fix problem when add_hours gives us data for the day after @stop_date
DELETE FROM #tmp_goal_results
WHERE date_from > @maxdate


IF  @mindate IS not NULL
BEGIN

--090416 MK's performance fix
	DELETE FROM goal_results
	WHERE date_From >@mindate and date_from <@maxdate
	AND exists 	(SELECT 1 FROM queues queues
			WHERE	queues.queue = goal_results.queue
			AND	queues.log_object_id = @log_object_id)
	
	DELETE FROM goal_results
	WHERE date_from =@mindate
	AND interval >=@mininterval
		AND exists 	(SELECT 1 FROM queues queues
				WHERE	queues.queue = goal_results.queue
				AND	queues.log_object_id = @log_object_id)
	
	IF @maxdate = @mindate
	BEGIN
		DELETE FROM goal_results
		WHERE date_from = @maxdate
		AND interval <= @maxinterval
		AND interval >=@mininterval
			AND exists 	(SELECT 1 FROM queues queues
					WHERE	queues.queue = goal_results.queue
					AND	queues.log_object_id = @log_object_id)

	END
	ELSE
	BEGIN
		DELETE FROM goal_results
		WHERE date_from = @maxdate
		AND interval <= @maxinterval
			AND exists 	(SELECT 1 FROM queues queues
					WHERE	queues.queue = goal_results.queue
					AND	queues.log_object_id = @log_object_id)
	END
/*
	DELETE FROM goal_results

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
			WHERE	queues.queue = goal_results.queue
			AND	queues.log_object_id = @log_object_id)
*/
END

IF EXISTS (SELECT 1 FROM #tmp_goal_results t
						INNER JOIN goal_results al ON al.queue = t.queue AND al.goal_id = t.goal_id
								AND al.date_from = t.date_from AND al.interval = t.interval)
BEGIN
	SELECT 'There is an error in p_insert_goal_results. The aggregation fails because this data cannot be inserted to goal_results:'
	SELECT * 
	FROM #tmp_goal_results t
	INNER JOIN goal_results al ON al.queue = t.queue AND al.goal_id = t.goal_id
								AND al.date_from = t.date_from AND al.interval = t.interval
END

/**************************************************
090416 Simple error checking of aggregated data	
**************************************************/
IF @debug = 1
BEGIN

	SELECT 'Debug mode - Goal results stats: Errors will be displayed below if found. Just a few checks are done.'

	SELECT date_from, sum(answ_call_cnt) answ_queue_logg, sum(aband_call_cnt) aband_queue_logg
	INTO #tmp_queue
	FROM queue_logg ql
	WHERE ((date_from > @last_logg_date and date_from < @maxdate)
			OR (date_from = @last_logg_date AND interval >= @mininterval)
			OR (date_from = @maxdate AND interval <= @maxinterval))
		AND EXISTS (select 1 from queues q where q.queue = ql.queue and q.log_object_id = @log_object_id)
	GROUP BY date_from
	
	SELECT date_from, sum(answ_call_cnt) answ_goal_results, sum(aband_call_cnt) aband_goal_results
	INTO #tmp_goal
	FROM #tmp_goal_results ql
	WHERE ((date_from > @last_logg_date and date_from < @maxdate)
			OR (date_from = @last_logg_date AND interval >= @mininterval)
			OR (date_from = @maxdate AND interval <= @maxinterval))
		AND EXISTS (select 1 from queues q where q.queue = ql.queue and q.log_object_id = @log_object_id)
	GROUP BY date_from

	-- answ/aband != answ/aband in queue logg
	IF EXISTS (select 1 from #tmp_queue q
				inner join #tmp_goal g on g.date_from=q.date_from
				where q.answ_queue_logg <> g.answ_goal_results or q.aband_queue_logg <> g.aband_goal_results)
	BEGIN
		SELECT 'Answered and/or abandoned calls differ from queue_logg:'
		SELECT * FROM #tmp_goal g
		INNER JOIN #tmp_queue q ON g.date_from=q.date_from
		WHERE q.answ_queue_logg <> g.answ_goal_results or q.aband_queue_logg <> g.aband_goal_results
		ORDER BY g.date_from
	END	
END
/********************************************************/
/* Add data from #tmp_goal_results to goal_results	*/
/********************************************************/
IF (@CTI_interval_per_hour*4 = @interval_per_hour)
BEGIN
	select '@CTI_interval_per_hour*4 = @interval_per_hour'

--091013 incorrect split
INSERT INTO goal_results (
		queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt)
	SELECT	queue,
		date_from,
		interval,
		goal_id,
--changed
		answ_call_cnt/4+answ_call_cnt%4,
		aband_call_cnt/4+aband_call_cnt%4
	FROM #tmp_goal_results
	WHERE	answ_call_cnt > 0
	OR		aband_call_cnt >0

INSERT INTO goal_results (
		queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt)
	SELECT	queue,
		date_from,
		interval + 1,
		goal_id,
		answ_call_cnt/4,
		aband_call_cnt/4
	FROM #tmp_goal_results
	WHERE	answ_call_cnt > 0
	OR		aband_call_cnt >0

INSERT INTO goal_results (
		queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt)
	SELECT	queue,
		date_from,
		interval + 2,
		goal_id,
		answ_call_cnt/4,
		aband_call_cnt/4
--changed
		--ceiling(convert(real,answ_call_cnt)/4.0),
		--ceiling(convert(real,aband_call_cnt)/4.0)
	FROM #tmp_goal_results
	WHERE	answ_call_cnt > 0
	OR		aband_call_cnt >0

INSERT INTO goal_results (
		queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt)
	SELECT	queue,
		date_from,
		interval + 3,
		goal_id,
		answ_call_cnt/4,
		aband_call_cnt/4
	FROM #tmp_goal_results
	WHERE	answ_call_cnt > 0
	OR		aband_call_cnt >0
END

IF (@CTI_interval_per_hour*6 = @interval_per_hour)
BEGIN
	select '@CTI_interval_per_hour*6 = @interval_per_hour'
INSERT INTO goal_results (
		queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt)
	SELECT	queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt/6,
		aband_call_cnt/6
	FROM #tmp_goal_results
	WHERE	answ_call_cnt > 0
	OR		aband_call_cnt >0

INSERT INTO goal_results (
		queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt)
	SELECT	queue,
		date_from,
		interval + 1,
		goal_id,
		answ_call_cnt/6,
		aband_call_cnt/6
	FROM #tmp_goal_results
	WHERE	answ_call_cnt > 0
	OR		aband_call_cnt >0

INSERT INTO goal_results (
		queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt)
	SELECT	queue,
		date_from,
		interval + 2,
		goal_id,
		answ_call_cnt/6,
		aband_call_cnt/6
	FROM #tmp_goal_results
	WHERE	answ_call_cnt > 0
	OR		aband_call_cnt >0

INSERT INTO goal_results (
		queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt)
	SELECT	queue,
		date_from,
		interval + 3,
		goal_id,
		answ_call_cnt/6,
		aband_call_cnt/6
	FROM #tmp_goal_results
	WHERE	answ_call_cnt > 0
	OR		aband_call_cnt >0

INSERT INTO goal_results (
		queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt)
	SELECT	queue,
		date_from,
		interval + 4,
		goal_id,
		ceiling(convert(real,answ_call_cnt)/6.0),
		ceiling(convert(real,aband_call_cnt)/6.0)
	FROM #tmp_goal_results
	WHERE	answ_call_cnt > 0
	OR		aband_call_cnt >0

INSERT INTO goal_results (
		queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt)
	SELECT	queue,
		date_from,
		interval + 5,
		goal_id,
		answ_call_cnt/6,
		aband_call_cnt/6
	FROM #tmp_goal_results
	WHERE	answ_call_cnt > 0
	OR		aband_call_cnt >0
END


IF (@CTI_interval_per_hour*2=@interval_per_hour)
BEGIN
	select '@CTI_interval_per_hour*2=@interval_per_hour'
	INSERT INTO goal_results (
		queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt)
	SELECT	queue,
		date_from,
		interval,
		goal_id,
		ceiling(convert(real,answ_call_cnt)/2.0),
		ceiling(convert(real,aband_call_cnt)/2.0)
	FROM #tmp_goal_results
	WHERE	answ_call_cnt > 0
	OR		aband_call_cnt >0
	INSERT INTO goal_results (
		queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt)
	SELECT	queue,
		date_from,
		interval+1,
		goal_id,
		answ_call_cnt/2,
		aband_call_cnt/2
	FROM #tmp_goal_results
	WHERE	answ_call_cnt > 0
	OR		aband_call_cnt >0
END

IF (@CTI_interval_per_hour*3 = @interval_per_hour)
BEGIN
	select '@CTI_interval_per_hour*3 = @interval_per_hour'
	INSERT INTO goal_results (
		queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt)
	SELECT	queue,
		date_from,
		interval,
		goal_id,
		round(convert(real,answ_call_cnt)/3.0,0),
		round(convert(real,aband_call_cnt)/3.0,0)
	FROM #tmp_goal_results
	WHERE	answ_call_cnt > 0
	OR		aband_call_cnt >0

	INSERT INTO goal_results (
		queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt)
	SELECT	queue,
		date_from,
		interval + 1,
		goal_id,
		ceiling(convert(real,answ_call_cnt)/3.0),
		ceiling(convert(real,aband_call_cnt)/3.0)
	FROM #tmp_goal_results
	WHERE	answ_call_cnt > 0
	OR		aband_call_cnt >0

	INSERT INTO goal_results (
		queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt)
	SELECT	queue,
		date_from,
		interval + 2,
		goal_id,
		answ_call_cnt/3,
		aband_call_cnt/3
	FROM #tmp_goal_results
	WHERE	answ_call_cnt > 0
	OR		aband_call_cnt >0
END




IF (@CTI_interval_per_hour = @interval_per_hour)
BEGIN
--090416
--select 'vanlig'
SELECT '@CTI_interval_per_hour=@interval_per_hour'
	INSERT INTO goal_results (
		queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt)
	SELECT	queue,
		date_from,
		interval,
		goal_id,
		answ_call_cnt,
		aband_call_cnt
	FROM #tmp_goal_results
	WHERE	answ_call_cnt > 0
	OR		aband_call_cnt >0
END
IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION InsertLoggDP
	RETURN
END
/********************************************************/
/* Update log_object_detail	                      */
/********************************************************/
--DECLARE @maxdate smalldatetime, @maxinterval int
IF (@start_date < '1970-01-01') AND (@end_date < '1970-01-01')
BEGIN
	SELECT @maxdate = MAX(date_from) FROM #tmp_goal_results
	SELECT @maxinterval = MAX(interval) FROM #tmp_goal_results
	WHERE date_from = @maxdate
	IF  @maxdate IS not NULL
	BEGIN
   		UPDATE log_object_detail
		SET date_value = @maxdate,
	            int_value = @maxinterval 
		WHERE log_object_id = @log_object_id
		AND detail_id = 3
	
	END
END
ELSE
BEGIN
	SELECT @maxdate = MAX(date_from) FROM goal_results
	SELECT @maxinterval = MAX(interval) FROM goal_results
	WHERE date_from = @maxdate
	IF  @maxdate IS not NULL
	BEGIN
   		UPDATE log_object_detail
		SET date_value = @maxdate,
	            int_value = @maxinterval 
		WHERE log_object_id = @log_object_id
		AND detail_id = 3
	
	END    
END

--090416
IF @maxdate IS NULL
BEGIN 
	SELECT 'No goal_results stats received from TeleoptiLogDb from time set in log_object_detail and one month forward.'	
END

IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION InsertLoggDP
	RETURN
END
COMMIT TRANSACTION InsertLoggDP
SET NOCOUNT OFF

GO

