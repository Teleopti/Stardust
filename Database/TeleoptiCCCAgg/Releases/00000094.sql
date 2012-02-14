/* 
BuildTime is: 
2009-04-22 
14:09
*/ 
/* 
Trunk initiated: 
2009-04-16 
11:49
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Devy Developer  
--Date: 2009-xx-xx  
--Desc: Because ...  
----------------  
 
GO 
 
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[agent_logg_view]'))
DROP VIEW [dbo].[agent_logg_view]

GO
CREATE VIEW [dbo].[agent_logg_view]
AS
SELECT     queue, date_from, interval, agent_id, agent_name, avail_dur, tot_work_dur, talking_call_dur, pause_dur, wait_dur, wrap_up_dur, answ_call_cnt, 
                      direct_out_call_cnt, direct_out_call_dur, direct_in_call_cnt, direct_in_call_dur, transfer_out_call_cnt, admin_dur
FROM         dbo.agent_logg_intraday WITH (NOLOCK)
UNION
SELECT     queue, date_from, interval, agent_id, agent_name, avail_dur, tot_work_dur, talking_call_dur, pause_dur, wait_dur, wrap_up_dur, answ_call_cnt, 
                      direct_out_call_cnt, direct_out_call_dur, direct_in_call_cnt, direct_in_call_dur, transfer_out_call_cnt, admin_dur
FROM         dbo.agent_logg WITH (NOLOCK)
GO
  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_update_stat_54]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_update_stat_54]
GO

CREATE PROCEDURE [dbo].[p_update_stat_54]
@log_object_id int,
@start_date smalldatetime = '1900-01-01',
@end_date smalldatetime = '1900-01-01',
@rel_start_date int =0,
@rel_start_int int=0
AS
declare @logdb_name varchar(50)
if exists (select 1 from log_object where log_object_id = log_object_id)
begin
	select @logdb_name = ltrim(rtrim(logdb_name))
	from log_object
	where log_object_id = @log_object_id
end
else
begin
	print 'ERROR: Log object with ID='+convert(varchar(5),@log_object_id)+' does not exist in system configuration.'
	return
end
if exists (select 1 from master.dbo.sysdatabases where name = @logdb_name)
begin

	exec p_insert_queue_logg_54 @log_object_id, @start_date, @end_date, @rel_start_date, @rel_start_int
	exec p_insert_agent_logg_54 @log_object_id, @start_date, @end_date, @rel_start_date, @rel_start_int
	exec p_insert_goal_results_54 @log_object_id, @start_date, @end_date, @rel_start_date, @rel_start_int


end
else
begin
	print 'ERROR: Log DB with name ='+@logdb_name+' does not exist on the server.'
end

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_update_stat]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_update_stat]
GO

CREATE PROCEDURE [dbo].[p_update_stat]
@log_object_id int,
@start_date smalldatetime = '1900-01-01',
@end_date smalldatetime = '1900-01-01',
@rel_start_date int =0,
@rel_start_int int=0
AS
declare @logdb_name varchar(50)
if exists (select 1 from log_object where log_object_id = log_object_id)
begin
	select @logdb_name = ltrim(rtrim(logdb_name))
	from log_object
	where log_object_id = @log_object_id
end
else
begin
	print 'ERROR: Log object with ID='+convert(varchar(5),@log_object_id)+' does not exist in system configuration.'
	return
end
if exists (select 1 from master.dbo.sysdatabases where name = @logdb_name)
begin

	exec p_insert_queue_logg @log_object_id, @start_date, @end_date, @rel_start_date, @rel_start_int
	exec p_insert_agent_logg @log_object_id, @start_date, @end_date, @rel_start_date, @rel_start_int
	exec p_insert_goal_results @log_object_id, @start_date, @end_date, @rel_start_date, @rel_start_int


end
else
begin
	print 'ERROR: Log DB with name ='+@logdb_name+' does not exist on the server.'
end

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_sts_update_agent_logg]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_sts_update_agent_logg]
GO

CREATE PROCEDURE [dbo].[p_sts_update_agent_logg]
@log_object_id int,
@last_logg_date smalldatetime,
@last_logg_interval int

AS

set nocount on 

begin transaction


--l�gg till -1 k� ifall den inte finns
insert into agent_logg(
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
select	qs.queue,	-- queue
	ast.date_from,	-- date_from
	ast.interval,	-- interval
	ast.agent_id,	-- agent_id
	af.agent_name,	-- agent_name
	0, 		-- avail_dur,		
	0, 		-- tot_work_dur,		
	0, 		-- talking_call_dur,	
	0, 		-- pause_dur,		
	0, 		-- wait_dur,		
	0, 		-- wrap_up_dur,		
	0, 		-- answ_call_cnt,
	0, 		-- direct_out_call_cnt,
	0, 		-- direct_out_call_dur,	
	0, 		-- direct_in_call_cnt,
	0, 		-- direct_in_call_dur,	
	0, 		-- transfer_out_call_cnt,
	0 		-- admin_dur)
from agent_state_logg as ast
inner join queues as qs on qs.orig_queue_id = -1
inner join agent_info as af on af.agent_id = ast.agent_id
where not exists(select agent_id from agent_logg where queue = qs.queue
						and date_from = ast.date_from
						and interval = ast.interval
						and agent_id = ast.agent_id
						and qs.log_object_id = @log_object_id)
group by qs.queue,ast.date_from,ast.interval,ast.agent_id,af.agent_name

if @@error <> 0
begin
	select 'ERROR: Rollback issued!!!'
	rollback transaction 
	return
end


--uppdatera poster p� k� -1 i agent_logg
update agent_logg set admin_dur = (select isnull(sum(ast.state_dur),0) from agent_state_logg as ast
					inner join agent_states as st on st.state_id = ast.state_id
					inner join queues as qs on qs.queue = al.queue
					where st.is_admin = 1 
					and st.is_active = 1
					and al.date_from = ast.date_from
					and qs.orig_queue_id = -1
					and al.agent_id = ast.agent_id
					and al.interval = ast.interval
					and qs.log_object_id = @log_object_id),
			avail_dur = (select isnull(ast.state_dur,0) from agent_state_logg as ast
					inner join agent_states as st on st.state_id = ast.state_id
					inner join queues as qs on qs.queue = al.queue
					where ast.state_id = (select state_id from agent_states where state_name = 'avail')
					and al.date_from = ast.date_from
					and qs.orig_queue_id = -1
					and al.agent_id = ast.agent_id
					and al.interval = ast.interval
					and qs.log_object_id = @log_object_id),
			pause_dur =  (select isnull(sum(ast.state_dur),0) from agent_state_logg as ast
					inner join agent_states as st on st.state_id = ast.state_id
					inner join queues as qs on qs.queue = al.queue
					where st.is_paus = 1 
					and st.is_active = 1
					and al.date_from = ast.date_from
					and qs.orig_queue_id = -1
					and al.agent_id = ast.agent_id
					and al.interval = ast.interval
					and qs.log_object_id = @log_object_id),
			wrap_up_dur =  (select isnull(sum(ast.state_dur),0) from agent_state_logg as ast
					inner join agent_states as st on st.state_id = ast.state_id
					inner join queues as qs on qs.queue = al.queue
					where st.is_wrap = 1 
					and st.is_active = 1
					and al.date_from = ast.date_from
					and qs.orig_queue_id = -1
					and al.agent_id = ast.agent_id
					and al.interval = ast.interval
					and qs.log_object_id = @log_object_id)
from agent_logg al
where al.date_from >= @last_logg_date
and al.interval >= (select case when al.date_from = @last_logg_date then @last_logg_interval else 0 end)
and al.queue = (select queue from queues where orig_queue_id = -1)


if @@error <> 0
begin
	select 'ERROR: Rollback issued!!!'
	rollback transaction 
	return
end


commit transaction

GO

  
GO  
 
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

  
GO  
 
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
	agent_name varchar(50) NULL,
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

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_SERGEL_insert_service_logg]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_SERGEL_insert_service_logg]
GO

CREATE      PROCEDURE [dbo].[p_SERGEL_insert_service_logg]

AS
/*
Special f�r Sergel. Textfiler fr�n Sergels �rendehanteringssystem l�ses in via schemalagd DTS.
Skapar upp k�er f�r respektive �rendetyp och l�gger sedan in raderna i agent_logg. /Zo� 2005-08-24
*/

SET NOCOUNT ON

DECLARE @max_date smalldatetime
DECLARE @min_date smalldatetime
DECLARE @min_per_interval int
DECLARE @int_per_day int

SELECT @int_per_day = system_setting_value from CCCv5_december..system_settings WHERE system_setting_id = 1
SELECT @min_per_interval = 1440/@int_per_day

CREATE TABLE #out	(queue int,
			date_from datetime,
			interval int,
			telia_id varchar(50),
			service varchar(50),
			answ_calls_cnt int,
			talking_call_dur int)

/*St�dar upp lite*/
UPDATE tcccagg6.toptiuser.service_file
SET start_time = LTRIM(RTRIM(start_time)), end_time = LTRIM(RTRIM(end_time)),  service = LTRIM(RTRIM(service))

UPDATE tcccagg6.toptiuser.service_file
SET start_time =
	CASE 
		WHEN LEN(start_time) < 8
		THEN '0' + start_time
	ELSE
		start_time
END

UPDATE tcccagg6.toptiuser.service_file
SET end_time =
	CASE 
		WHEN LEN(end_time) < 8
		THEN '0' + end_time
	ELSE
		end_time
END

--*******Uppdaterar efter optionalf�lten om det �r A-, B- eller C-markerat i filen
/*
--org
 UPDATE tcccagg6.toptiuser.service_file
 SET telia_id = ai.agent_id, sign_checked = 1
 FROM tcccagg6.toptiuser.service_file
 INNER JOIN tcccagg6..agent_info ai
 ON ai.agent_name = tcccagg6.toptiuser.service_file.telia_id
 INNER JOIN CCCv5_december..employee_period ep
 ON ep.login_id = ai.agent_id
 WHERE login_type = 'A'
 AND [date] BETWEEN ep.date_from AND ep.date_to
*/

UPDATE tcccagg6.toptiuser.service_file
--SET telia_id = tmc.local_user_id, sign_checked = 1
SET telia_id = ep.login_id, sign_checked = 1
FROM tcccagg6.toptiuser.service_file
INNER JOIN CCCv5_december..t_tmc_users tmc
ON tmc.user_name = telia_id
INNER JOIN  CCCv5_december..employees e
ON e.emp_id = tmc.local_user_id
INNER JOIN CCCv5_december..employee_period ep
ON e.emp_id = ep.emp_id
WHERE login_type = 'A'
AND [date] BETWEEN ep.date_from AND ep.date_to

UPDATE tcccagg6.toptiuser.service_file
SET telia_id = ep.login_id, sign_checked = 1
FROM tcccagg6.toptiuser.service_file
INNER JOIN cccv5_december..employee_optionals eo ON eo.optional_name ='racf'
INNER JOIN cccv5_december..employee_optional_values eov ON eov.optional_id=eo.optional_id
		AND eov.optional_text = telia_id
INNER JOIN cccv5_december..employee_period ep ON ep.emp_id = eov.emp_id 
		AND [date] BETWEEN ep.date_from AND ep.date_to
WHERE eo.optional_name = 'racf'
AND login_type = 'B'
AND ep.emp_id = eov.emp_id
AND [date] BETWEEN ep.date_from AND ep.date_to

UPDATE tcccagg6.toptiuser.service_file
SET telia_id = ep.login_id, sign_checked = 1
FROM tcccagg6.toptiuser.service_file
INNER JOIN cccv5_december..employee_optionals eo ON eo.optional_name ='ssign'
INNER JOIN cccv5_december..employee_optional_values eov ON eov.optional_id=eo.optional_id
		AND eov.optional_text = telia_id
