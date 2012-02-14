IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_sts_insert_agent_state_logg]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_sts_insert_agent_state_logg]
GO

CREATE PROCEDURE [dbo].[p_sts_insert_agent_state_logg]
@log_object_id int

AS

DECLARE
@last_logg_date smalldatetime,
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
SELECT @last_logg_date = date_value , @last_logg_interval = int_value FROM log_object_detail  WHERE log_object_id = @log_object_id AND detail_id = 4
SELECT @minutes_per_interval = (1440/int_value)  FROM ccc_system_info WHERE [id]=1
SELECT @interval_per_hour = 60/@minutes_per_interval
SELECT @CTI_minutes_per_interval = (1440/intervals_per_day) FROM log_object  WHERE log_object_id = @log_object_id
SELECT @CTI_interval_per_hour = 60/@CTI_minutes_per_interval
SELECT @last_logg_date_time = dateadd(minute,@last_logg_interval*@minutes_per_interval,@last_logg_date)
SELECT @stop_date = dateadd(month,1,@last_logg_date)
SELECT @acd_type=acd_type_id FROM log_object WHERE log_object_id = @log_object_id


/*********************************************************************************/
/* Adjust for different time zones between log_object and T-CCC?      */
/* If so, last logg interval might need adjustment.                                 */
/*********************************************************************************/
DECLARE @max_add_hours int

SELECT @max_add_hours=max(add_hours) FROM log_object_add_hours WHERE log_object_id = @log_object_id

IF @max_add_hours IS NOT NULL
BEGIN
	SELECT @last_logg_date = convert(smalldatetime,convert(varchar(10), dateadd(minute,@minutes_per_interval*(@last_logg_interval-@max_add_hours*@interval_per_hour),@last_logg_date),120))
	SELECT @last_logg_interval = @interval_per_hour*datepart(hour,dateadd(minute,@minutes_per_interval*(@last_logg_interval-@max_add_hours*@interval_per_hour),'1971-01-01'))
					+datepart(minute,dateadd(minute,@minutes_per_interval*(@last_logg_interval-@max_add_hours*@interval_per_hour),'1971-01-01'))/@minutes_per_interval
	SELECT @last_logg_date_time = dateadd(minute,@last_logg_interval*@minutes_per_interval,@last_logg_date)
END
/**********************************************************************/
/* Create a temporary table wich we will add data to	*/
/**********************************************************************/
CREATE TABLE #tmp_agent_state_logg
(
	date_from smalldatetime not null,
	state_id int not null,
	agent_id int not null,
	interval int not null,
	state_dur int null
)	

/***************************************************************/
/* Execute the correct procedure for this log object	   */
/***************************************************************/

DECLARE @txt varchar(8000)
SELECT @txt = (SELECT logdb_name FROM log_object WHERE log_object_id = @log_object_id)+'..'+
	(SELECT proc_name FROM log_object_detail WHERE log_object_id = @log_object_id AND detail_id = 4)+'
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
	DELETE FROM #tmp_agent_state_logg
	FROM #tmp_agent_state_logg a
	WHERE NOT EXISTS (SELECT 1 FROM log_object_add_hours b
		WHERE dateadd(minute,@minutes_per_interval*a.interval,a.date_from)
			BETWEEN b.datetime_from AND b.datetime_to
		AND b.log_object_id = @log_object_id)

	UPDATE #tmp_agent_state_logg SET
		date_from = convert(smalldatetime,convert(varchar(10),dateadd(minute,@minutes_per_interval*(a.interval+b.add_hours*@interval_per_hour),a.date_from),120)),
		interval = @interval_per_hour*datepart(hour,dateadd(minute,@minutes_per_interval*(a.interval+b.add_hours*@interval_per_hour),'1971-01-01'))+
		datepart(minute,dateadd(minute,@minutes_per_interval*(a.interval+b.add_hours*@interval_per_hour),'1971-01-01'))/@minutes_per_interval
	FROM #tmp_agent_state_logg a INNER JOIN log_object_add_hours b ON dateadd(minute,a.interval*@minutes_per_interval,a.date_from) BETWEEN b.datetime_from AND b.datetime_to
