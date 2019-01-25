IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_sts_insert_agent_logg]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_sts_insert_agent_logg]
GO

/*************************************/
/* Modified by David P 020130 */
/*************************************/
CREATE PROCEDURE [dbo].[p_sts_insert_agent_logg]
/*
Micke D 2004-02-27 Added 10 minutes intervals on 30 minutes logging
Micke D 2004-08-19 Added 10 minute intervals on 60 minute logging
Micke D 2004-08-19 Added 15 minute intervals on 60 minute logging
*/
@log_object_id int
AS
DECLARE @last_logg_date smalldatetime,
@last_logg_interval int,
@minutes_per_interval int,
@interval_per_hour int,
@CTI_interval_per_hour int,
@CTI_minutes_per_interval int,
@last_logg_date_time datetime,
@stop_date smalldatetime,
@acd_type int
SET NOCOUNT ON
/********************************************************/
/* Fetch latest log date and interval                   */
/********************************************************/
SELECT @last_logg_date = date_value , @last_logg_interval = int_value
FROM log_object_detail 
WHERE log_object_id = @log_object_id
AND detail_id = 2
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
SELECT @stop_date = dateadd(month,1,@last_logg_date)
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
	admin_dur int NULL)		--nytt
/***************************************************************/
/* Execute the correct procedure for this log object	   */
/***************************************************************/
DECLARE @txt varchar(8000)
SELECT @txt = (SELECT logdb_name FROM log_object WHERE log_object_id = @log_object_id)+'..'+
	(SELECT proc_name FROM log_object_detail WHERE log_object_id = @log_object_id AND detail_id = 2)+'
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
/*   Ta bort det senaste loggintervallet i agent_Logg	*/
/**********************************************************************/
DECLARE @mindate smalldatetime, @mininterval int
SELECT @mindate=MIN(date_from) from #tmp_alogg
SELECT @mininterval=MIN(interval) FROM #tmp_alogg
WHERE date_from = @mindate
IF  @mindate IS not NULL
BEGIN
	DELETE FROM agent_logg
	WHERE (date_from > @mindate
		OR 	(date_from = @mindate
			AND interval >= @mininterval))
	AND exists 	(SELECT 1 FROM queues queues
			WHERE	queues.queue = agent_logg.queue
			AND	queues.log_object_id = @log_object_id)
END
/**************************************************/
/*       insert into agent_logg		*/	
/**************************************************/
-- CTI 60 minuter AGG 15 minuter
IF (@CTI_interval_per_hour*4=@interval_per_hour)
BEGIN
select '@CTI_interval_per_hour*4=@interval_per_hour'
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
	transfer_out_call_cnt,
	admin_dur )
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
	agent_name,
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
	interval + 2,
	agent_id, 
	agent_name,
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
	interval + 3,
	agent_id, 
	agent_name,
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
	interval + 4,
	agent_id, 
	agent_name,
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
	interval + 5,
	agent_id, 
	agent_name,
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
	GROUP BY queue,date_from,interval,agent_id,agent_name
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
	agent_name,
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
	interval+1,
	agent_id, 
	agent_name,
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
	GROUP BY queue,date_from,interval,agent_id,agent_name
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
	agent_name,
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
	interval+2,
	agent_id, 
	agent_name,
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
	GROUP BY queue,date_from,interval,agent_id,agent_name
END

IF (@CTI_interval_per_hour = @interval_per_hour) 
BEGIN
select 'vanlig'
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
	GROUP BY queue,date_from,interval,agent_id,agent_name
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
DECLARE @maxdate smalldatetime, @maxinterval int
SELECT @maxdate = MAX(date_from) FROM #tmp_alogg
SELECT @maxinterval = MAX(interval) FROM #tmp_alogg
WHERE date_from = @maxdate
	IF  @maxdate IS not NULL
	BEGIN

   		UPDATE log_object_detail
		SET date_value = @maxdate,
	            int_value = @maxinterval 
		WHERE log_object_id = @log_object_id
		AND detail_id = 2
	END    
IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION 
	RETURN
END
/******** NEW 5.2 Business Scorecard ********/
/*DECLARE @text varchar(500)
SELECT @text = 'p_insert_agent_by_day_logg '+convert(varchar(10),@log_object_id)+
	', '+'
	'+''''+convert(varchar(10),@last_logg_date,120)+''''
EXECUTE (@text)


IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION 
	RETURN
END*/

--admin_dur
select @txt = 	'p_sts_update_agent_logg ' + 
		convert(varchar(10),@log_object_id) + ','  
		+ '' + ''''+convert(varchar(10),@last_logg_date,120)+'''' + ',' 
		+ convert(varchar(10),@last_logg_interval)
execute (@txt)

IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION 
	RETURN
END
COMMIT TRANSACTION
SET NOCOUNT OFF

GO

