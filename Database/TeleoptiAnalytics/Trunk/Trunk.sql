--------------
--New table to store custom Agg views
--------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_crossdatabaseview_custom]') AND type in (N'U'))
BEGIN
	CREATE TABLE [mart].[sys_crossdatabaseview_custom](
		[view_id] [int] IDENTITY(1,1) NOT NULL,
		[view_name] [varchar](100) NOT NULL,
		[view_definition] [varchar](4000) NOT NULL,
		[target_id] [int] NOT NULL,
	 CONSTRAINT [PK_sys_crossdatabaseview_custom] PRIMARY KEY CLUSTERED 
	(
		[view_id] ASC
	)
	)
	ALTER TABLE [mart].[sys_crossdatabaseview_custom]  WITH NOCHECK ADD  CONSTRAINT [FK_sys_crossdatabaseview_custom_sys_crossdatabaseview_target] FOREIGN KEY([target_id])
	REFERENCES [mart].[sys_crossdatabaseview_target] ([target_id])
	ALTER TABLE [mart].[sys_crossdatabaseview_custom] CHECK CONSTRAINT [FK_sys_crossdatabaseview_custom_sys_crossdatabaseview_target]
END
GO

----------------  
--Name: Karin Jeppsson
--Date: 2014-10-13
--Desc: Bug #30845 Fix of primary key in fact_schedule_convert
----------------
TRUNCATE TABLE [mart].[fact_schedule_convert]
GO
ALTER TABLE [mart].[fact_schedule_convert] DROP CONSTRAINT [PK_fact_schedule_convert]
GO
ALTER TABLE [mart].[fact_schedule_convert] ADD  CONSTRAINT [PK_fact_schedule_convert] PRIMARY KEY CLUSTERED 
(
	[shift_startdate_local_id] ASC,
	[scenario_id] ASC,
	[person_id] ASC,
	[schedule_date_id] ASC,
	[interval_id] ASC,
	[activity_starttime] ASC
)
GO
