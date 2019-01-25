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
	[agent_name] [nvarchar] (100) NULL ,
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
	agent_name nvarchar(100) null,
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

