----------------  
--Name: Karin & David
--Date: 2014-10-15
--Desc: PBI #30787 - redesign fact_queue
----------------
ALTER TABLE [mart].[fact_queue] DROP CONSTRAINT [FK_fact_queue_dim_date_Local]
ALTER TABLE [mart].[fact_queue] DROP CONSTRAINT [FK_fact_queue_dim_date_UTC]
ALTER TABLE [mart].[fact_queue] DROP CONSTRAINT [FK_fact_queue_dim_interval_Local]
ALTER TABLE [mart].[fact_queue] DROP CONSTRAINT [FK_fact_queue_dim_interval_UTC]
ALTER TABLE [mart].[fact_queue] DROP CONSTRAINT [FK_fact_queue_dim_queue]
GO
ALTER TABLE [mart].[fact_queue] DROP CONSTRAINT [DF_fact_queue_statistics_datasource_id]
ALTER TABLE [mart].[fact_queue] DROP CONSTRAINT [DF_fact_queue_statistics_datasource_update_date]
ALTER TABLE [mart].[fact_queue] DROP CONSTRAINT [DF_fact_queue_statistics_insert_date]
ALTER TABLE [mart].[fact_queue] DROP CONSTRAINT [DF_fact_queue_statistics_update_date]
GO

EXEC dbo.sp_rename @objname = N'[mart].[fact_queue]', @newname = N'fact_queue_old', @objtype = N'OBJECT'
GO
EXEC sp_rename N'[mart].[fact_queue_old].[PK_fact_queue]', N'PK_fact_queue_old', N'INDEX'
GO

CREATE TABLE [mart].[fact_queue](
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[queue_id] [int] NOT NULL,
	[offered_calls] [decimal](19, 0) NULL,
	[answered_calls] [decimal](19, 0) NULL,
	[answered_calls_within_SL] [decimal](19, 0) NULL,
	[abandoned_calls] [decimal](19, 0) NULL,
	[abandoned_calls_within_SL] [decimal](19, 0) NULL,
	[abandoned_short_calls] [decimal](19, 0) NULL,
	[overflow_out_calls] [decimal](19, 0) NULL,
	[overflow_in_calls] [decimal](19, 0) NULL,
	[talk_time_s] [decimal](19, 0) NULL,
	[after_call_work_s] [decimal](19, 0) NULL,
	[handle_time_s] [decimal](19, 0) NULL,
	[speed_of_answer_s] [decimal](19, 0) NULL,
	[time_to_abandon_s] [decimal](19, 0) NULL,
	[longest_delay_in_queue_answered_s] [decimal](19, 0) NULL,
	[longest_delay_in_queue_abandoned_s] [decimal](19, 0) NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_queue_statistics_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_queue_statistics_insert_date]  DEFAULT (getdate())
CONSTRAINT [PK_fact_queue] PRIMARY KEY CLUSTERED 
(
	[queue_id] ASC,
	[date_id] ASC,
	[interval_id] ASC
)
)
GO

ALTER TABLE [mart].[fact_queue]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_queue_dim_date_UTC] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_date_UTC]
GO

ALTER TABLE [mart].[fact_queue]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_queue_dim_interval_UTC] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_interval_UTC]
GO

ALTER TABLE [mart].[fact_queue]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_queue_dim_queue] FOREIGN KEY([queue_id])
REFERENCES [mart].[dim_queue] ([queue_id])
ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_queue]
GO

--Enable toggle
update mart.sys_configuration
set value='True'
where [key]='PBI30787OnlyLatestQueueAgentStatistics'

----------------  
--Name: Karin & David
--Date: 2014-10-20
--Desc: PBI #30787 - redesign fact_agent
----------------
ALTER TABLE [mart].[fact_agent] DROP CONSTRAINT [FK_fact_agent_dim_agent]
ALTER TABLE [mart].[fact_agent] DROP CONSTRAINT [FK_fact_agent_dim_date_Local]
ALTER TABLE [mart].[fact_agent] DROP CONSTRAINT [FK_fact_agent_dim_date_UTC]
ALTER TABLE [mart].[fact_agent] DROP CONSTRAINT [FK_fact_agent_dim_interval_Local]
ALTER TABLE [mart].[fact_agent] DROP CONSTRAINT [FK_fact_agent_dim_interval_UTC]

GO
ALTER TABLE [mart].[fact_agent] DROP CONSTRAINT [DF_fact_agent_datasource_id]
ALTER TABLE [mart].[fact_agent] DROP CONSTRAINT [DF_fact_agent_insert_date]
ALTER TABLE [mart].[fact_agent] DROP CONSTRAINT [DF_fact_agent_update_date]
GO
EXEC dbo.sp_rename @objname = N'[mart].[fact_agent]', @newname = N'fact_agent_old', @objtype = N'OBJECT'
GO
EXEC sp_rename N'[mart].[fact_agent_old].[PK_fact_agent]', N'PK_fact_agent_old', N'INDEX'
GO
CREATE TABLE [mart].[fact_agent](
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[acd_login_id] [int] NOT NULL,
	[ready_time_s] [decimal](19, 0) NULL,
	[logged_in_time_s] [decimal](19, 0) NULL,
	[not_ready_time_s] [decimal](19, 0) NULL,
	[idle_time_s] [decimal](19, 0) NULL,
	[direct_outbound_calls] [decimal](19, 0)  NULL,
	[direct_outbound_talk_time_s] [decimal](19, 0) NULL,
	[direct_incoming_calls] [decimal](19, 0)  NULL,
	[direct_incoming_calls_talk_time_s] [decimal](19, 0) NULL,
	[admin_time_s] [decimal](19, 0) NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_agent_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_agent_insert_date]  DEFAULT (getdate())
 CONSTRAINT [PK_fact_agent] PRIMARY KEY CLUSTERED 
(
	
	[date_id] ASC,
	[interval_id] ASC,
	[acd_login_id] ASC
)
)
GO

ALTER TABLE [mart].[fact_agent]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_dim_date_UTC] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_agent] CHECK CONSTRAINT [FK_fact_agent_dim_date_UTC]
GO
ALTER TABLE [mart].[fact_agent]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_dim_interval_UTC] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_agent] CHECK CONSTRAINT [FK_fact_agent_dim_interval_UTC]
GO
ALTER TABLE [mart].[fact_agent]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_dim_agent] FOREIGN KEY([acd_login_id])
REFERENCES [mart].[dim_acd_login] ([acd_login_id])
GO
ALTER TABLE [mart].[fact_agent] CHECK CONSTRAINT [FK_fact_agent_dim_agent]
GO



GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (409,'8.1.409') 
