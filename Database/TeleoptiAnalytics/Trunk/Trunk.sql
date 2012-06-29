----------------  
--Name: DavidJ
--Date: 2012-06-28
--Desc: #19806 - Fix two Identity Columns and Add correct Clustered index on queue_logg + agent_logg
----------------

------------------------
--Add IDENTITY column on table [queues]
------------------------
--drop FKs
ALTER TABLE dbo.queue_logg DROP CONSTRAINT FK_queue_logg_queues
ALTER TABLE dbo.queues DROP CONSTRAINT FK_queues_log_object
ALTER TABLE dbo.agent_logg DROP CONSTRAINT FK_agent_logg_queues
GO

--move data to temp table
CREATE TABLE dbo.Tmp_queues
	(
	queue int NOT NULL IDENTITY (1, 1),
	orig_desc nvarchar(50) NULL,
	log_object_id int NOT NULL,
	orig_queue_id int NULL,
	display_desc nvarchar(50) NULL
	)
	
SET IDENTITY_INSERT dbo.Tmp_queues ON
GO
INSERT INTO dbo.Tmp_queues (queue, orig_desc, log_object_id, orig_queue_id, display_desc)
SELECT queue, orig_desc, log_object_id, orig_queue_id, display_desc FROM dbo.queues WITH (HOLDLOCK TABLOCKX)

SET IDENTITY_INSERT dbo.Tmp_queues OFF
GO

--swap names
DROP TABLE dbo.queues
GO
EXECUTE sp_rename N'dbo.Tmp_queues', N'queues', 'OBJECT' 
GO

--Re-Add contstraints
ALTER TABLE dbo.queues ADD CONSTRAINT
	PK_queues PRIMARY KEY CLUSTERED 
	(
	queue
	)


ALTER TABLE [dbo].[queue_logg]  ADD CONSTRAINT [FK_queue_logg_queues] FOREIGN KEY([queue]) REFERENCES [dbo].[queues] ([queue])
ALTER TABLE dbo.queues ADD CONSTRAINT FK_queues_log_object FOREIGN KEY (log_object_id) REFERENCES dbo.log_object
ALTER TABLE dbo.agent_logg ADD CONSTRAINT FK_agent_logg_queues FOREIGN KEY (queue) REFERENCES dbo.queues

------------------------
--Add IDENTITY column on table [agent_info]
------------------------
ALTER TABLE dbo.agent_info DROP CONSTRAINT FK_agent_info_log_object
ALTER TABLE dbo.quality_logg DROP CONSTRAINT FK_quality_logg_agent_info
ALTER TABLE dbo.agent_logg DROP CONSTRAINT FK_agent_logg_agent_info

--move data to tmp table
CREATE TABLE dbo.Tmp_agent_info
	(
	Agent_id int NOT NULL IDENTITY (1, 1),
	Agent_name nvarchar(50) NOT NULL,
	is_active bit NULL,
	log_object_id int NOT NULL,
	orig_agent_id nvarchar(50) NOT NULL
	)
GO
SET IDENTITY_INSERT dbo.Tmp_agent_info ON
GO
INSERT INTO dbo.Tmp_agent_info (Agent_id, Agent_name, is_active, log_object_id, orig_agent_id)
SELECT Agent_id, Agent_name, is_active, log_object_id, orig_agent_id FROM dbo.agent_info WITH (HOLDLOCK TABLOCKX)
SET IDENTITY_INSERT dbo.Tmp_agent_info OFF
GO

--swap names
DROP TABLE dbo.agent_info
GO
EXECUTE sp_rename N'dbo.Tmp_agent_info', N'agent_info', 'OBJECT' 
GO

--re-Add constraints and indexes
ALTER TABLE dbo.agent_info ADD CONSTRAINT PK_agent_info PRIMARY KEY CLUSTERED (Agent_id)

CREATE UNIQUE NONCLUSTERED INDEX uix_orig_agent_id ON dbo.agent_info
	(
	log_object_id,
	orig_agent_id
	)
GO
ALTER TABLE dbo.agent_info ADD CONSTRAINT FK_agent_info_log_object FOREIGN KEY (log_object_id) REFERENCES dbo.log_object (log_object_id)
ALTER TABLE dbo.agent_logg ADD CONSTRAINT FK_agent_logg_agent_info FOREIGN KEY (agent_id) REFERENCES dbo.agent_info	(Agent_id)
ALTER TABLE dbo.quality_logg ADD CONSTRAINT	FK_quality_logg_agent_info FOREIGN KEY (agent_id) REFERENCES dbo.agent_info (Agent_id)

------------------------
--create a better clustered key [agent_logg]
------------------------
EXEC sp_rename N'[dbo].[agent_logg].[PK_agent_logg]', N'PK_agent_logg_toBeDropped', N'INDEX'

CREATE TABLE dbo.tmp_agent_logg(
	[queue] [int] NOT NULL,
	[date_from] [smalldatetime] NOT NULL,
	[interval] [int] NOT NULL,
	[agent_id] [int] NOT NULL,
	[agent_name] [nvarchar](50) NULL,
	[avail_dur] [int] NULL,
	[tot_work_dur] [int] NULL,
	[talking_call_dur] [int] NULL,
	[pause_dur] [int] NULL,
	[wait_dur] [int] NULL,
	[wrap_up_dur] [int] NULL,
	[answ_call_cnt] [int] NULL,
	[direct_out_call_cnt] [int] NULL,
	[direct_out_call_dur] [int] NULL,
	[direct_in_call_cnt] [int] NULL,
	[direct_in_call_dur] [int] NULL,
	[transfer_out_call_cnt] [int] NULL,
	[admin_dur] [int] NULL
	)

