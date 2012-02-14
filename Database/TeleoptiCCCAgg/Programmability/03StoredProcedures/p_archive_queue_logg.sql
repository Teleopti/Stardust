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

