----------------  
--Name: Karin & David
--Date: 2014-10-15
--Desc: PBI #30787 - redesign fact_queue
----------------
ALTER TABLE [mart].[fact_queue] DROP CONSTRAINT [FK_fact_queue_dim_date]
ALTER TABLE [mart].[fact_queue] DROP CONSTRAINT [FK_fact_queue_dim_interval]
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
	[abandoned_short_calls] [decimal](18, 0) NULL,
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
insert into [mart].[fact_queue]
select date_id, interval_id, queue_id, offered_calls, answered_calls, answered_calls_within_SL, abandoned_calls, abandoned_calls_within_SL, abandoned_short_calls, overflow_out_calls, overflow_in_calls, talk_time_s, after_call_work_s, handle_time_s, speed_of_answer_s, time_to_abandon_s, longest_delay_in_queue_answered_s, longest_delay_in_queue_abandoned_s, datasource_id, insert_date
from mart.fact_queue_old 
GO
ALTER TABLE [mart].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_date]
GO
ALTER TABLE [mart].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_interval]
GO
ALTER TABLE [mart].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_queue] FOREIGN KEY([queue_id])
REFERENCES [mart].[dim_queue] ([queue_id])
ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_queue]
GO