INNER JOIN cccv5_december..employee_period ep ON ep.emp_id = eov.emp_id 
		AND [date] BETWEEN ep.date_from AND ep.date_to
WHERE eo.optional_name = 'ssign'
AND login_type = 'C'
AND ep.emp_id = eov.emp_id
AND [date] BETWEEN ep.date_from AND ep.date_to
--*******

--******L�gger till felaktiga signs till feltabell
INSERT INTO cccv5_december.dbo.missing_signs (exported_sign, login_type, errand_date)
SELECT telia_id, login_type, date FROM tcccagg6.toptiuser.service_file
WHERE sign_checked <> 1
--*******


/*Hittar r�tt intervall*/
UPDATE tcccagg6.toptiuser.service_file 
SET interval = ((CAST(SUBSTRING(start_time,1,2) AS int) * 60) + CAST(SUBSTRING(start_time,3,2) AS int))/@min_per_interval

/*Grupperar det hela p� datum, tj�nst och intervall. R�knar antalet och summerar tiden i sekunder*/
INSERT INTO #out (date_from, telia_id, service, interval, answ_calls_cnt, talking_call_dur)
SELECT [date], telia_id, service, interval, COUNT(*), SUM(CAST(seconds AS int)) FROM tcccagg6.toptiuser.service_file
GROUP BY [date], telia_id, service, interval

/*L�gger upp nya "k�er" om det beh�vs*/
INSERT INTO tcccagg6..queues (orig_desc, display_desc, log_object_id, orig_queue_id)
SELECT DISTINCT service, service, 2, -999 FROM #out
WHERE NOT EXISTS (SELECT 1 FROM tcccagg6..queues WHERE tcccagg6..queues.orig_desc COLLATE Latin1_General_CI_AS = #out.service COLLATE Latin1_General_CI_AS )

/*Hittar "k�id"*/
UPDATE #out
SET queue = tcccagg6..queues.queue
FROM #out, tcccagg6..queues
WHERE #out.service COLLATE Latin1_General_CI_AS = tcccagg6..queues.orig_desc COLLATE Latin1_General_CI_AS 

/*Stoppar in allt nytt i service_logg f�r denna v�nda*/
INSERT INTO tcccagg6.toptiuser.service_logg (queue, date_from, interval, telia_id, service, answ_calls_cnt, talking_call_dur)
SELECT queue, date_from, interval, telia_id, service, answ_calls_cnt, talking_call_dur FROM #out

SELECT @max_date = MAX(date_from), @min_date = MIN(date_from) FROM #out
TRUNCATE TABLE #out

/*H�mtar ut allt aggat fr�n service_logg*/
INSERT INTO #out (queue, date_from, interval, telia_id, service, answ_calls_cnt, talking_call_dur)
SELECT queue, date_from, interval, telia_id, service, SUM(answ_calls_cnt), SUM(talking_call_dur) FROM tcccagg6.toptiuser.service_logg
WHERE date_from BETWEEN @min_date AND @max_date
GROUP BY queue, date_from, interval, telia_id, service

/*Tar bort  ur agent_logg*/
DELETE tcccagg6..agent_logg
FROM tcccagg6..agent_logg al
INNER JOIN #out o
ON o.queue = al.queue
AND o.date_from = al.date_from

/*Stoppar in det igen*/
/*
INSERT INTO tcccagg6..agent_logg (queue, date_from, interval, agent_id, agent_name,  admin_dur, answ_call_cnt)
SELECT o.queue, o.date_from, o.interval, o.telia_id, ai.agent_name, SUM(o.talking_call_dur), SUM(o.answ_calls_cnt) FROM #out o
INNER JOIN tcccagg6..agent_info ai ON ai.agent_id = o.telia_id
ON ai.orig_agent_id = o.telia_id
ON ai.agent_name COLLATE Latin1_General_CI_AS  = o.telia_id COLLATE Latin1_General_CI_AS
WHERE ai.log_object_id = 3
GROUP BY queue, date_from, interval, o.telia_id, ai.agent_name--, avail_dur
*/

DELETE FROM #out
WHERE ISNUMERIC(telia_id) <> 1

BEGIN TRAN
	
	INSERT INTO tcccagg6..agent_logg (queue, date_from, interval, agent_id, agent_name,  admin_dur, answ_call_cnt)
	SELECT queue, date_from,  interval, telia_id, ai.agent_name, SUM(talking_call_dur), SUM(answ_calls_cnt) FROM #out o
	INNER JOIN tcccagg6..agent_info ai
	ON ai.agent_id = o.telia_id
	GROUP BY queue, date_from, interval, telia_id, agent_name

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
END

ELSE
BEGIN
	UPDATE tcccagg6..log_object_detail
	SET date_value = GETDATE()
	WHERE log_object_id = 2
	AND detail_id = 1
	COMMIT TRAN
END

DROP TABLE #out
DELETE FROM tcccagg6.toptiuser.service_file

--select * from tcccagg6..agent_logg where agent_id = 256
--select * from tcccagg6..agent_info
--select * from tcccagg6..queue_logg
--select * from tcccagg6..queues
--select * from tcccagg6..service_logg
--select * from service_file
--select * from #out

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_functions_split_string]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_functions_split_string]
GO

CREATE Procedure [dbo].[p_log_NET_functions_split_string]
-- Takes an input string with strings separated by commas and
-- inserts the result into a field called string in a given table 
-- with name @table_name
--
-- Created: 990322 by viktor.edlund@advisorconsulting.se
-- Last changed: 990513 by viktor.edlund@advisorconsulting.se
-- Last changed: 990819 by Micke
@string_string varchar(4000),
@table_name varchar(20)
As
 SET NOCOUNT ON
 DECLARE @pos int
 DECLARE @string varchar(50)
 DECLARE @insert_text varchar(100)
 -- Exit if an empty string is given 
 IF @string_string = '' BEGIN
  RETURN 0
 END 
 -- For simplicty concatenate , at the end of the string
 SELECT @string_string = @string_string + ','
 -- Ensure that @pos <> 0  
 SELECT @pos = CHARINDEX(',', @string_string )
 WHILE @pos <> 0 BEGIN
  -- Get the position of the first ,
  SELECT @pos = CHARINDEX(',', @string_string )
  
  -- Exit?
  IF @pos = 0 OR @pos = 1 OR @string_string = ','
   return 0
  -- Extract the substring
  SELECT @string = SUBSTRING(@string_string,1,@pos-1)
  -- Skip leading blanks
  SELECT @string = LTRIM(@string)
  -- Extract everything except the string
  SELECT @string_string = STUFF (@string_string,1,@pos,'')
  -- Insert the string into the temporary table
  SELECT @insert_text = 'INSERT INTO ' + @table_name + ' SELECT "' + @string + '"'
  EXEC (@insert_text)
 END

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_data_symposium_snapshot]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_data_symposium_snapshot]
GO

CREATE PROCEDURE [dbo].[p_log_NET_data_symposium_snapshot]

@main_id int,
@node_id int,
@cleartable int,
@agentrecord_array ntext

/*
@Agent_Name nvarchar(100),
@Agent_ID int,
@State int,
@Supervisor_ID int,
@Time_In_State int,
@Answering_Skillset int,
@DN_In_Time_In_State int,
@DN_Out_Time_In_State int,
@Position_ID int
*/

/*
Purpose: This procedure recieves data from the Symposium log
and inserts data in the t_log_NET_data_symposium_agent table.
By: 2006-12-19 Ma�g
Parameters: 	
		@main_id		       = site id
		@cleartable		       = 1 Existing records will be deleted before inserting new ones, 
		                                 0 Only supplied agents records will be inserted or updated
		@agentrecord_array	       = Commaseparated strings with Symposium specific info to 
                                                 update/insert. Note that the actual values have to be numeric.
*/

AS



-- Table variable to hold the info from the comma separated array
DECLARE @tbl_agentrecords TABLE (listpos int IDENTITY(1, 1) NOT NULL,
                              Agent_ID int,
                              State int,
                              Supervisor_ID int,
                              Time_In_State int,
                              Answering_Skillset int,
                              DN_In_Time_In_State int,
                              DN_Out_Time_In_State int,
                              Position_ID int)


-- "Unpack" the comma separated array into our table variable
INSERT @tbl_agentrecords
	SELECT a.int1, a.int2, a.int3, a.int4, a.int5, a.int6, a.int7, a.int8
	FROM f_log_NET_charlist_to_multicolumn_table_int(@agentrecord_array, 8, ',') a

-- Clear the whole table before inserting records ?
IF @cleartable = 1
BEGIN
    BEGIN TRAN symp_snapshot

    DELETE FROM t_log_NET_data_symposium_agent
    WHERE main_id=@main_id
	
    INSERT t_log_NET_data_symposium_agent
        SELECT @main_id, 
               a.Agent_ID,
	       a.State,
	       a.Supervisor_ID,
	       a.Time_In_State ,
	       a.Answering_Skillset ,
	       a.DN_In_Time_In_State ,
	       a.DN_Out_Time_In_State ,
	       a.Position_ID,
               1
        FROM @tbl_agentrecords a
	
    COMMIT TRAN symp_snapshot
END
ELSE
-- Just update the supplied agent's statuses (delete/insert or insert/update ?)
BEGIN
    BEGIN TRAN symp_snapshot

    -- Delete/insert all agents 
    DELETE FROM t_log_NET_data_symposium_agent
        WHERE main_id=@main_id AND
              agent_id IN (SELECT Agent_ID FROM @tbl_agentrecords)
	
    INSERT t_log_NET_data_symposium_agent
        SELECT @main_id, 
               a.Agent_ID,
	       a.State,
	       a.Supervisor_ID,
	       a.Time_In_State ,
	       a.Answering_Skillset ,
	       a.DN_In_Time_In_State ,
	       a.DN_Out_Time_In_State ,
	       a.Position_ID,
               1
        FROM @tbl_agentrecords a

    COMMIT TRAN symp_snapshot
END

-- Added 070320 by Ma�g to support Teleopti PRO alarm features
-- Removed due to multidb support, 070425
--EXEC	[dbo].[p_log_NET_update_lastupdated] @main_id, @node_id

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_data_symposium_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_data_symposium_agent]
GO

CREATE  PROCEDURE [dbo].[p_log_NET_data_symposium_agent]

@main_id int,
@node_id int,
@Agent_Name nvarchar(100),
@Agent_ID int,
@State int,
@Supervisor_ID int,
@Time_In_State int,
@Answering_Skillset int,
@DN_In_Time_In_State int,
@DN_Out_Time_In_State int,
@Position_ID int



 AS



DECLARE @now DATETIME
SELECT @now = CONVERT(smalldatetime, getdate())

IF NOT exists (SELECT 1 FROM t_log_NET_data_symposium_agent WHERE main_id = @main_id  AND agent_id = @Agent_ID)
BEGIN
	

	INSERT INTO t_log_NET_data_symposium_agent(main_id, agent_id, state, supervisor_id, time_in_state, answering_skillset, dn_in_time_in_state, dn_out_time_in_state, position_id)
	SELECT
	@main_id ,
	@Agent_ID,
	@State,
	@Supervisor_ID,
	@Time_In_State ,
	@Answering_Skillset ,
	@DN_In_Time_In_State ,
	@DN_Out_Time_In_State ,
	--@Supervisor_User_ID 
	@Position_ID
