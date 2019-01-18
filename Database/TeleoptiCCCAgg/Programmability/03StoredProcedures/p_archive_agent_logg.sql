IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_archive_agent_logg]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_archive_agent_logg]
GO

CREATE   PROCEDURE [dbo].[p_archive_agent_logg] 
@date_from smalldatetime,
@date_to smalldatetime

WITH EXECUTE AS OWNER
AS
--Hej, Hej, Fake
-- Skapa temptabell
CREATE TABLE #tmp_a_logg (
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

