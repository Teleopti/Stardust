----------------  
--Name: Karin
--Date: 2012-11-21
--Desc: new columns for PBI 19854 

---------------------STAGE.STG_ABSENCE-------------------------------------
--ADD A NEW COLUMN IN STAGE.STG_ABSENCE FOR SHORTNAME

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_absence]') AND type in (N'U'))
DROP TABLE [stage].[stg_absence]
GO

CREATE TABLE [stage].[stg_absence](
	[absence_code] [uniqueidentifier] NOT NULL,
	[absence_name] [nvarchar](100) NOT NULL,
	[absence_shortname] [nvarchar](25) NOT NULL,
	[display_color] [int] NOT NULL,
	[display_color_html] [char](7) NOT NULL,
	[in_contract_time] [bit] NULL,
	[in_paid_time] [bit] NULL,
	[in_work_time] [bit] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
	[is_deleted] [bit] NOT NULL
	)
	
 ALTER TABLE [stage].[stg_absence] ADD CONSTRAINT [PK_stg_absence] PRIMARY KEY CLUSTERED 
(
	[absence_code] ASC
)

GO

----------------------MART.DIM_ABSENCE-----------------------------------
--ADD A NEW COLUMN IN MART.DIM_ABSENCE FOR DISPLAY_COLOR_HTML
ALTER TABLE mart.dim_absence ADD display_color_html char(7)

GO

--ADD A NEW COLUMN IN MART.DIM_ABSENCE

ALTER TABLE mart.dim_absence ADD absence_shortname nvarchar(25)
GO

--SET DEFAULT VALUE  AND NOT NULL ON NEW COLUMNS

UPDATE mart.dim_absence
SET absence_shortname ='-1'
WHERE absence_shortname IS NULL

GO

UPDATE mart.dim_absence
SET display_color_html='#FFFFFF' --DEFAULT WHITE
WHERE display_color_html IS NULL

GO

ALTER TABLE mart.dim_absence ALTER COLUMN absence_shortname nvarchar(25) NOT NULL
GO
ALTER TABLE mart.dim_absence ALTER COLUMN display_color_html char(7) NOT NULL
GO



-------------------STAGE.STG_ACTIVITY------------------------------------------------------------------


--ADD A NEW COLUMN IN STAGE.STG_ACTIVITY FOR DISPLAY_COLOR_HTML

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_activity]') AND type in (N'U'))
DROP TABLE [stage].[stg_activity]
GO

CREATE TABLE [stage].[stg_activity](
           [activity_code] [uniqueidentifier] NOT NULL,
           [activity_name] [nvarchar](50) NOT NULL,
           [display_color] [int] NOT NULL,
           [display_color_html] [char](7) NOT NULL,    
           [in_ready_time] [bit] NOT NULL,
           [in_contract_time] [bit] NULL,
           [in_paid_time] [bit] NULL,
           [in_work_time] [bit] NULL,
           [business_unit_code] [uniqueidentifier] NOT NULL,
           [business_unit_name] [nvarchar](50) NOT NULL,
           [datasource_id] [smallint] NOT NULL,
           [insert_date] [smalldatetime] NOT NULL,
           [update_date] [smalldatetime] NOT NULL,
           [datasource_update_date] [smalldatetime] NOT NULL,
           [is_deleted] [bit] NOT NULL
)

ALTER TABLE stage.stg_activity ADD CONSTRAINT
PK_stg_activity PRIMARY KEY CLUSTERED 
(
           activity_code
)
GO

ALTER TABLE [stage].[stg_activity] ADD  CONSTRAINT [DF_stg_activity_in_ready_time]  DEFAULT ((0)) FOR [in_ready_time]
GO

ALTER TABLE [stage].[stg_activity] ADD  CONSTRAINT [DF_stg_activity_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO

ALTER TABLE [stage].[stg_activity] ADD  CONSTRAINT [DF_stg_activity_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

-----------------------dim_activity---------------------
--ADD A NEW COLUMN IN MART.DIM_ACTIVITY FOR DISPLAY_COLOR_HTML
ALTER TABLE mart.dim_activity ADD display_color_html char(7)
GO

--SET DEFAULT VALUE AND NOT NULL ON NEW COLUMN
UPDATE mart.dim_activity
SET display_color_html='#FFFFFF'
WHERE display_color_html IS NULL
GO

ALTER TABLE mart.dim_activity ALTER COLUMN display_color_html char(7) NOT NULL
GO

------------------STG.STAGE_DIM_DAY_OFF----------------------------------
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_day_off]') AND type in (N'U'))
DROP TABLE [stage].[stg_day_off]
GO

CREATE TABLE [stage].[stg_day_off](
           [day_off_code] [uniqueidentifier] NULL,
           [day_off_name] [nvarchar](50) NOT NULL,
           [day_off_shortname] [nvarchar](25) NOT NULL,          
           [display_color] [int] NOT NULL,
           [display_color_html] [char](7) NOT NULL,    
           [business_unit_code] [uniqueidentifier] NOT NULL,
           [datasource_id] [smallint] NOT NULL,
           [insert_date] [smalldatetime] NOT NULL,
           [update_date] [smalldatetime] NOT NULL,
           [datasource_update_date] [smalldatetime] NULL
)

ALTER TABLE stage.stg_day_off ADD CONSTRAINT
           PK_stg_day_off PRIMARY KEY CLUSTERED 
           (
           day_off_name,
           business_unit_code
           )
GO
--------------------DIM_DAY_OFF-------------------------

--ADD A NEW COLUMN IN MART.DIM_ABSENCE FOR DISPLAY_COLOR_HTML
ALTER TABLE mart.dim_day_off ADD display_color_html char(7)
GO
--ADD A NEW COLUMN IN MART.DIM_ABSENCE
ALTER TABLE mart.dim_day_off ADD day_off_shortname nvarchar(25)
GO
--SET DEFAULT VALUE  AND NOT NULL ON NEW COLUMNS

UPDATE mart.dim_day_off SET day_off_shortname ='-1' WHERE day_off_shortname IS NULL
GO

UPDATE mart.dim_day_off SET display_color_html='#FFFFFF' --DEFAULT WHITE
WHERE display_color_html IS NULL
GO

ALTER TABLE mart.dim_day_off ALTER COLUMN day_off_shortname nvarchar(25) NOT NULL
GO
ALTER TABLE mart.dim_day_off ALTER COLUMN display_color_html char(7) NOT NULL
GO