END
ELSE
BEGIN
	UPDATE t_log_NET_data_symposium_agent
	
	SET 	state=@State,
		supervisor_id=@Supervisor_ID,
		time_in_state=@Time_In_State,
 		answering_skillset=@Answering_Skillset,
 		dn_in_time_in_state=@DN_In_Time_In_State,
		dn_out_time_in_state=@DN_Out_Time_In_State,
		position_id=@Position_ID,
		updated=1
	WHERE main_id=@main_id
	AND agent_id=@Agent_ID

END

-- Added 070320 by Ma�g to support Teleopti PRO alarm features
-- Removed due to multidb support, 070425
--EXEC	[dbo].[p_log_NET_update_lastupdated] @main_id, @node_id

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_data_solidus_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_data_solidus_agent]
GO

CREATE   PROCEDURE [dbo].[p_log_NET_data_solidus_agent]
@main_id int,
@node_id int,
@event_string nvarchar(4000) = ''

/*
Syfte: Tar emot och splittar en str�ng fr�n Solidusloggen samt
l�ser in datat i t_log_solidus_agent_status
Av: Zo� 2005-03-16
Anv�nds av: 
Parametrar:	@event_string = Str�ngen fr�n loggen


exec p_log_solidus_insert_realtime 1, 'EVENTID=4;RECID=40;'

*/

AS

DECLARE @event_type_id int
DECLARE @agent_id int
DECLARE @voice int
DECLARE @email int
DECLARE @media int
DECLARE @loggedon int

IF @event_string = 'CONNECTED' /*New connection*/
BEGIN
	DELETE FROM t_log_NET_data_solidus_agent WHERE main_id = @main_id
	RETURN
END

