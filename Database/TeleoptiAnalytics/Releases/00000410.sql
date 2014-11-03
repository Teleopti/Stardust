----------------  
--Name: Karin
--Date: 2014-10-24
--Desc: PBI #30787 - Change of detail type so that it matches Agg db
----------------
UPDATE [mart].[sys_datasource_detail_type]
SET [detail_desc]='Queue'
WHERE [detail_id]=1
GO
UPDATE [mart].[sys_datasource_detail_type]
SET [detail_desc]='Agent'
WHERE [detail_id]=2
GO

----------------  
--Name: Karin
--Date: 2014-10-28
--Desc: PBI #30789 - Redesign fact_agent_queue
----------------
IF NOT EXISTS(SELECT 1 FROM [mart].[sys_datasource_detail_type] WHERE detail_id=3)
BEGIN
	INSERT [mart].[sys_datasource_detail_type]
	SELECT 3,'Agent Queue'
END 
GO
ALTER TABLE [mart].[fact_agent_queue] DROP CONSTRAINT FK_fact_agent_queue_dim_agent
ALTER TABLE [mart].[fact_agent_queue] DROP CONSTRAINT FK_fact_agent_queue_dim_date_Local
ALTER TABLE [mart].[fact_agent_queue] DROP CONSTRAINT FK_fact_agent_queue_dim_date_UTC
ALTER TABLE [mart].[fact_agent_queue] DROP CONSTRAINT FK_fact_agent_queue_dim_interval_Local
ALTER TABLE [mart].[fact_agent_queue] DROP CONSTRAINT FK_fact_agent_queue_dim_interval_UTC
ALTER TABLE [mart].[fact_agent_queue] DROP CONSTRAINT FK_fact_agent_queue_dim_queue
GO
ALTER TABLE [mart].[fact_agent_queue] DROP CONSTRAINT [DF_fact_agent_queue_datasource_id]
ALTER TABLE [mart].[fact_agent_queue] DROP CONSTRAINT [DF_fact_agent_queue_insert_date]
ALTER TABLE [mart].[fact_agent_queue] DROP CONSTRAINT [DF_fact_agent_queue_update_date]
GO
EXEC dbo.sp_rename @objname = N'[mart].[fact_agent_queue]', @newname = N'fact_agent_queue_old', @objtype = N'OBJECT'
GO
EXEC sp_rename N'[mart].[fact_agent_queue_old].[PK_fact_agent_queue]', N'PK_fact_agent_queue_old', N'INDEX'
GO
CREATE TABLE [mart].[fact_agent_queue](
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[queue_id] [int] NOT NULL,
	[acd_login_id] [int] NOT NULL,
	[talk_time_s] [decimal](19,0) NULL,
	[after_call_work_time_s] [decimal](19,0) NULL,
	[answered_calls] [int] NULL,
	[transfered_calls] [int] NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_agent_queue_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_agent_queue_insert_date]  DEFAULT (getdate())
 CONSTRAINT [PK_fact_agent_queue] PRIMARY KEY CLUSTERED 
(
	[queue_id] ASC,
	[date_id] ASC,
	[interval_id] ASC,
	[acd_login_id] ASC
))
GO

ALTER TABLE [mart].[fact_agent_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_queue_dim_date_UTC] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_agent_queue] CHECK CONSTRAINT [FK_fact_agent_queue_dim_date_UTC]
GO
ALTER TABLE [mart].[fact_agent_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_queue_dim_interval_UTC] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_agent_queue] CHECK CONSTRAINT [FK_fact_agent_queue_dim_interval_UTC]
GO
ALTER TABLE [mart].[fact_agent_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_queue_dim_agent] FOREIGN KEY([acd_login_id])
REFERENCES [mart].[dim_acd_login] ([acd_login_id])
GO
ALTER TABLE [mart].[fact_agent_queue] CHECK CONSTRAINT [FK_fact_agent_queue_dim_agent]
GO
ALTER TABLE [mart].[fact_agent_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_queue_dim_queue] FOREIGN KEY([queue_id])
REFERENCES [mart].[dim_queue] ([queue_id])
GO
ALTER TABLE [mart].[fact_agent_queue] CHECK CONSTRAINT [FK_fact_agent_queue_dim_queue]
GO

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (410,'8.1.410') 
