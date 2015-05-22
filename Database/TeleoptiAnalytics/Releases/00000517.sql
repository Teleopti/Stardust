ALTER TABLE [mart].[etl_jobstep_execution] DROP CONSTRAINT [FK_etl_jobstep_execution_etl_job_execution]
ALTER TABLE [mart].[etl_jobstep_execution] DROP CONSTRAINT [FK_etl_jobstep_execution_etl_jobstep_error]
ALTER TABLE [mart].[etl_jobstep_execution] DROP CONSTRAINT [FK_etl_jobstep_execution_etl_jobstep_execution]
ALTER TABLE [mart].[etl_jobstep_execution] DROP CONSTRAINT [DF_etl_jobstep_execution_insert_date]
ALTER TABLE [mart].[etl_jobstep_execution] DROP CONSTRAINT [DF_etl_jobstep_execution_update_date]

EXEC sp_rename @objname = N'[mart].[etl_jobstep_execution]', @newname = N'etl_jobstep_execution_old', @objtype = N'OBJECT'
EXEC sp_rename @objname = N'[mart].[etl_jobstep_execution_old].[PK_etl_jobstep_execution]', @newname =  N'PK_etl_jobstep_execution_old', @objtype = N'INDEX'

CREATE TABLE [mart].[etl_jobstep_execution](
	[jobstep_execution_id] [int] IDENTITY(1,1) NOT NULL,
	[business_unit_code] [uniqueidentifier] NULL,
	[business_unit_name] [nvarchar](100) NULL,
	[duration_s] [int] NULL,
	[rows_affected] [int] NULL,
	[job_execution_id] [int] NULL,
	[jobstep_error_id] [int] NULL,
	[jobstep_id] [int] NULL,
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_etl_jobstep_execution_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_etl_jobstep_execution_update_date]  DEFAULT (getdate()),
)

--PK non-clustered
ALTER TABLE [mart].[etl_jobstep_execution] ADD  CONSTRAINT [PK_etl_jobstep_execution] PRIMARY KEY NONCLUSTERED 
(
	[jobstep_execution_id] ASC
)

--Clustered index
CREATE CLUSTERED INDEX [CIX_etl_jobstep_execution_JobExecutionError] ON [mart].[etl_jobstep_execution]
(
	[job_execution_id] ASC,
	[jobstep_error_id] ASC
)

--Non-clustered index
CREATE NONCLUSTERED INDEX [CIX_etl_jobstep_execution_ErrorId] ON [mart].[etl_jobstep_execution]
(
	[jobstep_error_id] ASC
)

--Get the data
SET IDENTITY_INSERT [mart].[etl_jobstep_execution] ON

INSERT INTO [mart].[etl_jobstep_execution] 
	(
	jobstep_execution_id,
	business_unit_code,
	business_unit_name,
	duration_s,
	rows_affected,
	job_execution_id,
	jobstep_error_id,
	jobstep_id,
	insert_date,
	update_date
	)
SELECT * FROM [mart].[etl_jobstep_execution_old]

SET IDENTITY_INSERT [mart].[etl_jobstep_execution] OFF


--re-Add FKs
ALTER TABLE [mart].[etl_jobstep_execution]  WITH NOCHECK ADD  CONSTRAINT [FK_etl_jobstep_execution_etl_job_execution] FOREIGN KEY([job_execution_id])
REFERENCES [mart].[etl_job_execution] ([job_execution_id])
ALTER TABLE [mart].[etl_jobstep_execution] CHECK CONSTRAINT [FK_etl_jobstep_execution_etl_job_execution]

ALTER TABLE [mart].[etl_jobstep_execution]  WITH NOCHECK ADD  CONSTRAINT [FK_etl_jobstep_execution_etl_jobstep_error] FOREIGN KEY([jobstep_error_id])
REFERENCES [mart].[etl_jobstep_error] ([jobstep_error_id])
ALTER TABLE [mart].[etl_jobstep_execution] CHECK CONSTRAINT [FK_etl_jobstep_execution_etl_jobstep_error]

ALTER TABLE [mart].[etl_jobstep_execution]  WITH NOCHECK ADD  CONSTRAINT [FK_etl_jobstep_execution_etl_jobstep_execution] FOREIGN KEY([jobstep_id])
REFERENCES [mart].[etl_jobstep] ([jobstep_id])
ALTER TABLE [mart].[etl_jobstep_execution] CHECK CONSTRAINT [FK_etl_jobstep_execution_etl_jobstep_execution]

--Clean up
DROP TABLE [mart].[etl_jobstep_execution_old]