IF (LEN(@event_string)>12)
BEGIN
	
	SELECT @event_string = left(REPLACE(@event_string,';',','),len(@event_string))
	
	CREATE TABLE #tmpParams (params nvarchar(200))
	EXEC p_log_NET_functions_split_string @event_string, #tmpParams

	CREATE TABLE #params(param_name nvarchar(50),value nvarchar(200))
	
	INSERT INTO #params
	SELECT LEFT(params,CHARINDEX('=',params)-1),RIGHT(params,LEN(params)-CHARINDEX('=',params)) 
	FROM #tmpParams
	
	SELECT @event_type_id = CONVERT(int,value) FROM #params WHERE param_name = 'EVENTID'
	SELECT @agent_id = CONVERT(int,value) FROM #params WHERE param_name = 'RECID'
	
	/* Set startvalues for the parameters */
	SELECT @voice = -1, @email = -1, @media = -1, @loggedon = -1
	
	IF @event_type_id = 1 /*BROADCAST_LOGON*/
	BEGIN
		SELECT @loggedon = 1
		SELECT @voice = CONVERT(int,value) FROM #params WHERE param_name = 'VoiceReady'
		SELECT @email = CONVERT(int,value) FROM #params WHERE param_name = 'EmailReady'
		SELECT @media = CONVERT(int,value) FROM #params WHERE param_name = 'MediaReady'
	END

	IF @event_type_id = 2 /*BROADCAST_LOGOFF*/
	BEGIN
		SELECT @loggedon = 0
		SELECT @voice = 0
		SELECT @email = 0
		SELECT @media = 0
	END
	
	IF @event_type_id = 3 /*BROADCAST_READY*/
	BEGIN
		SELECT @voice = 1
	END

	IF @event_type_id = 4 /*BROADCAST_NOTREADY*/
	BEGIN
		SELECT @voice = 0
	END	
	
	IF @event_type_id = 5 /*BROADCAST_EMAILREADY*/
	BEGIN
		SELECT @email = 1
	END
	
	IF @event_type_id = 6 /*BROADCAST_EMAILNOTREADY*/
	BEGIN
		SELECT @email = 0
	END
	
	IF @event_type_id = 7 /*BROADCAST_ORIGINATED*/
	BEGIN
		SELECT @voice = 2
	END
	
	IF @event_type_id = 8 /*BROADCAST_DELIVERED*/
	BEGIN
		SELECT @voice = 2
	END	
	
	IF @event_type_id = 9 /*BROADCAST_ESTABLISHED*/
	BEGIN
		SELECT @voice = 2
	END	
	
	IF @event_type_id = 10 /*BROADCAST_HELD*/
	BEGIN
		SELECT @voice = 2
	END	
	
	IF @event_type_id = 11 /*BROADCAST_RETRIEVED*/
	BEGIN
		SELECT @voice = 2
	END
		
	IF @event_type_id = 12 /*BROADCAST_TRANSFERED*/
	BEGIN
		SELECT @voice = 2
	END

	IF @event_type_id = 13 /*BROADCAST_CONFERENCED*/
	BEGIN
		SELECT @voice = 2
	END

	IF @event_type_id = 14 /*BROADCAST_CONNECTIONCLEARED*/
	BEGIN
		SELECT @voice = 1
		IF (SELECT value FROM #params WHERE param_name = 'ClericalFlag' )= '1' /*Busy or Clerical*/
			SELECT @voice = 2

	END

	IF @event_type_id = 15 /*BROADCAST_CALENDED*/
	BEGIN
		DECLARE @tmp_event_id INT
		SELECT @tmp_event_id = event_id FROM t_log_NET_data_solidus_agent 
		WHERE agent_id = @agent_id AND main_id = @main_id
		
		IF @tmp_event_id <> 4
			SELECT @voice = 1
	
	END
	
	IF @event_type_id = 16 /*BROADCAST_CALLREJECTED*/
	BEGIN
		SELECT @voice = 2
		IF (SELECT value FROM #params WHERE param_name = 'CAUSE' )= 'Agent Logged Off'
		BEGIN
			SELECT @loggedon = 0
			SELECT @voice = 0
			SELECT @email = 0
			SELECT @media = 0
		END
	END
	
	IF @event_type_id = 17 /*BROADCAST_CALLBACKACCEPTED*/
	BEGIN
		SELECT @voice = 2
		IF (SELECT value FROM #params WHERE param_name = 'CAUSE' )= 'Agent Logged Off'
		BEGIN
			SELECT @loggedon = 0
			SELECT @voice = 0
			SELECT @email = 0
			SELECT @media = 0
		END
	END

	IF @event_type_id = 18 /*BROADCAST_CALLBACKREJECTED*/
	BEGIN
		SELECT @voice = 2
		IF (SELECT value FROM #params WHERE param_name = 'CAUSE' )= 'Agent Logged Off'
		BEGIN
			SELECT @loggedon = 0
			SELECT @voice = 0
			SELECT @email = 0
			SELECT @media = 0
		END
	END
	
	IF @event_type_id = 19 /*BROADCAST_CALLBACKSTATUS*/
	BEGIN
		SELECT @voice = -1 /*Do nothiing*/
	END

	IF @event_type_id = 20 /*BROADCAST_EMAILREJECT*/
	BEGIN
		SELECT @email = 2
		IF (SELECT value FROM #params WHERE param_name = 'CAUSE' )= 'Agent Logged Off'
		BEGIN
			SELECT @loggedon = 0
			SELECT @voice = 0
			SELECT @email = 0
			SELECT @media = 0
		END
	END
	
	IF @event_type_id = 21 /*BROADCAST_EMAILDELETE*/
	BEGIN
		SELECT @voice = -1 /*Do nothiing*/
	END

	IF @event_type_id = 22 /*BROADCAST_EMAILREPLY*/
	BEGIN
		SELECT @email = 2
	END

	IF @event_type_id = 23 /*BROADCAST_EMAILINFO*/
	BEGIN
		SELECT @email = 2
	END

	IF @event_type_id = 24 /*BROADCAST_CALLINFORMATION*/
	BEGIN
		SELECT @voice = 2
		IF (SELECT value FROM #params WHERE param_name = 'CALLTYPE' )= '4'
			SELECT @voice = 2
		IF (SELECT value FROM #params WHERE param_name = 'CALLTYPE' )= '5'
			SELECT @email = 2
	END

	IF @event_type_id = 25 /*BROADCAST_EMAILREJECT*/
	BEGIN
		SELECT @voice = 2
	END

	IF @event_type_id = 26 /*BROADCAST_MEDIASTARTED*/
	BEGIN
		SELECT @media = 2
	END

	IF @event_type_id = 27 /*BROADCAST_MEDIASTOPPED*/
	BEGIN
		SELECT @media = 1
	END

	IF @event_type_id = 28 /*BROADCAST_CSRSTATUS*/
	BEGIN
		IF (SELECT value FROM #params WHERE param_name = 'STATUS' )= 'Ready'
			SELECT @media = 1
		IF (SELECT value FROM #params WHERE param_name = 'STATUS' )= 'Not Ready'
			SELECT @media = 0
		IF (SELECT value FROM #params WHERE param_name = 'STATUS' )= 'Logged Off'
		BEGIN
			SELECT @loggedon = 0
			SELECT @voice = 0
			SELECT @email = 0
			SELECT @media = 0
		END
	END

	IF @event_type_id = 29 /*BROADCAST_CAMPAIGNACCEPT*/
	BEGIN
		SELECT @voice = 2
	END

	IF @event_type_id = 30 /*BROADCAST_CAMPAIGREJECTED*/
	BEGIN
		SELECT @voice = 2
		IF (SELECT value FROM #params WHERE param_name = 'CAUSE' )= 'Agent Logged Off'
		BEGIN
			SELECT @loggedon = 0
			SELECT @voice = 0
			SELECT @email = 0
			SELECT @media = 0
		END
	END
	
	IF @event_type_id = 31 /*BROADCAST_CAMPAIGNSTATUS*/
	BEGIN
		SELECT @voice = 1
	END

	IF @event_type_id = 32 /*BROADCAST_CAMPAIGNINFO*/
	BEGIN
		SELECT @voice = 2
	END

	IF @event_type_id = 33 /*BROADCAST_SERVICEGROUP_ADDED*/
	BEGIN
		SELECT @voice = -1 /* Do nothing*/
	END

	IF @event_type_id = 34 /*BROADCAST_AGENT_SERVICEGROUPS_CHANGED*/
	BEGIN
		SELECT @voice = -1 /* Do nothing*/
	END

	IF @event_type_id = 35 /*BROADCAST_DATA_COMPLETE*/
	BEGIN
		SELECT @voice = -1 /* Do nothing*/
	END

	IF @event_type_id = 36 /*BROADCAST_DISCONECT*/
	BEGIN
		SELECT @voice = -1 /* Do nothing OR should we delete everything?*/
	END

	IF @event_type_id = 37 /*BROADCAST_XFERC_ALLINFORMATION*/
	BEGIN
		SELECT @voice = 2
	END

	
	IF NOT EXISTS (SELECT 1 FROM t_log_NET_data_solidus_agent WHERE main_id = @main_id AND agent_id = @agent_id)
	BEGIN
		SELECT @loggedon = 1 /*If an event on agent the agent must be loggedon*/

		IF @voice = -1
			SELECT @voice = 0	
		IF @email = -1
			SELECT @email = 0
		IF @media = -1
			SELECT @media = 0	
		
		INSERT INTO t_log_NET_data_solidus_agent(main_id, agent_id, loggedon, voice, email, media, updated, event_id)
		SELECT @main_id, @agent_id, @loggedon, @voice, @email, @media, 1, @event_type_id
	END
	ELSE
	BEGIN
		IF @loggedon <> -1
		BEGIN
			UPDATE t_log_NET_data_solidus_agent SET loggedon = @loggedon, updated = 1, event_id = @event_type_id
			WHERE agent_id = @agent_id 
			AND main_id = @main_id
		END		

		IF @voice <> -1
		BEGIN
			UPDATE t_log_NET_data_solidus_agent SET voice = @voice, updated = 1, event_id = @event_type_id
			WHERE agent_id = @agent_id 
			AND main_id = @main_id
		END

		IF @email <> -1
		BEGIN
			UPDATE t_log_NET_data_solidus_agent SET email = @email, updated = 1, event_id = @event_type_id
			WHERE agent_id = @agent_id 
			AND main_id = @main_id
		END

		IF @media <> -1
		BEGIN
			UPDATE t_log_NET_data_solidus_agent SET media = @media, updated = 1, event_id = @event_type_id 
			WHERE agent_id = @agent_id 
			AND main_id = @main_id
		END
		
		DECLARE @curvoice int, @curemail int, @curmedia int
		SELECT @curvoice = voice FROM t_log_NET_data_solidus_agent WHERE main_id = @main_id AND agent_id = @agent_id
		SELECT @curemail = email FROM t_log_NET_data_solidus_agent WHERE main_id = @main_id AND agent_id = @agent_id
		SELECT @curmedia = media FROM t_log_NET_data_solidus_agent WHERE main_id = @main_id AND agent_id = @agent_id

		IF (@curvoice <> 0 OR @curemail <> 0 OR @curmedia <> 0)
		BEGIN
			UPDATE t_log_NET_data_solidus_agent
			SET loggedon = 1
			WHERE main_id = @main_id
			AND agent_id = @agent_id
		END
	END

	/* Write data to logg table, just for testing. Delete old data */
	IF (SELECT datediff(day,MIN(time),getdate()) FROM t_log_NET_data_solidus_logg) > 0
	BEGIN
		TRUNCATE TABLE t_log_NET_data_solidus_logg
	END
	
	INSERT INTO t_log_NET_data_solidus_logg(main_id, agent_id, event_type_id, parameter, value)
	SELECT @main_id, @agent_id,@event_type_id,param_name, value
	FROM #params

END

-- Added 070320 by Ma�g to support Teleopti PRO alarm features
-- Removed due to multidb support, 070425
--EXEC	[dbo].[p_log_NET_update_lastupdated] @main_id, @node_id

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_data_genesys_ctia_snapshot]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_data_genesys_ctia_snapshot]
GO

CREATE PROCEDURE [dbo].[p_log_NET_data_genesys_ctia_snapshot]

@main_id int,
@node_id int,
@cleartable int,
@agent_id_array ntext,
@agent_status_array ntext,
@agent_id_remove_array ntext

/*
Purpose: This procedure recieves data from the Genesys CTIA bridge log
and inserts data in the t_log_NET_data_genesys_ctia_agent table.
The supplied parameters can either be single string values or commaseparated string values.
By: 2006-12-13 Ma�g
Parameters: 	
		@main_id		       = site id
		@cleartable		       = 1 Existing records will be deleted before inserting new ones, 
		                         0 Only supplied agents records will be inserted or updated
		@agent_id_array	       = Commaseparated strings with Genesys unique agent id's to update/insert
		@agent_status_array	   = Commaseparated strings with Genesys agent statuses (corresponding 
		                         to the supplied agent id's)
		@agent_id_remove_array = Commaseparated strings with Genesys unique agent id's that should
		                         be removed from table (i.e. logged out)
*/
AS

-- Table variable to hold the info from the comma separated arrays
DECLARE @tbl_agentrecords TABLE (listpos     int IDENTITY(1, 1) NOT NULL,
                                 agentid     nvarchar(1024),
                                 agentstatus nvarchar(1024))

-- Table variable to hold the info from the comma separated arrays
DECLARE @tbl_agentstoremove TABLE (listpos int IDENTITY(1, 1) NOT NULL,
                                   agentid nvarchar(1024))

-- "Unpack" the two comma separated arrays into our table variable
INSERT @tbl_agentrecords
	SELECT a.nstr, s.nstr
	FROM f_log_NET_charlist_to_table(@agent_id_array, DEFAULT) a
	JOIN f_log_NET_charlist_to_table(@agent_status_array, DEFAULT) s ON a.listpos = s.listpos
	WHERE a.nstr <> ''

-- "Unpack" the comma separated array with agent id's to remove into the table variable
INSERT @tbl_agentstoremove
	SELECT a.nstr
	FROM f_log_NET_charlist_to_table(@agent_id_remove_array, DEFAULT) a
	WHERE a.nstr <> ''

-- Clear the whole table before inserting records ?
IF @cleartable = 1
BEGIN
	BEGIN TRAN genesys_ctia_snapshot
	
	DELETE FROM t_log_NET_data_genesys_ctia_agent
	WHERE main_id=@main_id
	
	INSERT t_log_NET_data_genesys_ctia_agent
	SELECT @main_id, a.agentid, a.agentstatus, 1
	FROM @tbl_agentrecords a
	
	COMMIT TRAN genesys_ctia_snapshot
END
ELSE
-- Just update the supplied agent's statuses (delete/insert or insert/update ?)
BEGIN
	BEGIN TRAN genesys_ctia_snapshot

	-- Remove all loggedout agents
	DELETE FROM t_log_NET_data_genesys_ctia_agent
	WHERE main_id=@main_id AND
	agent_id IN (SELECT convert(int, agentid) FROM @tbl_agentstoremove)
	
	-- Delete/insert all agents 
	DELETE FROM t_log_NET_data_genesys_ctia_agent
	WHERE main_id=@main_id AND
	agent_id IN (SELECT convert(int, agentid) FROM @tbl_agentrecords)
	
	INSERT t_log_NET_data_genesys_ctia_agent
	SELECT @main_id, a.agentid, a.agentstatus, 1
	FROM @tbl_agentrecords a

	COMMIT TRAN genesys_ctia_snapshot
END

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_data_genesys_ctia_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_data_genesys_ctia_agent]
GO

CREATE  PROCEDURE [dbo].[p_log_NET_data_genesys_ctia_agent]

@main_id int,
@node_id int,
@agent_id nvarchar(200),
@agent_status  nvarchar(200)

/*
Purpose: This procedure recieves data from the Genesys CTIA bridge log
and inserts data in the t_log_NET_data_genesys_ctia_agent table
By: 2006-12-12 Ma�g
Parameters: 	
		@main_id		= site id
		@agent_id		= Genesys unique id for the agent 
		@agent_status	= Agent status		
*/
AS

IF NOT exists (SELECT 1 FROM t_log_NET_data_genesys_ctia_agent WHERE main_id = @main_id  AND agent_id = @agent_id)
BEGIN
	INSERT INTO t_log_NET_data_genesys_ctia_agent(main_id, agent_id, agent_status, updated)
	VALUES (@main_id, @agent_id, @agent_status, 1)
END
ELSE
BEGIN
	UPDATE t_log_NET_data_genesys_ctia_agent
	
	SET agent_status=@agent_status,
		updated=1
	WHERE main_id=@main_id
	AND agent_id=@agent_id

END

-- Added 070320 by Ma�g to support Teleopti PRO alarm features
-- Removed due to multidb support, 070425
--EXEC	[dbo].[p_log_NET_update_lastupdated] @main_id, @node_id

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_data_callguide_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_data_callguide_agent]
GO

CREATE PROCEDURE [dbo].[p_log_NET_data_callguide_agent]
@main_id int,
@node_id int,
@agent_id int,
@agent_name nvarchar(200),
@event nvarchar(200),
@agent_status nvarchar(200),
@work_level nvarchar(200)

/*
Purpose: This procedure recieves data from the callguide log 
and inserts data in the callguide table
By: 2005-09-06 Peder
Parameters: 	@main_id = site id
		@node_id = TeleoptiLog node id
		@agent_id = Callguide's unique id for the agent 
		@agent_name = Name of the agent, from Callguide
		@event = The name of the event that fired
		@agent_status = Name of the status
		@work_level = The name of the worklevel
*/

AS
SET NOCOUNT ON

--DECLARE @workmode_code int
DECLARE @workmode nvarchar(100)

if @event = 'AgentLogin'
BEGIN
	SELECT @agent_status = 'paused'
	SELECT @work_level = 'private'
END

if @event = 'AgentLogout'
BEGIN
	SELECT @agent_status = 'logout'
	SELECT @work_level = ''

	--SELECT @workmode_code = 120
END

SELECT @workmode = @agent_status + ' ' + @work_level
SELECT @workmode = ltrim(rtrim(@workmode))

--SELECT @workmode_code = workmode_code FROM t_rta_states WHERE state_name = @workmode

IF NOT EXISTS (SELECT 1 FROM t_log_NET_data_callguide_agent WHERE main_id = @main_id AND agent_id = @agent_id)
BEGIN
	INSERT INTO t_log_NET_data_callguide_agent(main_id, node_id, agent_id, agent_name, event, agent_status, work_level, updated, workmode_code)
	VALUES (@main_id, @node_id, @agent_id, @agent_name, @event, @agent_status, @work_level, 1, @workmode)
END
ELSE
BEGIN
	UPDATE t_log_NET_data_callguide_agent
		SET agent_name = @agent_name
		, event = @event
		, agent_status = @agent_status
		, work_level = @work_level
		, updated = 1
		, workmode_code = @workmode
	WHERE main_id = @main_id
	AND node_id = @node_id
	AND agent_id = @agent_id
END

-- Added 070320 by Ma�g to support Teleopti PRO alarm features
-- Removed due to multidb support, 070425
--EXEC	[dbo].[p_log_NET_update_lastupdated] @main_id, @node_id

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_data_avaya_agent_init]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_data_avaya_agent_init]
GO

CREATE PROCEDURE [dbo].[p_log_NET_data_avaya_agent_init]
@main_id int,
@node_id int

AS

BEGIN TRAN avaya_agent_init_tran
DELETE FROM t_log_NET_data_avaya_agent
WHERE main_id=@main_id AND node_id = @node_id
COMMIT TRAN avaya_agent_init_tran

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_data_avaya_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_data_avaya_agent]
GO

CREATE PROCEDURE [dbo].[p_log_NET_data_avaya_agent] 

@main_id int,
@node_id int,
@TS int,
@EXTENSION	int,		-- EXTENSION
@WORKMODE_DIRECTION int,	-- WORKMODE DIRECTION(AGSTATE)
@AGDURATION int,		-- AGDURATION
@AUXREASON	 int,		-- AUXREASON
@DA_INQUEUE int,		-- DA_INQUEUE
@WORKSKILL	int,		-- WORKSKILL
@ACDONHOLD	 int,		-- ACDONHOLD
@ACD	int,			-- ACD
@LOGID int			--LOGID


AS

IF NOT exists (SELECT 1 FROM t_log_NET_data_avaya_agent WHERE main_id = @main_id  AND logid = @LOGID)
BEGIN
	INSERT INTO t_log_NET_data_avaya_agent(main_id, node_id, time_stamp, extension, workmode_direction, ag_duration, auxreason, da_inqueue, workskill, acdonhold, acd, logid)
	SELECT
	@main_id ,
	@node_id,
	@TS ,
	@EXTENSION	,		
	@WORKMODE_DIRECTION ,	
	@AGDURATION ,		
	@AUXREASON	 ,		
	@DA_INQUEUE ,		
	@WORKSKILL	,		
	@ACDONHOLD	 ,		
	@ACD	,			
	@LOGID 			
END
ELSE
BEGIN
	UPDATE t_log_NET_data_avaya_agent
	
	SET time_stamp=@TS, 
		workmode_direction=@WORKMODE_DIRECTION, 
		ag_duration=@AGDURATION, 
		auxreason=@AUXREASON, 
		da_inqueue=@DA_INQUEUE, 
		workskill=@WORKSKILL, 
		acdonhold=@ACDONHOLD, 
		acd=@ACD,
		extension=@EXTENSION,
		updated=1
	WHERE main_id=@main_id
	AND logid=@LOGID
	AND node_id = @node_id
	
END

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_insert_queue_logg_54]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_insert_queue_logg_54]
GO

/*****************************************/
/* Created by Anders F 20020605  */
/*****************************************/
CREATE PROCEDURE [dbo].[p_insert_queue_logg_54]
/*
Micke D 2004-02-27 Added 10 minutes intervals on 30 minutes logging
Micke D 2004-08-19 Added 10 minute intervals on 60 minute logging
Micke D 2004-08-19 Added 15 minute intervals on 60 minute logging
*/
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
@default_short_call_treshold int,
@default_service_level_sec int,
@int_per_day int
SET NOCOUNT ON

/*
@start_date anv�nds om man vill agga fr�n specifikt datum
@end_date anv�nds om man vill agga till ett specifikt datum
@rel_start_date anv�nds om man t ex vill agg bak�t en dag hela tiden. Skickar d� in -1
@rel_start_int anv�nds om man t ex vill agg bak�t fyra intervall hela tiden. Skickar d� in -4
*/
/********************************************************/
/* Fetch latest log date and interval                   */
/********************************************************/
SELECT @last_logg_date = date_value , @last_logg_interval = int_value
FROM log_object_detail 
WHERE log_object_id = @log_object_id
AND detail_id = 1

SELECT @stop_date = dateadd(month,1,@last_logg_date)
SELECT @int_per_day= int_value
FROM ccc_system_info 
WHERE [id]=1


/* 

H�r blir det lite nytt  

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
			/* H�r ligger intervallet fortfarande inom dagen s� det �r bara att plussa */
			SELECT @last_logg_interval = @last_logg_interval + @rel_start_int
		END
		ELSE IF  (@rel_start_int + @last_logg_interval >= @int_per_day)
		BEGIN
			/* H�r har intervallet g�tt �ver till n�sta dag vi �kar p� dagen en dag och �ndrar intervallet */
			SELECT @last_logg_date = dateadd ( day,1,@last_logg_date)
			SELECT @last_logg_interval = (@last_logg_interval + @rel_start_int) - (@int_per_day - 1)
		END 
	END 
	ELSE
	BEGIN
		/* H�r �r intervallet ig�r */
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
	H�r blir det nytt
	
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
END


IF  @mindate IS not NULL
BEGIN

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
	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval,
		round(convert(real,offd_direct_call_cnt)/4.0,0),
		round(convert(real,overflow_in_call_cnt)/4.0,0),
		round(convert(real,aband_call_cnt)/4.0,0),
		round(convert(real,overflow_out_call_cnt)/4.0,0),
		round(convert(real,answ_call_cnt)/4.0,0),
		round(convert(real,queued_and_answ_call_dur)/4.0,0),
		round(convert(real,queued_and_aband_call_dur)/4.0,0),
		round(convert(real,talking_call_dur)/4.0,0),
		round(convert(real,wrap_up_dur)/4.0,0),
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		round(convert(real,ans_servicelevel_cnt)/4.0,0),
		round(convert(real,wait_dur)/4.0,0),
		round(convert(real,aband_short_call_cnt)/4.0,0),
		round(convert(real,aband_within_sl_cnt)/4.0,0)
				
		FROM #tmp_queue_logg

	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval + 1,
		round(convert(real,offd_direct_call_cnt)/4.0,0),
		round(convert(real,overflow_in_call_cnt)/4.0,0),
		round(convert(real,aband_call_cnt)/4.0,0),
		round(convert(real,overflow_out_call_cnt)/4.0,0),
		round(convert(real,answ_call_cnt)/4.0,0),
		round(convert(real,queued_and_answ_call_dur)/4.0,0),
		round(convert(real,queued_and_aband_call_dur)/4.0,0),
		round(convert(real,talking_call_dur)/4.0,0),
		round(convert(real,wrap_up_dur)/4.0,0),
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		round(convert(real,ans_servicelevel_cnt)/4.0,0),
		round(convert(real,wait_dur)/4.0,0),
		round(convert(real,aband_short_call_cnt)/4.0,0),
		round(convert(real,aband_within_sl_cnt)/4.0,0)
				
		FROM #tmp_queue_logg

	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval + 2,
		ceiling(convert(real,offd_direct_call_cnt)/4.0),
		ceiling(convert(real,overflow_in_call_cnt)/4.0),
		ceiling(convert(real,aband_call_cnt)/4.0),
		ceiling(convert(real,overflow_out_call_cnt)/4.0),
		ceiling(convert(real,answ_call_cnt)/4.0),
		ceiling(convert(real,queued_and_answ_call_dur)/4.0),
		ceiling(convert(real,queued_and_aband_call_dur)/4.0),
		ceiling(convert(real,talking_call_dur)/4.0),
		ceiling(convert(real,wrap_up_dur)/4.0),
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		ceiling(convert(real,ans_servicelevel_cnt)/4.0),
		ceiling(convert(real,wait_dur)/4.0),
		ceiling(convert(real,aband_short_call_cnt)/4.0),
		ceiling(convert(real,aband_within_sl_cnt)/4.0)

		FROM #tmp_queue_logg

	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval + 3,
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
		ceiling(convert(real,answ_call_cnt)/2.0),
		ceiling(convert(real,queued_and_answ_call_dur)/2.0),
		ceiling(convert(real,queued_and_aband_call_dur)/2.0),
		ceiling(convert(real,talking_call_dur)/2.0),
		ceiling(convert(real,wrap_up_dur)/2.0),
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
		answ_call_cnt/2,
		queued_and_answ_call_dur/2,
		queued_and_aband_call_dur/2,
		talking_call_dur/2,
		wrap_up_dur/2,
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
		AND detail_id = 3
	
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

IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION
	RETURN
END

/******** NEW 5.2 Business Scorecard ********/
DECLARE @text varchar(500)
SELECT @text = 'p_insert_queue_by_day_logg '+convert(varchar(10),@log_object_id)+
	', '+'
	'+''''+convert(varchar(10),@last_logg_date,120)+''''
--EXECUTE (@text)
/******** NEW 5.2 Business Scorecard ********/
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

  
GO  
 
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
*/
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
@default_short_call_treshold int,
@default_service_level_sec int,
@int_per_day int
SET NOCOUNT ON

/*
@start_date anv�nds om man vill agga fr�n specifikt datum
@end_date anv�nds om man vill agga till ett specifikt datum
@rel_start_date anv�nds om man t ex vill agg bak�t en dag hela tiden. Skickar d� in -1
@rel_start_int anv�nds om man t ex vill agg bak�t fyra intervall hela tiden. Skickar d� in -4
*/
/********************************************************/
/* Fetch latest log date and interval                   */
/********************************************************/
SELECT @last_logg_date = date_value , @last_logg_interval = int_value
FROM log_object_detail 
WHERE log_object_id = @log_object_id
AND detail_id = 1

SELECT @stop_date = dateadd(month,1,@last_logg_date)
SELECT @int_per_day= int_value
FROM ccc_system_info 
WHERE [id]=1


/* 

H�r blir det lite nytt  

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
			/* H�r ligger intervallet fortfarande inom dagen s� det �r bara att plussa */
			SELECT @last_logg_interval = @last_logg_interval + @rel_start_int
		END
		ELSE IF  (@rel_start_int + @last_logg_interval >= @int_per_day)
		BEGIN
			/* H�r har intervallet g�tt �ver till n�sta dag vi �kar p� dagen en dag och �ndrar intervallet */
			SELECT @last_logg_date = dateadd ( day,1,@last_logg_date)
			SELECT @last_logg_interval = (@last_logg_interval + @rel_start_int) - (@int_per_day - 1)
		END 
	END 
	ELSE
	BEGIN
		/* H�r �r intervallet ig�r */
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
	H�r blir det nytt
	
*/

DECLARE @mindate smalldatetime, @mininterval int
DECLARE @maxdate smalldatetime, @maxinterval int

SELECT @mindate=MIN(date_from) from #tmp_queue_logg
SELECT @mininterval=MIN(interval) FROM #tmp_queue_logg
WHERE date_from = @mindate

SELECT @maxdate=MAX(date_from) from #tmp_queue_logg
SELECT @maxinterval=MAX(interval) FROM #tmp_queue_logg
WHERE date_from = @maxdate

/*Hantera konverteringar mellan olika intervall*/
SELECT @maxinterval = @maxinterval + (@interval_per_hour / @CTI_interval_per_hour ) - 1


IF @maxdate > @stop_date
BEGIN
	SELECT @maxdate = @stop_date
END


IF  @mindate IS not NULL
BEGIN

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
	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval,
		round(convert(real,offd_direct_call_cnt)/4.0,0),
		round(convert(real,overflow_in_call_cnt)/4.0,0),
		round(convert(real,aband_call_cnt)/4.0,0),
		round(convert(real,overflow_out_call_cnt)/4.0,0),
		round(convert(real,answ_call_cnt)/4.0,0),
		round(convert(real,queued_and_answ_call_dur)/4.0,0),
		round(convert(real,queued_and_aband_call_dur)/4.0,0),
		round(convert(real,talking_call_dur)/4.0,0),
		round(convert(real,wrap_up_dur)/4.0,0),
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		round(convert(real,ans_servicelevel_cnt)/4.0,0),
		round(convert(real,wait_dur)/4.0,0),
		round(convert(real,aband_short_call_cnt)/4.0,0),
		round(convert(real,aband_within_sl_cnt)/4.0,0)
				
		FROM #tmp_queue_logg

	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval + 1,
		round(convert(real,offd_direct_call_cnt)/4.0,0),
		round(convert(real,overflow_in_call_cnt)/4.0,0),
		round(convert(real,aband_call_cnt)/4.0,0),
		round(convert(real,overflow_out_call_cnt)/4.0,0),
		round(convert(real,answ_call_cnt)/4.0,0),
		round(convert(real,queued_and_answ_call_dur)/4.0,0),
		round(convert(real,queued_and_aband_call_dur)/4.0,0),
		round(convert(real,talking_call_dur)/4.0,0),
		round(convert(real,wrap_up_dur)/4.0,0),
		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		round(convert(real,ans_servicelevel_cnt)/4.0,0),
		round(convert(real,wait_dur)/4.0,0),
		round(convert(real,aband_short_call_cnt)/4.0,0),
		round(convert(real,aband_within_sl_cnt)/4.0,0)
				
		FROM #tmp_queue_logg

	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval + 2,
		ceiling(convert(real,offd_direct_call_cnt)/4.0),
		ceiling(convert(real,overflow_in_call_cnt)/4.0),
		ceiling(convert(real,aband_call_cnt)/4.0),
		ceiling(convert(real,overflow_out_call_cnt)/4.0),
		ceiling(convert(real,answ_call_cnt)/4.0),
		ceiling(convert(real,queued_and_answ_call_dur)/4.0),
		ceiling(convert(real,queued_and_aband_call_dur)/4.0),
		ceiling(convert(real,talking_call_dur)/4.0),
		ceiling(convert(real,wrap_up_dur)/4.0),

		queued_answ_longest_que_dur,
		queued_aband_longest_que_dur,
		avg_avail_member_cnt,
		ceiling(convert(real,ans_servicelevel_cnt)/4.0),
		ceiling(convert(real,wait_dur)/4.0),
		ceiling(convert(real,aband_short_call_cnt)/4.0),
		ceiling(convert(real,aband_within_sl_cnt)/4.0)


		FROM #tmp_queue_logg

	INSERT INTO queue_logg 
	SELECT
		queue,
		date_from,
		interval + 3,

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

IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION
	RETURN
END

/******** NEW 5.2 Business Scorecard ********/
DECLARE @text varchar(500)
SELECT @text = 'p_insert_queue_by_day_logg '+convert(varchar(10),@log_object_id)+
	', '+'
	'+''''+convert(varchar(10),@last_logg_date,120)+''''

EXECUTE (@text)
/******** NEW 5.2 Business Scorecard ********/
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

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_insert_queue_by_day_logg]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_insert_queue_by_day_logg]
GO

