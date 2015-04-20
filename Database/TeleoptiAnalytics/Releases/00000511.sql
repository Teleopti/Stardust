CREATE TABLE [stage].[stg_schedule_changed_servicebus](
	[schedule_date_local] [smalldatetime] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[datasource_update_date] [smalldatetime] NOT NULL)
GO

ALTER TABLE [stage].[stg_schedule_changed_servicebus] ADD CONSTRAINT [PK_stg_schedule_changed_servicebus] PRIMARY KEY CLUSTERED 
(
	[schedule_date_local] ASC,
	[person_code] ASC,
	[scenario_code] ASC
)

