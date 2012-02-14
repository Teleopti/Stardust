/* 
Trunk initiated: 
2009-11-11 
16:09
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: JN
--Date: 2009-11-11  
--Desc: Add is_delete columns for skill and workload  
----------------  





----------------  
--Name: Henry Greijer
--Date: 2009-11-27  
--Desc: Preference stuff  
----------------  

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_stg_schedule_preference_datasource_id]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_schedule_preference] DROP CONSTRAINT [DF_stg_schedule_preference_datasource_id]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_stg_schedule_preference_insert_date]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_schedule_preference] DROP CONSTRAINT [DF_stg_schedule_preference_insert_date]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_stg_schedule_preference_update_date]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_schedule_preference] DROP CONSTRAINT [DF_stg_schedule_preference_update_date]
END

GO

/****** Object:  Table [stage].[stg_schedule_preference]    Script Date: 11/24/2009 15:53:01 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_schedule_preference]') AND type in (N'U'))
DROP TABLE [stage].[stg_schedule_preference]
GO


/****** Object:  Table [stage].[stg_schedule_preference]    Script Date: 11/24/2009 15:28:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [stage].[stg_schedule_preference](
	[person_restriction_code] [uniqueidentifier] NOT NULL,
	[restriction_date] [datetime] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[interval_id] [int] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[shift_category_code] [uniqueidentifier] NULL,
	[day_off_code] [uniqueidentifier] NULL,
	[StartTimeMinimum] [bigint] NULL,
	[StartTimeMaximum] [bigint] NULL,
	[endTimeMinimum] [bigint] NULL,
	[endTimeMaximum] [bigint] NULL,
	[WorkTimeMinimum] [bigint] NULL,
	[WorkTimeMaximum] [bigint] NULL,
	[preference_accepted] [int] NULL,
	[preference_declined] [int] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] ,
	[insert_date] [smalldatetime] ,
	[update_date] [smalldatetime] ,
	[datasource_update_date] [smalldatetime] ,
 CONSTRAINT [PK_stg_schedule_preference] PRIMARY KEY CLUSTERED 
(
	[person_restriction_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]

GO

ALTER TABLE [stage].[stg_schedule_preference] ADD  CONSTRAINT [DF_stg_schedule_preference_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO

ALTER TABLE [stage].[stg_schedule_preference] ADD  CONSTRAINT [DF_stg_schedule_preference_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO

ALTER TABLE [stage].[stg_schedule_preference] ADD  CONSTRAINT [DF_stg_schedule_preference_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

ALTER TABLE [stage].[stg_schedule_preference] ADD  CONSTRAINT [DF_stg_schedule_preference_datasource_update_date]  DEFAULT (getdate()) FOR [datasource_update_date]

GO






----------------  
--Name: Henry Greijer
--Date: 2009-11-30 
--Desc: Preference Type stuff  
----------------  


CREATE TABLE mart.Tmp_dim_preference_type
	(
	preference_type_id int NOT NULL,
	preference_type_name nvarchar(50) NOT NULL,
	resource_key nvarchar(100) NOT NULL
	)  ON MART
GO
IF EXISTS(SELECT * FROM mart.dim_preference_type)
	 EXEC('INSERT INTO mart.Tmp_dim_preference_type (preference_type_id, preference_type_name, resource_key)
		SELECT preference_type_id, preference_type_name, CONVERT(nvarchar(100), term_id) FROM mart.dim_preference_type WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE mart.fact_schedule_preference
	DROP CONSTRAINT FK_fact_schedule_preference_dim_preference_type
GO
DROP TABLE mart.dim_preference_type
GO
EXECUTE sp_rename N'mart.Tmp_dim_preference_type', N'dim_preference_type', 'OBJECT' 
GO
ALTER TABLE mart.dim_preference_type ADD CONSTRAINT
	PK_dim_preference_type PRIMARY KEY CLUSTERED 
	(
	preference_type_id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO
ALTER TABLE mart.fact_schedule_preference ADD CONSTRAINT
	FK_fact_schedule_preference_dim_preference_type FOREIGN KEY
	(
	preference_type_id
	) REFERENCES mart.dim_preference_type
	(
	preference_type_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (177,'7.0.177') 
