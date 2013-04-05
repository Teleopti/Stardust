----------------  
--Name: Karin and David
--Date: 2013-03-04
--Desc: #22446
---------------- 
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_job_delayed]') AND type in (N'U'))
BEGIN
	CREATE TABLE mart.etl_job_delayed
		(
			Id int identity (1,1) not null,
			stored_procedured nvarchar(300) not null,
			parameter_string nvarchar(1000) not null,
			insert_date smalldatetime not null,
			execute_date smalldatetime null
		)
	ALTER TABLE mart.etl_job_delayed ADD CONSTRAINT
		PK_etl_job_delayed PRIMARY KEY CLUSTERED 
		(
		Id
		)
	ALTER TABLE [mart].[etl_job_delayed] ADD  CONSTRAINT [DF_etl_job_delayed_insert_date]  DEFAULT (getdate()) FOR [insert_date]
END

----------------  
--Name: David
--Date: 2013-0-18
--Desc: bug #22699 - Support [Queue].[PeekMessage]
----------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[Queue].[Messages]') AND name = N'IX_Message_QueueId_Processed_ProcessingUntil')
CREATE NONCLUSTERED INDEX IX_Message_QueueId_Processed_ProcessingUntil
ON [Queue].[Messages] ([QueueId],[Processed],[ProcessingUntil])
INCLUDE ([CreatedAt],[ExpiresAt])
GO

----------------  
--Name: David
--Date: 2013-04-05
--Desc: bug #22969 - unwanted load of bridge_time_zone
----------------
IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'dim_time_zone delete data' AND jobstep_id=82)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(82,N'dim_time_zone delete data')
GO