--First create the correct order of data
CREATE CLUSTERED INDEX [CIX_agent_logg] ON [dbo].[tmp_agent_logg]
(
	[date_from] ASC,
	[agent_id] ASC,
	[queue] ASC,
	[interval] ASC
)

--For, now the PK will be the same as the Clsutered Index, but ordered differently.
--This a) might be usefull for some queries, and it will help us when/if we at some point need to change the table order (again)
ALTER TABLE [dbo].[tmp_agent_logg] ADD  CONSTRAINT [PK_agent_logg] PRIMARY KEY NONCLUSTERED
(
	[queue] ASC,
	[date_from] ASC,
	[interval] ASC,
	[agent_id] ASC
)

INSERT INTO [dbo].[tmp_agent_logg]
           ([queue]
           ,[date_from]
           ,[interval]
           ,[agent_id]
           ,[agent_name]
           ,[avail_dur]
           ,[tot_work_dur]
           ,[talking_call_dur]
           ,[pause_dur]
           ,[wait_dur]
           ,[wrap_up_dur]
           ,[answ_call_cnt]
           ,[direct_out_call_cnt]
           ,[direct_out_call_dur]
           ,[direct_in_call_cnt]
           ,[direct_in_call_dur]
           ,[transfer_out_call_cnt]
           ,[admin_dur])
SELECT [queue]
      ,[date_from]
      ,[interval]
      ,[agent_id]
      ,[agent_name]
      ,[avail_dur]
      ,[tot_work_dur]
      ,[talking_call_dur]
      ,[pause_dur]
      ,[wait_dur]
      ,[wrap_up_dur]
      ,[answ_call_cnt]
      ,[direct_out_call_cnt]
      ,[direct_out_call_dur]
      ,[direct_in_call_cnt]
      ,[direct_in_call_dur]
      ,[transfer_out_call_cnt]
      ,[admin_dur]
FROM [dbo].[agent_logg]

--swap names
DROP TABLE [dbo].[agent_logg]
GO
EXECUTE sp_rename N'dbo.Tmp_agent_logg', N'agent_logg', 'OBJECT' 
GO

ALTER TABLE [dbo].[agent_logg] ADD CONSTRAINT [FK_agent_logg_agent_info] FOREIGN KEY([agent_id]) REFERENCES [dbo].[agent_info] ([Agent_id])
ALTER TABLE [dbo].[agent_logg] ADD CONSTRAINT [FK_agent_logg_queues] FOREIGN KEY([queue]) REFERENCES [dbo].[queues] ([queue])
GO

------------------------
--create a better clustered key queue_logg
------------------------
EXEC sp_rename N'[dbo].[queue_logg].[PK_queue_logg]', N'PK_queue_logg_toBeDropped', N'INDEX'

CREATE TABLE [dbo].[tmp_queue_logg](
	[queue] [int] NOT NULL,
	[date_from] [smalldatetime] NOT NULL,
	[interval] [int] NOT NULL,
	[offd_direct_call_cnt] [int] NULL,
	[overflow_in_call_cnt] [int] NULL,
	[aband_call_cnt] [int] NULL,
	[overflow_out_call_cnt] [int] NULL,
	[answ_call_cnt] [int] NULL,
	[queued_and_answ_call_dur] [int] NULL,
	[queued_and_aband_call_dur] [int] NULL,
	[talking_call_dur] [int] NULL,
	[wrap_up_dur] [int] NULL,
	[queued_answ_longest_que_dur] [int] NULL,
	[queued_aband_longest_que_dur] [int] NULL,
	[avg_avail_member_cnt] [int] NULL,
	[ans_servicelevel_cnt] [int] NULL,
	[wait_dur] [int] NULL,
	[aband_short_call_cnt] [int] NULL,
	[aband_within_sl_cnt] [int] NULL
)

--First create the correct order of data
CREATE CLUSTERED INDEX [CIX_queue_logg] ON [dbo].[tmp_queue_logg]
(
	[date_from] ASC,
	[queue] ASC,
	[interval] ASC
)

--For, now the PK will be the same as the Clsutered Index, but ordered differently.
--This a) might be usefull for some queries, and it will help us when/if we at some point need to change the table order (again)
ALTER TABLE [dbo].[tmp_queue_logg] ADD  CONSTRAINT [PK_queue_logg] PRIMARY KEY NONCLUSTERED
(
	[queue] ASC,
	[date_from] ASC,
	[interval] ASC
)

--swap names
DROP TABLE [dbo].[queue_logg]
GO
EXECUTE sp_rename N'dbo.Tmp_queue_logg', N'queue_logg', 'OBJECT' 
GO

ALTER TABLE [dbo].[queue_logg] ADD CONSTRAINT [FK_queue_logg_queues] FOREIGN KEY([queue]) REFERENCES [dbo].[queues] ([queue])
GO
