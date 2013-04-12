

----------------  
--Name: David
--Date: 2013-0-18
--Desc: bug #22699 - Support [Queue].[PeekMessage]
----------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[Queue].[Messages]') AND name = N'IX_Message_QueueId_Processed_ProcessingUntil')
CREATE NONCLUSTERED INDEX IX_Message_QueueId_Processed_ProcessingUntil
ON [Queue].[Messages] ([QueueId],[Processed],[ProcessingUntil])
INCLUDE ([CreatedAt],[ExpiresAt])

----------------  
--Name: Ola & David
--Date: 2013-04-12
--Desc: PBI #22523 - Speed up ETL Intrady load
----------------
CREATE TABLE [stage].[stg_schedule_deleted](
	[schedule_date] [datetime] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NOT NULL
 CONSTRAINT [PK_stg_schedule_deleted] PRIMARY KEY CLUSTERED 
(
	[schedule_date] ASC,
	[person_code] ASC,
	[scenario_code] ASC
)
)

ALTER TABLE [stage].[stg_schedule_deleted] ADD  CONSTRAINT [DF_stg_schedule_deleted_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
ALTER TABLE [stage].[stg_schedule_deleted] ADD  CONSTRAINT [DF_stg_schedule_deleted_insert_date]  DEFAULT (getdate()) FOR [insert_date]
ALTER TABLE [stage].[stg_schedule_deleted] ADD  CONSTRAINT [DF_stg_schedule_deleted_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