CREATE PROCEDURE [dbo].[p_insert_queue_by_day_logg]
@log_object_id int,
@last_logg_date smalldatetime
AS

/* First time used? */
IF NOT EXISTS (SELECT 1 FROM queue_by_day_logg)
BEGIN
	SELECT @last_logg_date = (SELECT min(date_from) FROM queue_logg ql INNER JOIN queues q ON
									ql.queue=q.queue AND
									q.log_object_id=@log_object_id)
END

BEGIN TRAN



CREATE TABLE #queue_logg (
	[queue] [int] NOT NULL ,
	[date_from] [smalldatetime] NOT NULL ,
	[interval] [int] NOT NULL ,
	[offd_direct_call_cnt] [int] NULL ,
	[overflow_in_call_cnt] [int] NULL ,
	[aband_call_cnt] [int] NULL ,
	[overflow_out_call_cnt] [int] NULL ,
	[answ_call_cnt] [int] NULL ,
	[queued_and_answ_call_dur] [int] NULL ,
	[queued_and_aband_call_dur] [int] NULL ,
	[talking_call_dur] [int] NULL ,
	[wrap_up_dur] [int] NULL ,
	[queued_answ_longest_que_dur] [int] NULL ,
	[queued_aband_longest_que_dur] [int] NULL ,
	[avg_avail_member_cnt] [int] NULL ,
	[ans_servicelevel_cnt] [int] NULL ,
	[wait_dur] [int] NULL ,
	[aband_short_call_cnt] [int] NULL ,
	[aband_within_sl_cnt] [int] NULL 
)


