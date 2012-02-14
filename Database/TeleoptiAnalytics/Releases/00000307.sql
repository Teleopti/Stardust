/* 
Trunk initiated: 
2010-11-08 
08:26
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Robin Karlsson / Tamas Balog
--Date: 2010-11-08
--Desc: Included person period code in the stage table for acd login person
----------------  
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_acd_login_insert_date]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_acd_login_person] DROP CONSTRAINT [DF_acd_login_insert_date]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_acd_login_update_date]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_acd_login_person] DROP CONSTRAINT [DF_acd_login_update_date]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_acd_login_datasource_update_date]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_acd_login_person] DROP CONSTRAINT [DF_acd_login_datasource_update_date]
END

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_acd_login_person]') AND type in (N'U'))
DROP TABLE [stage].[stg_acd_login_person]
GO

CREATE TABLE [stage].[stg_acd_login_person](
	[acd_login_code] [nvarchar](50) NULL,
	[person_code] [uniqueidentifier] NULL,
	[start_date] [smalldatetime] NULL,
	[end_date] [smalldatetime] NULL,
	[person_period_code] [uniqueidentifier] NULL,
	[log_object_datasource_id] [int] NULL,
	[log_object_name] [nvarchar](50) NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL
) ON [STAGE]

GO

ALTER TABLE [stage].[stg_acd_login_person] ADD  CONSTRAINT [DF_acd_login_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO

ALTER TABLE [stage].[stg_acd_login_person] ADD  CONSTRAINT [DF_acd_login_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

ALTER TABLE [stage].[stg_acd_login_person] ADD  CONSTRAINT [DF_acd_login_datasource_update_date]  DEFAULT (getdate()) FOR [datasource_update_date]
GO
 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (307,'7.1.307') 
