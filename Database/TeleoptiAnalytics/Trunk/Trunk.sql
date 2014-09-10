
----------------  
--Name: David Jonsson
--Date: 2014-09-10
--Desc: Bug #27933 - setting preference_typ_id in code
----------------
DROP TABLE [stage].[stg_schedule_preference]
GO
CREATE TABLE [stage].[stg_schedule_preference](
            [person_restriction_code] [uniqueidentifier] NOT NULL,
            [restriction_date] [datetime] NOT NULL,
            [person_code] [uniqueidentifier] NOT NULL,
            [scenario_code] [uniqueidentifier] NOT NULL,
            [shift_category_code] [uniqueidentifier] NULL,
            [day_off_code] [uniqueidentifier] NULL,
            [day_off_name] [nvarchar](50) NULL,
            [day_off_shortname] [nvarchar](25) NULL,
            [preference_type_id] tinyint NOT NULL,
            [preference_fulfilled] [int] NULL,
            [preference_unfulfilled] [int] NULL,
            [business_unit_code] [uniqueidentifier] NOT NULL,
            [datasource_id] [smallint] NULL CONSTRAINT [DF_stg_schedule_preference_datasource_id]  DEFAULT ((1)),
            [insert_date] [smalldatetime] NULL CONSTRAINT [DF_stg_schedule_preference_insert_date]  DEFAULT (getdate()),
            [update_date] [smalldatetime] NULL CONSTRAINT [DF_stg_schedule_preference_update_date]  DEFAULT (getdate()),
            [datasource_update_date] [smalldatetime] NULL CONSTRAINT [DF_stg_schedule_preference_datasource_update_date]  DEFAULT (getdate()),
            [activity_code] [uniqueidentifier] NULL,
            [absence_code] [uniqueidentifier] NULL,
            [must_have] [int] NULL,
CONSTRAINT [PK_stg_schedule_preference] PRIMARY KEY CLUSTERED 
(
            [person_restriction_code] ASC,
            [scenario_code] ASC
)
) ON [PRIMARY]

GO