CREATE  INDEX #i_r_idx4 ON #queue_logg (queue, date_from)


INSERT INTO #queue_logg
	SELECT * FROM queue_logg WHERE date_from >= @last_logg_date


DELETE FROM queue_by_day_logg
FROM queue_by_day_logg ql 
	INNER JOIN queues q ON ql.queue=q.queue AND q.log_object_id=@log_object_id
WHERE ql.date_from >= @last_logg_date

IF @@ERROR <> 0
BEGIN
	RAISERROR('Error on delete from queue_by_day_logg.',16,1)
	ROLLBACK TRAN
	RETURN
END

INSERT INTO queue_by_day_logg
	SELECT ql.queue,
	ql.date_from,
	sum(offd_direct_call_cnt),
	sum(overflow_in_call_cnt),
	sum(aband_call_cnt),
	sum(overflow_out_call_cnt),
	sum(answ_call_cnt),
	sum(queued_and_answ_call_dur),
	sum(queued_and_aband_call_dur),
	sum(talking_call_dur),
	sum(wrap_up_dur),
	max(queued_answ_longest_que_dur),
	max(queued_aband_longest_que_dur),
	0,
	sum(ans_servicelevel_cnt),
	sum(wait_dur),
	sum(aband_short_call_cnt),
	sum(aband_within_sl_cnt)
FROM #queue_logg ql 
	INNER JOIN queues q ON ql.queue=q.queue AND q.log_object_id=@log_object_id
WHERE ql.date_from >= @last_logg_date
GROUP BY ql.queue,ql.date_from

IF @@ERROR <> 0
BEGIN
	RAISERROR('Error on insert into queue_by_day_logg.',16,1)
	ROLLBACK TRAN
	RETURN
END

COMMIT TRAN

RETURN

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_insert_goal_results_54]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_insert_goal_results_54]
GO

/***************************************/
/* Modified by DavidP 020130 */
/*****************************/
CREATE PROCEDURE [dbo].[p_insert_goal_results_54]
/*
Micke D 2004-02-27 Added 10 minutes intervals on 30 minutes logging
Micke D 2004-08-19 Added 10 minute intervals on 60 minute logging
Micke D 2004-08-19 Added 15 minute intervals on 60 minute logging
*/
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
@start_date anv�nds om man vill agga fr�n specifikt datum
@end_date anv�nds om man vill agga till ett specifikt datum
@rel_start_date anv�nds om man t ex vill agg bak�t en dag hela tiden. Skickar d� in -1
@rel_start_int anv�nds om man t ex vill agg bak�t fyra intervall hela tiden. Skickar d� in -4
*/
/********************************************************/
/* Fetch latest log date and interval                   */
/********************************************************/
SELECT @last_logg_date = date_value , @last_logg_interval = int_value
FROM log_object_detail 
WHERE log_object_id = @log_object_id
AND detail_id = 3

SELECT @stop_date = dateadd(month,1,@last_logg_date)
SELECT @int_per_day= int_value
FROM ccc_system_info 
WHERE [id]=1


/* 

H�r blir det lite nytt  

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
			/* H�r ligger intervallet fortfarande inom dagen s� det �r bara att plussa */
			SELECT @last_logg_interval = @last_logg_interval + @rel_start_int
		END
		ELSE IF  (@rel_start_int + @last_logg_interval >= @int_per_day)
		BEGIN
			/* H�r har intervallet g�tt �ver till n�sta dag vi �kar p� dagen en dag och �ndrar intervallet */
			SELECT @last_logg_date = dateadd ( day,1,@last_logg_date)
			SELECT @last_logg_interval = (@last_logg_interval + @rel_start_int) - (@int_per_day - 1)
		END 
	END 
	ELSE
	BEGIN
		/* H�r �r intervallet ig�r */
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
	H�r blir det nytt
	
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
END

IF  @mindate IS not NULL
BEGIN

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



END
/********************************************************/
/* Add data from #tmp_goal_results to goal_results	*/
/********************************************************/
IF (@CTI_interval_per_hour*4 = @interval_per_hour)
BEGIN
	select '@CTI_interval_per_hour*4 = @interval_per_hour'
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
		ceiling(convert(real,answ_call_cnt)/4.0),
		ceiling(convert(real,aband_call_cnt)/4.0)
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
		answ_call_cnt/3,
		aband_call_cnt/3
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
select 'vanligt'
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
IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION InsertLoggDP
	RETURN
END
COMMIT TRANSACTION InsertLoggDP
SET NOCOUNT OFF

GO

  
GO  
 
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
*/
@log_object_id int,
@start_date smalldatetime = '1900-01-01',
@end_date smalldatetime = '1900-01-01',
@rel_start_date int =0,
@rel_start_int int=0 AS DECLARE @last_logg_date smalldatetime,
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
@start_date anv�nds om man vill agga fr�n specifikt datum
@end_date anv�nds om man vill agga till ett specifikt datum
@rel_start_date anv�nds om man t ex vill agg bak�t en dag hela tiden. Skickar d� in -1
@rel_start_int anv�nds om man t ex vill agg bak�t fyra intervall hela tiden. Skickar d� in -4
*/
/********************************************************/
/* Fetch latest log date and interval                   */
/********************************************************/
SELECT @last_logg_date = date_value , @last_logg_interval = int_value
FROM log_object_detail 
WHERE log_object_id = @log_object_id
AND detail_id = 3

SELECT @stop_date = dateadd(month,1,@last_logg_date)
SELECT @int_per_day= int_value
FROM ccc_system_info 
WHERE [id]=1


/* 

H�r blir det lite nytt  

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
			/* H�r ligger intervallet fortfarande inom dagen s� det �r bara att plussa */
			SELECT @last_logg_interval = @last_logg_interval + @rel_start_int
		END
		ELSE IF  (@rel_start_int + @last_logg_interval >= @int_per_day)
		BEGIN
			/* H�r har intervallet g�tt �ver till n�sta dag vi �kar p� dagen en dag och �ndrar intervallet */
			SELECT @last_logg_date = dateadd ( day,1,@last_logg_date)
			SELECT @last_logg_interval = (@last_logg_interval + @rel_start_int) - (@int_per_day - 1)
		END 
	END 
	ELSE
	BEGIN
		/* H�r �r intervallet ig�r */
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
	H�r blir det nytt
	
*/


DECLARE @mindate smalldatetime, @mininterval int
DECLARE @maxdate smalldatetime, @maxinterval int

SELECT @mindate=MIN(date_from) from #tmp_goal_results
SELECT @mininterval=MIN(interval) FROM #tmp_goal_results
WHERE date_from = @mindate

SELECT @maxdate=MAX(date_from) from #tmp_goal_results
SELECT @maxinterval=MAX(interval) FROM #tmp_goal_results
WHERE date_from = @maxdate

/*Hantera konverteringar mellan olika intervall*/
SELECT @maxinterval = @maxinterval + (@interval_per_hour / @CTI_interval_per_hour ) - 1



IF @maxdate > @stop_date
BEGIN
	SELECT @maxdate = @stop_date
END

IF  @mindate IS not NULL
BEGIN

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



END
/********************************************************/
/* Add data from #tmp_goal_results to goal_results	*/
/********************************************************/
IF (@CTI_interval_per_hour*4 = @interval_per_hour)
BEGIN
	select '@CTI_interval_per_hour*4 = @interval_per_hour'
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
		ceiling(convert(real,answ_call_cnt)/4.0),
		ceiling(convert(real,aband_call_cnt)/4.0)
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
select 'vanligt'
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
IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION InsertLoggDP
	RETURN
END
COMMIT TRANSACTION InsertLoggDP
SET NOCOUNT OFF

GO

  
GO  
 
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

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_insert_agent_logg_54]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_insert_agent_logg_54]
GO

/*************************************/
/* Modified by David P 020130 */
/*************************************/
CREATE PROCEDURE [dbo].[p_insert_agent_logg_54]
/*
Micke D 2004-02-27 Added 10 minutes intervals on 30 minutes logging
Micke D 2004-08-19 Added 10 minute intervals on 60 minute logging
Micke D 2004-08-19 Added 15 minute intervals on 60 minute logging
*/
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
@start_date anv�nds om man vill agga fr�n specifikt datum
@end_date anv�nds om man vill agga till ett specifikt datum
@rel_start_date anv�nds om man t ex vill agg bak�t en dag hela tiden. Skickar d� in -1
@rel_start_int anv�nds om man t ex vill agg bak�t fyra intervall hela tiden. Skickar d� in -4
*/
/********************************************************/
/* Fetch latest log date and interval                   */
/********************************************************/
SELECT @last_logg_date = date_value , @last_logg_interval = int_value
FROM log_object_detail 
WHERE log_object_id = @log_object_id
AND detail_id = 2

SELECT @stop_date = dateadd(month,1,@last_logg_date)
SELECT @int_per_day= int_value
FROM ccc_system_info 
WHERE [id]=1


/* 

H�r blir det lite nytt  

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
			/* H�r ligger intervallet fortfarande inom dagen s� det �r bara att plussa */
			SELECT @last_logg_interval = @last_logg_interval + @rel_start_int
		END
		ELSE IF  (@rel_start_int + @last_logg_interval >= @int_per_day)
		BEGIN
			/* H�r har intervallet g�tt �ver till n�sta dag vi �kar p� dagen en dag och �ndrar intervallet */
			SELECT @last_logg_date = dateadd ( day,1,@last_logg_date)
			SELECT @last_logg_interval = (@last_logg_interval + @rel_start_int) - (@int_per_day - 1)
		END 
	END 
	ELSE
	BEGIN
		/* H�r �r intervallet ig�r */
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
/* Create a temporary table wich we will add data to	*/
/**********************************************************************/
CREATE TABLE #tmp_alogg (
	queue int NOT NULL,
	date_from smalldatetime NOT NULL ,
	interval int NOT NULL,
	agent_id int NOT NULL, 
	agent_name varchar(50) NULL,
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
	transfer_out_call_cnt int NULL)
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
/*
	H�r blir det nytt
	
*/

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
END

