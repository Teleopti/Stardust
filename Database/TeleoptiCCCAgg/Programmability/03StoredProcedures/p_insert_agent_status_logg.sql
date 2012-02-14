IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_insert_agent_status_logg]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_insert_agent_status_logg]
GO

/*************************************/
/* Modified by David P 020130 */
/*Updated by Magnus Karlsson 2006-01-17*/
/*************************************/
CREATE         PROCEDURE [dbo].[p_insert_agent_status_logg]

@log_object_id int,
@start_date smalldatetime = '1900-01-01',
@end_date smalldatetime = '1900-01-01',
@rel_start_date int =0,
@rel_start_int int=0
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
@start_date anv�nds om man vill agga frin specifikt datum
@end_date anv�nds om man vill agga till ett specifikt datum
@rel_start_date anv�nds om man t ex vill agg bakit en dag hela tiden. Skickar di in -1
@rel_start_int anv�nds om man t ex vill agg bakit fyra intervall hela tiden. Skickar di in -4
*/

/********************************************************/
/* Fetch latest log date and interval                   */
/********************************************************/
SELECT @last_logg_date = date_value , @last_logg_interval = int_value
FROM log_object_detail 
WHERE log_object_id = @log_object_id
AND detail_id = 4

SELECT @stop_date = dateadd(month,1,@last_logg_date)
SELECT @int_per_day= int_value
FROM ccc_system_info 
WHERE [id]=1

/* H�r blir det lite nytt  */
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
			/* H�r ligger intervallet fortfarande inom dagen si det �r bara att plussa */
			SELECT @last_logg_interval = @last_logg_interval + @rel_start_int
		END
		ELSE IF  (@rel_start_int + @last_logg_interval >= @int_per_day)
		BEGIN
			/* H�r har intervallet gitt �ver till n�sta dag vi �kar pi dagen en dag och �ndrar intervallet */
			SELECT @last_logg_date = dateadd ( day,1,@last_logg_date)
			SELECT @last_logg_interval = (@last_logg_interval + @rel_start_int) - (@int_per_day - 1)
		END 
	END 
	ELSE
	BEGIN
		/* H�r �r intervallet igir */
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
CREATE TABLE #tmp_agent_status (
		agent_id nvarchar(50), 
		logon_id varchar(100),
                           time_from datetime,
                           time_to datetime,
                           state int,
                           reason_id int)



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
	DELETE FROM #tmp_agent_status
	FROM #tmp_agent_status a
	WHERE NOT EXISTS (SELECT 1 FROM .log_object_add_hours b
		WHERE a.time_from BETWEEN b.datetime_from AND b.datetime_to
		AND b.log_object_id = @log_object_id)

	UPDATE #tmp_agent_status 
	SET time_from = dateadd(hour,b.add_hours,time_from),
		time_to = dateadd(hour,b.add_hours,time_to)
	FROM #tmp_agent_status a 
		INNER JOIN .log_object_add_hours b ON time_from BETWEEN b.datetime_from AND b.datetime_to
	WHERE log_object_id = @log_object_id

END


/**********************************************************************/
/*    This is where the actual insert to the real tables start!	*/
/**********************************************************************/

BEGIN TRANSACTION 
/**********************************************************************/
/*   Ta bort det senaste loggintervallet i agent_Logg	*/
/**********************************************************************/
/*
	H�r blir det nytt
	
*/

DECLARE @mindate smalldatetime, @mininterval int
DECLARE @maxdate smalldatetime, @maxinterval int
DECLARE @maxtime smalldatetime


SELECT @mindate=MIN(time_from) from #tmp_agent_status
SELECT @maxdate=MAX(time_from) from #tmp_agent_status

IF @maxdate > @stop_date
BEGIN
	SELECT @maxdate = @stop_date
END

IF  @mindate IS not NULL
BEGIN

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
/*
select * from #tmp_agent_status
where agent_id = 116
and time_from between '2006-01-02' and '2006-01-04'
order by time_from
*/

create table #tmp_time(agent_id int,
			time_From datetime)
insert into #tmp_time
select agent_id,min(time_From) from #tmp_Agent_status
group by agent_id



	DELETE FROM .t_agent_status_logg
	FROM .t_agent_status_logg tal
		INNER JOIN #tmp_agent_status tas on tas.agent_id = CONVERT(int,tal.agent_id) 
		INNER JOIN #tmp_time t on t.agent_id=tal.agent_id and tal.time_From >=t.time_from


/*
	DELETE FROM .t_agent_status_logg
	FROM .t_agent_status_logg tal
		INNER JOIN #tmp_agent_status tas on tas.agent_id = CONVERT(int,tal.agent_id) 
	AND tas.time_from = tal.time_from 
	AND tas.state = tal.state
	AND tas.reason_id = tal.reason_id
*/



END



/********************************************************/
/* H�r insertar vi										*/
/********************************************************/



INSERT t_agent_status_logg(agent_id, logon_id, time_from, time_to, state, reason_id)
  SELECT agent_id, logon_id, time_from, time_to, state, reason_id
    FROM #tmp_agent_status

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
	SELECT @maxtime = MAX(DATEADD(hh, -1, time_from)) FROM #tmp_agent_status
	SELECT @maxdate = CONVERT(varchar(10),@maxtime,112)
	SELECT @maxinterval = (DATEPART(hh, @maxtime)* 60) / @minutes_per_interval
	
	
	IF  @maxdate IS not NULL
	BEGIN
	
		UPDATE log_object_detail
		SET date_value = @maxdate,
	            int_value = @maxinterval 
		WHERE log_object_id = @log_object_id
		AND detail_id = 4
	END
END
ELSE
BEGIN
	SELECT @maxtime = MAX(DATEADD(hh, -1, time_from)) FROM t_agent_status_logg
	SELECT @maxdate = CONVERT(varchar(10),@maxtime,112)
	SELECT @maxinterval = (DATEPART(hh, @maxtime)* 60) / @minutes_per_interval

	IF  @maxdate IS not NULL
	BEGIN
		UPDATE log_object_detail
		SET date_value = @maxdate,
            		int_value = @maxinterval 
		WHERE log_object_id = @log_object_id
		AND detail_id = 4
	END    
END

IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION 
	RETURN
END


COMMIT TRANSACTION


DROP TABLE #tmp_agent_status


SET NOCOUNT OFF

GO