END

/**********************************************************************/
/*    This is where the actual insert to the real tables start!	*/
/**********************************************************************/
BEGIN TRANSACTION 
/**********************************************************************/
/*   Ta bort det senaste loggintervallet i agent_state_logg	*/
/**********************************************************************/
DECLARE @mindate smalldatetime, @mininterval int
SELECT @mindate=MIN(date_from) from #tmp_agent_state_logg
SELECT @mininterval=MIN(interval) FROM #tmp_agent_state_logg WHERE date_from = @mindate
IF  @mindate IS not NULL
BEGIN
	DELETE FROM agent_state_logg
	WHERE (date_from > @mindate
		OR 	(date_from = @mindate
			AND interval >= @mininterval))
	AND exists 	(SELECT 1 FROM agent_states states
			WHERE	states.state_id = agent_state_logg.state_id
			AND	states.log_object_id = @log_object_id)
END
/**************************************************/
/*       insert into agent_state_logg	*/	
/**************************************************/
-- CTI 60 minuter AGG 15 minuter
IF (@CTI_interval_per_hour*4=@interval_per_hour)
BEGIN
	select '@CTI_interval_per_hour*4=@interval_per_hour'
	INSERT agent_state_logg (
	date_from,
	state_id,
	agent_id,
	interval,
	state_dur)
	SELECT
	date_from,
	state_id,
	agent_id,
	interval,
	round(convert(real,sum(state_dur))/4.0,0)
	FROM #tmp_agent_state_logg
	GROUP BY date_from,state_id,agent_id,interval

	INSERT agent_state_logg (
	date_from,
	state_id,
	agent_id,
	interval,
	state_dur)
	SELECT
	date_from,
	state_id,
	agent_id,
	interval + 1,
	round(convert(real,sum(state_dur))/4.0,0)
	FROM #tmp_agent_state_logg
	GROUP BY date_from,state_id,agent_id,interval

	INSERT agent_state_logg (
	date_from,
	state_id,
	agent_id,
	interval,
	state_dur)
	SELECT
	date_from,
	state_id,
	agent_id,
	interval + 2,
	ceiling(convert(real,sum(state_dur))/4.0)
	FROM #tmp_agent_state_logg
	GROUP BY date_from,state_id,agent_id,interval

	INSERT agent_state_logg (
	date_from,
	state_id,
	agent_id,
	interval,
	state_dur)
	SELECT
	date_from,
	state_id,
	agent_id,
	interval + 3,
	sum(state_dur)/4
	FROM #tmp_agent_state_logg
	GROUP BY date_from,state_id,agent_id,interval
END

-- CTI 60 minuter AGG 10 minuter
IF (@CTI_interval_per_hour*6=@interval_per_hour)
BEGIN
	select '@CTI_interval_per_hour*6=@interval_per_hour'
	INSERT agent_state_logg (
	date_from,
	state_id,
	agent_id,
	interval,
	state_dur)
	SELECT
	date_from,
	state_id,
	agent_id,
	interval,
	round(convert(real,sum(state_dur))/6.0,0)
	FROM #tmp_agent_state_logg
	GROUP BY date_from,state_id,agent_id,interval

	INSERT agent_state_logg (
	date_from,
	state_id,
	agent_id,
	interval,
	state_dur)
	SELECT
	date_from,
	state_id,
	agent_id,
	interval + 1,
	round(convert(real,sum(state_dur))/6.0,0)
	FROM #tmp_agent_state_logg
	GROUP BY date_from,state_id,agent_id,interval

	INSERT agent_state_logg (
	date_from,
	state_id,
	agent_id,
	interval,
	state_dur)
	SELECT
	date_from,
	state_id,
	agent_id,
	interval + 2,
	round(convert(real,sum(state_dur))/6.0,0)
	FROM #tmp_agent_state_logg
	GROUP BY date_from,state_id,agent_id,interval

	INSERT agent_state_logg (
	date_from,
	state_id,
	agent_id,
	interval,
	state_dur)
	SELECT
	date_from,
	state_id,
	agent_id,
	interval + 3,
	round(convert(real,sum(state_dur))/6.0,0)
	FROM #tmp_agent_state_logg
	GROUP BY date_from,state_id,agent_id,interval

	INSERT agent_state_logg (
	date_from,
	state_id,
	agent_id,
	interval,
	state_dur)
	SELECT
	date_from,
	state_id,
	agent_id,
	interval + 4,
	ceiling(convert(real,sum(state_dur))/6.0)
	FROM #tmp_agent_state_ogg
	GROUP BY date_from,state_id,agent_id,interval

	INSERT agent_state_logg (
	date_from,
	state_id,
	agent_id,
	interval,
	state_dur)
	SELECT
	date_from,
	state_id,
	agent_id,
	interval + 5,
	sum(state_dur)/6
	FROM #tmp_agent_state_logg
	GROUP BY date_from,state_id,agent_id,interval
