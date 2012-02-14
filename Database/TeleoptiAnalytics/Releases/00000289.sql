/* 
Trunk initiated: 
2010-06-17 
15:14
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: jonas n
--Date: 2010-06-21  
--Desc: Add table to keep track of pm users  
----------------
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [mart].[pm_user](
	[user_name] [nvarchar](256) NOT NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL
) ON [PRIMARY]

GO

ALTER TABLE [mart].[pm_user] ADD  CONSTRAINT [DF_pm_user_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO

ALTER TABLE [mart].[pm_user] ADD  CONSTRAINT [DF_pm_user_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

----------------
--Name: Robin K
--Date: 2010-07-05  
--Desc: Added a bunch of new columns to the workload to be able to do calculations of queue data in reports
----------------
CREATE TABLE [mart].[dim_workload_new](
	[workload_id] [int] IDENTITY(1,1) NOT NULL,
	[workload_code] [uniqueidentifier] NULL,
	[workload_name] [nvarchar](100) NOT NULL,
	[skill_id] [int] NULL,
	[skill_code] [uniqueidentifier] NULL,
	[skill_name] [nvarchar](100) NOT NULL,
	[time_zone_id] [int] NOT NULL,
	[forecast_method_code] [uniqueidentifier] NULL,
	[forecast_method_name] [nvarchar](100) NOT NULL,
	[percentage_offered] [float] NOT NULL,
	[percentage_overflow_in] [float] NOT NULL,
	[percentage_overflow_out] [float] NOT NULL,
	[percentage_abandoned] [float] NOT NULL,
	[percentage_abandoned_short] [float] NOT NULL,
	[percentage_abandoned_within_service_level] [float] NOT NULL,
	[percentage_abandoned_after_service_level] [float] NOT NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [datetime] NOT NULL,
	[is_deleted] [bit] NOT NULL,
 CONSTRAINT [PK2_dim_workload] PRIMARY KEY CLUSTERED 
(
	[workload_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MART]
) ON [MART]

GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF2_dim_workload_workload_name]  DEFAULT ('Not Defined') FOR [workload_name]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF2_dim_workload_skill_id]  DEFAULT ((-1)) FOR [skill_id]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF2_dim_workload_skill_name]  DEFAULT ('Not Defined') FOR [skill_name]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF2_dim_workload_time_zone_id]  DEFAULT ((-1)) FOR [time_zone_id]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF2_dim_workload_forecast_method_name]  DEFAULT ('Not Defined') FOR [forecast_method_name]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF_dim_workload_percentage_offered]  DEFAULT ((1)) FOR [percentage_offered]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF_dim_workload_percentage_overflow_in]  DEFAULT ((1)) FOR [percentage_overflow_in]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF_dim_workload_percentage_overflow_out]  DEFAULT ((-1)) FOR [percentage_overflow_out]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF_dim_workload_percentage_abandoned]  DEFAULT ((-1)) FOR [percentage_abandoned]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF_dim_workload_percentage_abandoned_short]  DEFAULT ((0)) FOR [percentage_abandoned_short]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF_dim_workload_abandoned_within_service_level]  DEFAULT ((1)) FOR [percentage_abandoned_within_service_level]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF_dim_workload_abandoned_after_service_level]  DEFAULT ((1)) FOR [percentage_abandoned_after_service_level]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF2_dim_workload_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF2_dim_workload_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF2_dim_workload_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF2_dim_workload_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF2_dim_workload_datasource_update_date]  DEFAULT ('1900-01-01') FOR [datasource_update_date]
GO

ALTER TABLE [mart].[dim_workload_new] ADD  CONSTRAINT [DF2_dim_workload_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
GO

SET IDENTITY_INSERT mart.dim_workload_new ON
GO

IF EXISTS(SELECT * FROM mart.dim_workload)
EXEC('INSERT INTO mart.dim_workload_new (workload_id,workload_code,workload_name,skill_id,skill_code,skill_name,time_zone_id,forecast_method_code,forecast_method_name,percentage_offered,percentage_overflow_in,percentage_overflow_out,percentage_abandoned,percentage_abandoned_short,percentage_abandoned_within_service_level,percentage_abandoned_after_service_level,business_unit_id,datasource_id,insert_date,update_date,datasource_update_date,is_deleted)
	SELECT workload_id,workload_code,workload_name,skill_id,skill_code,skill_name,time_zone_id,forecast_method_code,forecast_method_name,1,1,-1,-1,0,1,1,business_unit_id,datasource_id,insert_date,update_date,datasource_update_date,is_deleted FROM mart.dim_workload WITH (HOLDLOCK TABLOCKX)')
GO

SET IDENTITY_INSERT mart.dim_workload_new OFF
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_dim_workload_workload_name]') AND type = 'D')
BEGIN
ALTER TABLE [mart].[dim_workload] DROP CONSTRAINT [DF_dim_workload_workload_name]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_dim_workload_skill_id]') AND type = 'D')
BEGIN
ALTER TABLE [mart].[dim_workload] DROP CONSTRAINT [DF_dim_workload_skill_id]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_dim_workload_skill_name]') AND type = 'D')
BEGIN
ALTER TABLE [mart].[dim_workload] DROP CONSTRAINT [DF_dim_workload_skill_name]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_dim_workload_time_zone_id]') AND type = 'D')
BEGIN
ALTER TABLE [mart].[dim_workload] DROP CONSTRAINT [DF_dim_workload_time_zone_id]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_dim_workload_forecast_method_name]') AND type = 'D')
BEGIN
ALTER TABLE [mart].[dim_workload] DROP CONSTRAINT [DF_dim_workload_forecast_method_name]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_dim_workload_business_unit_id]') AND type = 'D')
BEGIN
ALTER TABLE [mart].[dim_workload] DROP CONSTRAINT [DF_dim_workload_business_unit_id]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_dim_workload_datasource_id]') AND type = 'D')
BEGIN
ALTER TABLE [mart].[dim_workload] DROP CONSTRAINT [DF_dim_workload_datasource_id]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_dim_workload_insert_date]') AND type = 'D')
BEGIN
ALTER TABLE [mart].[dim_workload] DROP CONSTRAINT [DF_dim_workload_insert_date]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_dim_workload_update_date]') AND type = 'D')
BEGIN
ALTER TABLE [mart].[dim_workload] DROP CONSTRAINT [DF_dim_workload_update_date]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_dim_workload_datasource_update_date]') AND type = 'D')
BEGIN
ALTER TABLE [mart].[dim_workload] DROP CONSTRAINT [DF_dim_workload_datasource_update_date]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_dim_workload_is_deleted]') AND type = 'D')
BEGIN
ALTER TABLE [mart].[dim_workload] DROP CONSTRAINT [DF_dim_workload_is_deleted]
END

GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_bridge_queue_workload_dim_workload]') AND parent_object_id = OBJECT_ID(N'[mart].[bridge_queue_workload]'))
ALTER TABLE [mart].[bridge_queue_workload] DROP CONSTRAINT [FK_bridge_queue_workload_dim_workload]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_forecast_workload_dim_workload]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_forecast_workload]'))
ALTER TABLE [mart].[fact_forecast_workload] DROP CONSTRAINT [FK_fact_forecast_workload_dim_workload]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_workload]') AND type in (N'U'))
DROP TABLE [mart].[dim_workload]
GO

sp_rename N'[mart].[dim_workload_new]',N'dim_workload','OBJECT';
GO

sp_rename N'[mart].[PK2_dim_workload]',N'PK_dim_workload','OBJECT';
GO

sp_rename N'[mart].[DF2_dim_workload_workload_name]',N'DF_dim_workload_workload_name','OBJECT';
GO

sp_rename N'[mart].[DF2_dim_workload_skill_id]',N'DF_dim_workload_skill_id','OBJECT';
GO

sp_rename N'[mart].[DF2_dim_workload_skill_name]',N'DF_dim_workload_skill_name','OBJECT';
GO

sp_rename N'[mart].[DF2_dim_workload_time_zone_id]',N'DF_dim_workload_time_zone_id','OBJECT';
GO

sp_rename N'[mart].[DF2_dim_workload_forecast_method_name]',N'DF_dim_workload_forecast_method_name','OBJECT';
GO

sp_rename N'[mart].[DF2_dim_workload_datasource_update_date]',N'DF_dim_workload_datasource_update_date','OBJECT';
GO

sp_rename N'[mart].[DF2_dim_workload_business_unit_id]',N'DF_dim_workload_business_unit_id','OBJECT';
GO

sp_rename N'[mart].[DF2_dim_workload_datasource_id]',N'DF_dim_workload_datasource_id','OBJECT';
GO

sp_rename N'[mart].[DF2_dim_workload_insert_date]',N'DF_dim_workload_insert_date','OBJECT';
GO

sp_rename N'[mart].[DF2_dim_workload_update_date]',N'DF_dim_workload_update_date','OBJECT';
GO

sp_rename N'[mart].[DF2_dim_workload_is_deleted]',N'DF_dim_workload_is_deleted','OBJECT';
GO

ALTER TABLE [mart].[bridge_queue_workload]  WITH CHECK ADD  CONSTRAINT [FK_bridge_queue_workload_dim_workload] FOREIGN KEY([workload_id])
REFERENCES [mart].[dim_workload] ([workload_id])
GO

ALTER TABLE [mart].[bridge_queue_workload] CHECK CONSTRAINT [FK_bridge_queue_workload_dim_workload]
GO

ALTER TABLE [mart].[fact_forecast_workload]  WITH CHECK ADD  CONSTRAINT [FK_fact_forecast_workload_dim_workload] FOREIGN KEY([workload_id])
REFERENCES [mart].[dim_workload] ([workload_id])
GO

ALTER TABLE [mart].[fact_forecast_workload] CHECK CONSTRAINT [FK_fact_forecast_workload_dim_workload]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_stg_workload_skill_is_deleted]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_workload] DROP CONSTRAINT [DF_stg_workload_skill_is_deleted]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_stg_workload_is_deleted]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_workload] DROP CONSTRAINT [DF_stg_workload_is_deleted]
END

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_workload]') AND type in (N'U'))
DROP TABLE [stage].[stg_workload]
GO


CREATE TABLE [stage].[stg_workload](
	[workload_code] [uniqueidentifier] NOT NULL,
	[workload_name] [nvarchar](100) NOT NULL,
	[skill_code] [uniqueidentifier] NULL,
	[skill_name] [nvarchar](100) NOT NULL,
	[time_zone_code] [nvarchar](50) NOT NULL,
	[forecast_method_code] [uniqueidentifier] NULL,
	[forecast_method_name] [nvarchar](100) NOT NULL,
	[percentage_offered] [float] NOT NULL,
	[percentage_overflow_in] [float] NOT NULL,
	[percentage_overflow_out] [float] NOT NULL,
	[percentage_abandoned] [float] NOT NULL,
	[percentage_abandoned_short] [float] NOT NULL,
	[percentage_abandoned_within_service_level] [float] NOT NULL,
	[percentage_abandoned_after_service_level] [float] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [datetime] NOT NULL,
	[skill_is_deleted] [bit] NOT NULL,
	[is_deleted] [bit] NOT NULL,
 CONSTRAINT [PK_stg_workload] PRIMARY KEY CLUSTERED 
(
	[workload_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]

GO

ALTER TABLE [stage].[stg_workload] ADD  CONSTRAINT [DF_stg_workload_skill_is_deleted]  DEFAULT ((0)) FOR [skill_is_deleted]
GO

ALTER TABLE [stage].[stg_workload] ADD  CONSTRAINT [DF_stg_workload_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
GO

 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (289,'7.1.289') 
