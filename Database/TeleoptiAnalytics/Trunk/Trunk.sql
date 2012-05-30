CREATE TABLE [mart].[dim_quality_quest](
                             [id] [int] NOT NULL,
                             [name] [nvarchar](200) NULL,
                             [question_code] [int] NULL,
                             [quality_type] [int] NULL,
                             [insert_date] [smalldatetime] NULL,
                             [update_date] [smalldatetime] NULL,
                             [datasource] [nvarchar](50) NULL
)

ALTER TABLE [mart].[dim_quality_quest] ADD CONSTRAINT [PK_dim_quality_quest] PRIMARY KEY CLUSTERED 
(
                             [id] ASC
)

GO

CREATE TABLE [mart].[dim_quality_type](
                             [type_id] [numeric](18, 0) NOT NULL,
                             [value] [nvarchar](50) NOT NULL,
                             [calculate_total] [nvarchar](50) NULL
)
ALTER TABLE [mart].[dim_quality_type] ADD CONSTRAINT [PK_dim_quality_type] PRIMARY KEY CLUSTERED 
(
                             [type_id] ASC
)


GO

CREATE TABLE mart.fact_quality_percentage
	(
	date_id int NOT NULL,
	qm_login_id int NOT NULL,
	quest_id int NOT NULL,
	quest_type_id int NOT NULL,
	percentage decimal(18, 0) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE mart.fact_quality_percentage ADD CONSTRAINT
	PK_fact_quality_percentage PRIMARY KEY CLUSTERED 
	(
	date_id,
	qm_login_id,
	quest_id,
	quest_type_id
	)

GO

CREATE TABLE [mart].[fact_quality_grades](
                             [date_id] [int] NOT NULL,
                             [qm_login_id] [int] NOT NULL,
                             [quest_id] [int] NOT NULL,
                             [quest_type_id] [int] NOT NULL,
                             [grades] [decimal](18, 0) NULL
)
GO
ALTER TABLE mart.fact_quality_grades ADD CONSTRAINT
	PK_fact_quality_grades PRIMARY KEY CLUSTERED 
	(
	date_id,
	qm_login_id,
	quest_id,
	quest_type_id
	) 

GO


CREATE TABLE [mart].[fact_quality_points](
                             [date_id] [int] NOT NULL,
                             [qm_login_id] [int] NOT NULL,
                             [quest_id] [int] NOT NULL,
                             [quest_type_id] [int] NOT NULL,
                             [points] [int] NULL
)

GO
ALTER TABLE mart.fact_quality_points ADD CONSTRAINT
	PK_fact_quality_points PRIMARY KEY CLUSTERED 
	(
	date_id,
	qm_login_id,
	quest_id,
	quest_type_id
	)

GO