END


IF (@CTI_interval_per_hour*2=@interval_per_hour)
BEGIN
	select '@CTI_interval_per_hour*2=@interval_per_hour'
	INSERT agent_state_logg (
	date_from,
	state_id,
	agent_id,
	interval,
	state_dur)
	SELECT
	date_from,
	state_id,
	agent_id,
	interval,
	ceiling(convert(real,sum(state_dur))/2.0)
	FROM #tmp_agent_state_logg
	GROUP BY date_from,state_id,agent_id,interval

	INSERT agent_state_logg (
	date_from,
	state_id,
	agent_id,
	interval,
	state_dur)
	SELECT
	date_from,
	state_id,
	agent_id,
	interval+1,
	sum(state_dur)/2
	FROM #tmp_agent_state_logg
	GROUP BY date_from,state_id,agent_id,interval
END

IF (@CTI_interval_per_hour*3 = @interval_per_hour)
BEGIN
	select '@CTI_interval_per_hour*3=@interval_per_hour'
	INSERT agent_state_logg (
	date_from,
	state_id,
	agent_id,
	interval,
	state_dur)
	SELECT
	date_from,
	state_id,
	agent_id,
	interval,
	round(convert(real,sum(state_dur))/3.0,0)
	FROM #tmp_agent_state_logg
	GROUP BY date_from,state_id,agent_id,interval

	INSERT agent_state_logg (
	date_from,
	state_id,
	agent_id,
	interval,
	state_dur)
	SELECT
	date_from,
	state_id,
	agent_id,
	interval + 1,
	ceiling(convert(real,sum(state_dur))/3.0)
	FROM #tmp_agent_state_logg
	GROUP BY date_from,state_id,agent_id,interval

	INSERT agent_state_logg (
	date_from,
	state_id,
	agent_id,
	interval,
	state_dur)
	SELECT
	date_from,
	state_id,
	agent_id,
	interval+2,
	sum(state_dur)/3
	FROM #tmp_agent_state_logg
	GROUP BY date_from,state_id,agent_id,interval
END

IF (@CTI_interval_per_hour = @interval_per_hour) 
BEGIN
	select 'vanlig'
	INSERT agent_state_logg (
	date_from,
	state_id,
	agent_id,
	interval,
	state_dur)
	SELECT
	date_from,
	state_id,
	agent_id,
	interval,
	sum(state_dur)
	FROM #tmp_agent_state_logg
	GROUP BY date_from,state_id,agent_id,interval
END
IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION 
	RETURN
END
/**************************************************/
/* Uppdatera last_logg			*/
/**************************************************/
DECLARE @maxdate smalldatetime, @maxinterval int
SELECT @maxdate = MAX(date_from) FROM #tmp_agent_state_logg
SELECT @maxinterval = MAX(interval) FROM #tmp_agent_state_logg
WHERE date_from = @maxdate
	IF  @maxdate IS not NULL
	BEGIN
   		UPDATE log_object_detail
		SET date_value = @maxdate,
	            int_value = @maxinterval 
		WHERE log_object_id = @log_object_id
		AND detail_id = 4
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