IF  @mindate IS not NULL
BEGIN

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
	transfer_out_call_cnt )
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
	round(convert(real,sum(transfer_out_call_cnt))/4.0,0)
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
	transfer_out_call_cnt )
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
	round(convert(real,sum(transfer_out_call_cnt))/4.0,0)
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
	transfer_out_call_cnt )
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
	ceiling(convert(real,sum(transfer_out_call_cnt))/4.0)
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
	transfer_out_call_cnt )
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
	sum(transfer_out_call_cnt)/4
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
	transfer_out_call_cnt )
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
	round(convert(real,sum(transfer_out_call_cnt))/6.0,0)
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
	transfer_out_call_cnt )
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
	round(convert(real,sum(transfer_out_call_cnt))/6.0,0)
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
	transfer_out_call_cnt )
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
	round(convert(real,sum(transfer_out_call_cnt))/6.0,0)
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
	transfer_out_call_cnt )
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
	round(convert(real,sum(transfer_out_call_cnt))/6.0,0)
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
	transfer_out_call_cnt )
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
	ceiling(convert(real,sum(transfer_out_call_cnt))/6.0)
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
	transfer_out_call_cnt )
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
	sum(transfer_out_call_cnt)/6
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
	transfer_out_call_cnt )
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
	ceiling(convert(real,sum(transfer_out_call_cnt))/2.0)
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
	transfer_out_call_cnt )
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
	sum(transfer_out_call_cnt)/2
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
	transfer_out_call_cnt )
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
	round(convert(real,sum(transfer_out_call_cnt))/3.0,0)
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
	transfer_out_call_cnt )
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
	ceiling(convert(real,sum(transfer_out_call_cnt))/3.0)
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
	transfer_out_call_cnt )
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
	sum(transfer_out_call_cnt)/3
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
	transfer_out_call_cnt )
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
	sum(transfer_out_call_cnt)
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
--DECLARE @maxdate smalldatetime, @maxinterval int

IF (@start_date < '1970-01-01') AND (@end_date < '1970-01-01')
BEGIN
	SELECT @maxdate = MAX(date_from) FROM #tmp_alogg
	SELECT @maxinterval = MAX(interval) FROM #tmp_alogg
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
	SELECT @maxdate = MAX(date_from) FROM agent_logg
	SELECT @maxinterval = MAX(interval) FROM agent_logg
	WHERE date_from = @maxdate

	IF  @maxdate IS not NULL
	BEGIN
		UPDATE log_object_detail
		SET date_value = @maxdate,
            		int_value = @maxinterval 
		WHERE log_object_id = @log_object_id
		AND detail_id = 2
	END    
END

IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION 
	RETURN
END
/******** NEW 5.2 Business Scorecard ********/
DECLARE @text varchar(500)
SELECT @text = 'p_insert_agent_by_day_logg '+convert(varchar(10),@log_object_id)+
	', '+'
	'+''''+convert(varchar(10),@last_logg_date,120)+''''
EXECUTE (@text)
/******** NEW 5.2 Business Scorecard ********/
IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION 
	RETURN
END
COMMIT TRANSACTION
SET NOCOUNT OFF

GO

  
GO  
 
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
*/
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
@start_date anv�nds om man vill agga fr�n specifikt datum
@end_date anv�nds om man vill agga till ett specifikt datum
@rel_start_date anv�nds om man t ex vill agg bak�t en dag hela tiden. Skickar d� in -1
@rel_start_int anv�nds om man t ex vill agg bak�t fyra intervall hela tiden. Skickar d� in -4
*/
/********************************************************/
/* Fetch latest log date and interval                   */
/********************************************************/
SELECT @last_logg_date = date_value , @last_logg_interval = int_value
FROM log_object_detail 
WHERE log_object_id = @log_object_id
AND detail_id = 2

SELECT @stop_date = dateadd(month,1,@last_logg_date)
SELECT @int_per_day= int_value
FROM ccc_system_info 
WHERE [id]=1


/* 

H�r blir det lite nytt  

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
			/* H�r ligger intervallet fortfarande inom dagen s� det �r bara att plussa */
			SELECT @last_logg_interval = @last_logg_interval + @rel_start_int
		END
		ELSE IF  (@rel_start_int + @last_logg_interval >= @int_per_day)
		BEGIN
			/* H�r har intervallet g�tt �ver till n�sta dag vi �kar p� dagen en dag och �ndrar intervallet */
			SELECT @last_logg_date = dateadd ( day,1,@last_logg_date)
			SELECT @last_logg_interval = (@last_logg_interval + @rel_start_int) - (@int_per_day - 1)
		END 
	END 
	ELSE
	BEGIN
		/* H�r �r intervallet ig�r */
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
/* Create a temporary table wich we will add data to	*/
/**********************************************************************/
CREATE TABLE #tmp_alogg (
	queue int NOT NULL,
	date_from smalldatetime NOT NULL ,
	interval int NOT NULL,
	agent_id int NOT NULL, 
	agent_name varchar(50) NULL,
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
/*
	H�r blir det nytt
	
*/

DECLARE @mindate smalldatetime, @mininterval int
DECLARE @maxdate smalldatetime, @maxinterval int


SELECT @mindate=MIN(date_from) from #tmp_alogg
SELECT @mininterval=MIN(interval) FROM #tmp_alogg
WHERE date_from = @mindate

SELECT @maxdate=MAX(date_from) from #tmp_alogg
SELECT @maxinterval=MAX(interval) FROM #tmp_alogg
WHERE date_from = @maxdate

/*Hantera konverteringar mellan olika intervall*/
SELECT @maxinterval = @maxinterval + (@interval_per_hour / @CTI_interval_per_hour ) - 1

IF @maxdate > @stop_date
BEGIN
	SELECT @maxdate = @stop_date
END

IF  @mindate IS not NULL
BEGIN

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
	admin_dur )
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
--DECLARE @maxdate smalldatetime, @maxinterval int

IF (@start_date < '1970-01-01') AND (@end_date < '1970-01-01')
BEGIN
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
END
ELSE
BEGIN
	SELECT @maxdate = MAX(date_from) FROM agent_logg
	SELECT @maxinterval = MAX(interval) FROM agent_logg
	WHERE date_from = @maxdate

	IF  @maxdate IS not NULL
	BEGIN
		UPDATE log_object_detail
		SET date_value = @maxdate,
            		int_value = @maxinterval 
		WHERE log_object_id = @log_object_id
		AND detail_id = 2
	END    
END

IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION 
	RETURN
END
/******** NEW 5.2 Business Scorecard ********/
DECLARE @text varchar(500)
SELECT @text = 'p_insert_agent_by_day_logg '+convert(varchar(10),@log_object_id)+
	', '+'
	'+''''+convert(varchar(10),@last_logg_date,120)+''''
EXECUTE (@text)
/******** NEW 5.2 Business Scorecard ********/
IF @@ERROR <> 0
BEGIN
	SELECT 'ERROR: Rollback issued!!!'
	ROLLBACK TRANSACTION 
	RETURN
END
COMMIT TRANSACTION
SET NOCOUNT OFF

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_insert_agent_by_day_logg]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_insert_agent_by_day_logg]
GO

CREATE PROCEDURE [dbo].[p_insert_agent_by_day_logg]
@log_object_id int,
@last_logg_date smalldatetime
AS

/* First time used? */
IF NOT EXISTS (SELECT 1 FROM agent_by_day_logg)
BEGIN
	SELECT @last_logg_date = (SELECT min(date_from) FROM agent_logg ql INNER JOIN queues q ON
									ql.queue=q.queue AND
									q.log_object_id=@log_object_id)
END


CREATE TABLE #agent_logg_temp (
	[queue] [int] NOT NULL ,
	[date_from] [smalldatetime] NOT NULL ,
	[interval] [int] NOT NULL ,
	[agent_id] [int] NOT NULL ,
	[agent_name] [nvarchar] (50) NULL ,
	[avail_dur] [int] NULL ,
	[tot_work_dur] [int] NULL ,
	[talking_call_dur] [int] NULL ,
	[pause_dur] [int] NULL ,
	[wait_dur] [int] NULL ,
	[wrap_up_dur] [int] NULL ,
	[answ_call_cnt] [int] NULL ,
	[direct_out_call_cnt] [int] NULL ,
	[direct_out_call_dur] [int] NULL ,
	[direct_in_call_cnt] [int] NULL ,
	[direct_in_call_dur] [int] NULL ,
	[transfer_out_call_cnt] [int] NULL ,
	[admin_dur] [int] NULL		
) 

CREATE  INDEX #i_r_idx4 ON #agent_logg_temp (date_from, agent_id)


INSERT INTO #agent_logg_temp
	SELECT * FROM agent_logg WHERE date_from >= @last_logg_date

CREATE TABLE #agent_logg (
	date_from smalldatetime NOT NULL ,
	interval int not null,
	agent_id int NOT NULL, 
	agent_name nvarchar(50) null,
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
	transfer_out_call_cnt int NULL)


BEGIN TRAN

DELETE FROM agent_by_day_logg
FROM agent_by_day_logg ab
	INNER JOIN agent_logg al ON ab.agent_id = al.agent_id AND ab.date_from = al.date_from
	INNER JOIN agent_info ai ON ai.agent_id = al.agent_id AND ai.log_object_id = @log_object_id
WHERE al.date_from >= @last_logg_date

IF @@ERROR <> 0
BEGIN
	RAISERROR('Error on delete from agent_by_day_logg.',16,1)
	ROLLBACK TRAN
	RETURN
END

INSERT INTO #agent_logg
	SELECT al.date_from,
	al.interval,
	al.agent_id,
	max(al.agent_name),
	max(avail_dur),
	max(tot_work_dur),
	sum(talking_call_dur),
	max(pause_dur),
	max(wait_dur),
	sum(wrap_up_dur),
	sum(answ_call_cnt),
	sum(direct_out_call_cnt),
	sum(direct_out_call_dur),
	sum(direct_in_call_cnt),
	sum(direct_in_call_dur),
	sum(transfer_out_call_cnt)
FROM #agent_logg_temp al
	INNER JOIN agent_info ai ON ai.agent_id = al.agent_id AND ai.log_object_id = @log_object_id
WHERE date_from >= @last_logg_date
GROUP BY 
	al.date_from,
	al.interval,
	al.agent_id

IF @@ERROR <> 0
BEGIN
	RAISERROR('Error on insert into #agent_logg.',16,1)
	ROLLBACK TRAN
	RETURN
END

INSERT INTO agent_by_day_logg
	SELECT date_from,
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
	sum(transfer_out_call_cnt)
FROM #agent_logg
GROUP BY date_from,agent_id

IF @@ERROR <> 0
BEGIN
	RAISERROR('Error on insert into agent_by_day_logg.',16,1)
	ROLLBACK TRAN
	RETURN
END

COMMIT TRAN

RETURN

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_create_logDB]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_create_logDB]
GO

CREATE  procedure [dbo].[p_create_logDB]

/**********************************************/
/* To add logDB information to AggDB */
/**********************************************/

@acd_type_id int,
@logDB varchar(50)

as


declare @id int
select @id = isnull(max(log_object_id),0) + 1 from log_object

/*Check if ACD Type exists*/

if exists (select 1 from acd_type where acd_type_id = @acd_type_id)
begin
	insert into log_object(log_object_id, acd_type_id, log_object_desc,  logDB_name,intervals_per_day)
	select @id, @acd_type_id, acd_type_desc, @logDB,96 
	from acd_type
	where acd_type_id = @acd_type_id

	insert into log_object_detail(log_object_id, detail_id, detail_desc, proc_name, int_value, date_value)
	select @id,detail_id, detail_name, proc_name,0,'2002-01-01' 
	from acd_type_detail
	where acd_type_id = @acd_type_id
