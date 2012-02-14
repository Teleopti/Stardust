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

