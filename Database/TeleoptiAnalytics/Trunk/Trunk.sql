IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule_deviation]') AND name = N'IX_fact_schedule_deviation_person_shiftstart_local')
CREATE NONCLUSTERED INDEX [IX_fact_schedule_deviation_person_shiftstart_local] ON [mart].[fact_schedule_deviation]
(
	[shift_startdate_local_id] ASC,
	[person_id] ASC
)
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_agent]') AND name = N'IX_fact_agent_ready_time_s')
CREATE NONCLUSTERED INDEX [IX_fact_agent_ready_time_s] ON [mart].[fact_agent]
(
	[acd_login_id] ASC,
	[date_id] ASC,
	[interval_id] ASC
)
INCLUDE ([ready_time_s])
GO

CREATE TABLE [mart].[etl_job_intraday_settings](
	[business_unit_id] [smallint] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[detail_id] [int] NOT NULL,
	[target_date] [smalldatetime] NOT NULL,
	[target_interval] [smallint] NOT NULL,
	[intervals_back] [smallint] NOT NULL,
	[is_utc] [bit] NOT NULL,
 CONSTRAINT [PK_etl_job_intraday_settings] PRIMARY KEY CLUSTERED 
(
	[business_unit_id] ASC,
	[datasource_id],
	[detail_id] ASC
)
)
GO

INSERT INTO [mart].[etl_job_intraday_settings]
SELECT -1,datasource_id, detail_id, target_date_local, target_interval_local, intervals_back,0
FROM [mart].[sys_datasource_detail]
GO

EXEC dbo.sp_rename @objname = N'[mart].[sys_datasource_detail_type]', @newname = N'etl_job_intraday_settings_type', @objtype = N'OBJECT'
GO

DROP TABLE mart.sys_datasource_detail
GO
INSERT INTO mart.etl_job_intraday_settings_type
SELECT 4,'Deviation'
GO