end

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_common_get_db_data]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_common_get_db_data]
GO

CREATE PROCEDURE [dbo].[p_common_get_db_data]

AS

SELECT CONVERT(nvarchar(50),SERVERPROPERTY('ServerName')) + '/' + DB_NAME() as aggdb

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_archive_queue_logg]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_archive_queue_logg]
GO

CREATE  PROCEDURE [dbo].[p_archive_queue_logg]
@date_from smalldatetime,
@date_to smalldatetime

AS

CREATE TABLE #tmp_q_logg (
	[queue] [int] NOT NULL ,
	[date_from] [smalldatetime] NOT NULL ,
	[interval] [int] NOT NULL ,
	[offd_direct_call_cnt] [int] NULL ,
	[overflow_in_call_cnt] [int] NULL ,
	[aband_call_cnt] [int] NULL ,
	[overflow_out_call_cnt] [int] NULL ,
	[answ_call_cnt] [int] NULL ,
	[queued_and_answ_call_dur] [int] NULL ,
	[queued_and_aband_call_dur] [int] NULL ,
	[talking_call_dur] [int] NULL ,
	[wrap_up_dur] [int] NULL ,
	[queued_answ_longest_que_dur] [int] NULL ,
	[queued_aband_longest_que_dur] [int] NULL ,
	[avg_avail_member_cnt] [int] NULL ,
	[ans_servicelevel_cnt] [int] NULL ,
	[wait_dur] [int] NULL ,
	[aband_short_call_cnt] [int] NULL ,
	[aband_within_sl_cnt] [int] NULL 
) 

-- log_object har en FK till acd_type s� vi m�ste s�kerst�lla att de
-- acd_type id som vi vill flytta verkligen finns
INSERT INTO xxx.dbo.acd_type (acd_type_id, acd_type_desc)
SELECT acd_type_id, acd_type_desc
FROM acd_type
WHERE acd_type_id NOT IN (SELECT acd_type_id FROM xxx.dbo.acd_type)

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- agent_info och queues har en FK till log_object s� vi m�ste s�kerst�lla att de
-- log_object id som vi vill flytta verkligen finns
--SET IDENTITY_INSERT xxx.dbo.log_object ON
INSERT INTO xxx.dbo.log_object (log_object_id, acd_type_id, log_object_desc, logdb_name, intervals_per_day, default_service_level_sec, default_short_call_treshold)
SELECT log_object_id, acd_type_id, log_object_desc, logdb_name, intervals_per_day, default_service_level_sec, default_short_call_treshold
FROM log_object
WHERE log_object_id NOT IN (SELECT log_object_id FROM xxx.dbo.log_object)

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
--SET IDENTITY_INSERT xxx.dbo.log_object OFF

-- queue_logg har en FK till queues s� vi m�ste s�kerst�lla att de 
-- k� id som vi vill flytta verkligen finns
SET IDENTITY_INSERT xxx.dbo.queues ON

INSERT INTO xxx.dbo.queues (queue, queue_desc, log_object_id, orig_queue_id)
SELECT queue, orig_desc, log_object_id, orig_queue_id
FROM queues
WHERE queue NOT IN (SELECT queue FROM xxx.dbo.queues)

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
SET IDENTITY_INSERT xxx.dbo.queues OFF

-- Starta transaktion
BEGIN TRAN

-- L�gg in i temptabellen
INSERT INTO #tmp_q_logg
SELECT * 
FROM queue_logg
WHERE date_from BETWEEN @date_from AND @date_to

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- Sudda fr�n arkivet
DELETE xxx.dbo.queue_logg
FROM #tmp_q_logg a1
INNER JOIN xxx.dbo.queue_logg a2
ON a1.date_from = a2.date_from AND a1.queue = a2.queue AND a1.interval = a2.interval

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- L�gg in i arkivet
INSERT INTO xxx.dbo.queue_logg
SELECT *
FROM #tmp_q_logg

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- Sudda fr�n loggen
DELETE queue_logg
FROM #tmp_q_logg a1
INNER JOIN queue_logg a2
ON a1.date_from = a2.date_from AND a1.queue = a2.queue AND a1.interval = a2.interval

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
ELSE
BEGIN
	COMMIT TRAN
END

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_archive_agent_report]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_archive_agent_report]
GO

CREATE  PROCEDURE [dbo].[p_archive_agent_report]
@date_from smalldatetime,
@date_to smalldatetime

AS

-- Skapa temptabell
CREATE TABLE #tmp_a_report (
	[id_nr] [int] NOT NULL ,
	[date] [smalldatetime] NOT NULL ,
	[interval] [smalldatetime] NOT NULL ,
	[agt_id] [int] NOT NULL ,
	[agt_name] [varchar] (50) NULL ,
	[acd_dn_number] [varchar] (10) NOT NULL ,
	[calls_answd] [int] NULL ,
	[non_actvy_calls] [int] NULL ,
	[acd_calls_xferd] [int] NULL ,
	[in_dn_calls] [int] NULL ,
	[out_dn_calls] [int] NULL ,
	[dn_calls_xferd] [int] NULL ,
	[short_calls] [int] NULL ,
	[total_acd_talk_time] [int] NULL ,
	[total_not_ready_time] [int] NULL ,
	[total_in_dn_time] [int] NULL ,
	[total_out_dn_time] [int] NULL ,
	[total_wait_time] [int] NULL ,
	[total_hold_time] [int] NULL ,
	[total_walk_time] [int] NULL ,
	[total_busy_time] [int] NULL ,
	[total_login_time] [int] NULL ,
	[total_consult_time] [int] NULL ,
	[total_staff_time] [int] NULL 
)


-- Starta transaktion
BEGIN TRAN

-- L�gg in i temptabellen
INSERT INTO #tmp_a_report
SELECT * 
FROM t_agent_report
WHERE date BETWEEN @date_from AND @date_to

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- Sudda fr�n arkivet
DELETE AggDB_Archive.dbo.t_agent_report
FROM #tmp_a_report a1
INNER JOIN AggDB_Archive.dbo.t_agent_report a2
ON a1.date = a2.date AND a1.id_nr = a2.id_nr AND a1.interval = a2.interval AND a1.agt_id = a2.agt_id 
	AND a1.acd_dn_number COLLATE SQL_Latin1_General_CP1_CI_AS = a2.acd_dn_number COLLATE SQL_Latin1_General_CP1_CI_AS


IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- L�gg in i arkivet
INSERT INTO AggDB_Archive.dbo.t_agent_report
SELECT *
FROM #tmp_a_report

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- Sudda fr�n loggen
DELETE t_agent_report
FROM #tmp_a_report a1
INNER JOIN t_agent_report a2
ON a1.date = a2.date AND a1.id_nr = a2.id_nr AND a1.interval = a2.interval AND a1.agt_id = a2.agt_id 
	AND a1.acd_dn_number COLLATE SQL_Latin1_General_CP1_CI_AS = a2.acd_dn_number COLLATE SQL_Latin1_General_CP1_CI_AS
--	AND CAST(a1.acd_dn_number AS varchar(10)) COLLATE SQL_Latin1_General_CP1_CI_AS) = CAST(a2.acd_dn_number AS varchar(10)) COLLATE SQL_Latin1_General_CP1_CI_AS)

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
ELSE
BEGIN
	COMMIT TRAN
END

GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_archive_agent_logg]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_archive_agent_logg]
GO

CREATE   PROCEDURE [dbo].[p_archive_agent_logg] 
@date_from smalldatetime,
@date_to smalldatetime

AS

-- Skapa temptabell
CREATE TABLE #tmp_a_logg (
	[queue] [int] NOT NULL ,
	[date_from] [smalldatetime] NOT NULL ,
	[interval] [int] NOT NULL ,
	[agent_id] [int] NOT NULL ,
	[agent_name] [nvarchar] (50) NULL ,
	[avail_dur] [int] NULL ,
	[tot_work_dur] [int] NULL ,
	[talking_call_dur] [int] NULL ,
	[pause_dur] [int] NULL ,
	[wait_dur] [int] NULL ,
	[wrap_up_dur] [int] NULL ,
	[answ_call_cnt] [int] NULL ,
	[direct_out_call_cnt] [int] NULL ,
	[direct_out_call_dur] [int] NULL ,
	[direct_in_call_cnt] [int] NULL ,
	[direct_in_call_dur] [int] NULL ,
	[transfer_out_call_cnt] [int] NULL 
) 

-- log_object har en FK till acd_type s� vi m�ste s�kerst�lla att de
-- acd_type id som vi vill flytta verkligen finns
INSERT INTO xxx.dbo.acd_type (acd_type_id, acd_type_desc)
SELECT acd_type_id, acd_type_desc
FROM acd_type
WHERE acd_type_id NOT IN (SELECT acd_type_id FROM xxx.dbo.acd_type)

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- agent_info och queues har en FK till log_object s� vi m�ste s�kerst�lla att de
-- log_object id som vi vill flytta verkligen finns
--SET IDENTITY_INSERT xxx.dbo.log_object ON
INSERT INTO xxx.dbo.log_object (log_object_id, acd_type_id, log_object_desc, logdb_name, intervals_per_day, default_service_level_sec, default_short_call_treshold)
SELECT log_object_id, acd_type_id, log_object_desc, logdb_name, intervals_per_day, default_service_level_sec, default_short_call_treshold
FROM log_object
WHERE log_object_id NOT IN (SELECT log_object_id FROM xxx.dbo.log_object)

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
--SET IDENTITY_INSERT xxx.dbo.log_object OFF

-- queue_logg har en FK till agent_info s� vi m�ste s�kerst�lla att de 
-- agent id som vi vill flytta verkligen finns
SET IDENTITY_INSERT xxx.dbo.agent_info ON
INSERT INTO xxx.dbo.agent_info (agent_id, agent_name, is_active, log_object_id, orig_agent_id)
SELECT agent_id, agent_name, is_active, log_object_id, orig_agent_id
FROM agent_info
WHERE agent_id NOT IN (SELECT agent_id FROM xxx.dbo.agent_info)

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
SET IDENTITY_INSERT xxx.dbo.agent_info OFF

-- queue_logg har en FK till queues s� vi m�ste s�kerst�lla att de 
-- k� id som vi vill flytta verkligen finns
SET IDENTITY_INSERT xxx.dbo.queues ON

INSERT INTO xxx.dbo.queues (queue, queue_desc, log_object_id, orig_queue_id)
SELECT queue, orig_desc, log_object_id, orig_queue_id
FROM queues
WHERE queue NOT IN (SELECT queue FROM xxx.dbo.queues)

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
SET IDENTITY_INSERT xxx.dbo.queues OFF

-- Starta transaktion
BEGIN TRAN

-- L�gg in i temptabellen
INSERT INTO #tmp_a_logg
SELECT * 
FROM agent_logg
WHERE date_from BETWEEN @date_from AND @date_to

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- Sudda fr�n arkivet
DELETE xxx.dbo.agent_logg
FROM #tmp_a_logg a1
INNER JOIN xxx.dbo.agent_logg a2
ON a1.date_from = a2.date_from AND a1.queue = a2.queue AND a1.interval = a2.interval AND a1.agent_id = a2.agent_id

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- L�gg in i arkivet
INSERT INTO xxx.dbo.agent_logg
SELECT *
FROM #tmp_a_logg

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- Sudda fr�n loggen
DELETE agent_logg
FROM #tmp_a_logg a1
INNER JOIN agent_logg a2
ON a1.date_from = a2.date_from AND a1.queue = a2.queue AND a1.interval = a2.interval AND a1.agent_id = a2.agent_id

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
ELSE
BEGIN
	COMMIT TRAN
END

GO

  
GO  
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (94,'7.0.94') 
