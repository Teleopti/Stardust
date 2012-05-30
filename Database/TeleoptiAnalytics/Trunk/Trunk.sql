------------------
--dim_quality_quest_type
------------------
CREATE TABLE [mart].[dim_quality_quest_type](
	[quality_quest_type_id] [int] IDENTITY(1,1) NOT NULL,
	[quality_quest_type_name] [nvarchar](200) NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_id] [smallint] NOT NULL
)

ALTER TABLE [mart].[dim_quality_quest_type]
ADD CONSTRAINT [PK_dim_quality_quest_type] PRIMARY KEY CLUSTERED 
(
	[quality_quest_type_id] ASC
)
ALTER TABLE [mart].[dim_quality_quest_type] ADD  CONSTRAINT [DF_dim_quality_quest_type_name] DEFAULT (N'Not Defined') FOR [quality_quest_type_name]
ALTER TABLE [mart].[dim_quality_quest_type] ADD  CONSTRAINT [DF_dim_quality_quest_type_datasource] DEFAULT ((-1)) FOR [datasource_id]
ALTER TABLE [mart].[dim_quality_quest_type] ADD  CONSTRAINT [DF_dim_quality_quest_type_insert] DEFAULT (getdate()) FOR [insert_date]
ALTER TABLE [mart].[dim_quality_quest_type] ADD  CONSTRAINT [DF_dim_quality_quest_type_update] DEFAULT (getdate()) FOR [update_date]
GO

------------------
--dim_quality_quest
------------------
CREATE TABLE [mart].[dim_quality_quest](
	[quality_quest_id] [int] IDENTITY(1,1) NOT NULL,
	[quality_quest_agg_id] [int] NULL,
	[quality_quest_original_id] [int] NULL,
	[quality_quest_name] [nvarchar](200) NOT NULL,
	[quality_quest_code] [int] NULL,
	[quality_quest_type_id] [int] NOT NULL,
	[log_object_name] [nvarchar](100) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL
)

ALTER TABLE [mart].[dim_quality_quest]
ADD CONSTRAINT [PK_dim_quality_quest] PRIMARY KEY CLUSTERED 
(
	[quality_quest_id] ASC
)

ALTER TABLE [mart].[dim_quality_quest] WITH CHECK ADD  CONSTRAINT [FK_dim_quality_quest_dim_quality_quest_type] FOREIGN KEY([quality_quest_type_id])
REFERENCES [mart].[dim_quality_quest_type] ([quality_quest_type_id])

ALTER TABLE [mart].[dim_quality_quest] ADD  CONSTRAINT [DF_dim_quality_quest_code] DEFAULT (N'Not Defined') FOR [quality_quest_name]
ALTER TABLE [mart].[dim_quality_quest] ADD  CONSTRAINT [DF_dim_quality_quest_type] DEFAULT (-1) FOR [quality_quest_type_id]
ALTER TABLE [mart].[dim_quality_quest] ADD  CONSTRAINT [DF_dim_quality_quest_datasource] DEFAULT ((-1)) FOR [datasource_id]
ALTER TABLE [mart].[dim_quality_quest] ADD  CONSTRAINT [DF_dim_quality_quest_insert] DEFAULT (getdate()) FOR [insert_date]
ALTER TABLE [mart].[dim_quality_quest] ADD  CONSTRAINT [DF_dim_quality_quest_update] DEFAULT (getdate()) FOR [update_date]

GO

------------------
--fact_quality_percentage
------------------
CREATE TABLE mart.fact_quality_percentage
	(
	date_id int NOT NULL,
	qm_login_id int NOT NULL,
	quality_quest_id int NOT NULL,
	percentage decimal(18, 0) NULL
	)

ALTER TABLE mart.fact_quality_percentage
ADD CONSTRAINT PK_fact_quality_percentage PRIMARY KEY CLUSTERED 
	(
	date_id,
	qm_login_id,
	quality_quest_id
	)
	
ALTER TABLE [mart].[fact_quality_percentage]  WITH CHECK ADD  CONSTRAINT [FK_fact_quality_percentage_dim_quality_quest] FOREIGN KEY([quality_quest_id])
REFERENCES [mart].[dim_quality_quest] ([quality_quest_id])

ALTER TABLE [mart].[fact_quality_percentage]  WITH CHECK ADD  CONSTRAINT [FK_fact_quality_percentage_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])

ALTER TABLE [mart].[fact_quality_percentage]  WITH CHECK ADD  CONSTRAINT [FK_fact_quality_percentage_dim_acd_login] FOREIGN KEY([qm_login_id])
REFERENCES [mart].[dim_acd_login] ([acd_login_id])
GO

------------------
--fact_quality_grades
------------------
CREATE TABLE [mart].[fact_quality_grades](
	[date_id] [int] NOT NULL,
	[qm_login_id] [int] NOT NULL,
	[quality_quest_id] [int] NOT NULL,
	[grades] [decimal](18, 0) NULL
)

ALTER TABLE mart.fact_quality_grades
ADD CONSTRAINT PK_fact_quality_grades PRIMARY KEY CLUSTERED 
	(
	date_id,
	qm_login_id,
	quality_quest_id
	) 

ALTER TABLE [mart].[fact_quality_grades]  WITH CHECK ADD  CONSTRAINT [FK_fact_quality_grades_dim_agent] FOREIGN KEY([qm_login_id])
REFERENCES [mart].[dim_acd_login] ([acd_login_id])

ALTER TABLE [mart].[fact_quality_grades]  WITH CHECK ADD  CONSTRAINT [FK_fact_quality_grades_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])

ALTER TABLE [mart].[fact_quality_grades]  WITH CHECK ADD  CONSTRAINT [FK_fact_quality_grades_dim_quality_quest] FOREIGN KEY([quality_quest_id])
REFERENCES [mart].[dim_quality_quest] ([quality_quest_id])
GO

------------------
--fact_quality_points
------------------
CREATE TABLE [mart].[fact_quality_points](
	 [date_id] [int] NOT NULL,
	 [qm_login_id] [int] NOT NULL,
	 [quality_quest_id] [int] NOT NULL,
	 [points] [int] NULL
)

ALTER TABLE mart.fact_quality_points ADD CONSTRAINT
	PK_fact_quality_points PRIMARY KEY CLUSTERED 
	(
	date_id,
	qm_login_id,
	quality_quest_id
	)
	
ALTER TABLE [mart].[fact_quality_points]  WITH CHECK ADD  CONSTRAINT [FK_fact_quality_points_dim_agent] FOREIGN KEY([qm_login_id])
REFERENCES [mart].[dim_acd_login] ([acd_login_id])

ALTER TABLE [mart].[fact_quality_points]  WITH CHECK ADD  CONSTRAINT [FK_fact_quality_points_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])

ALTER TABLE [mart].[fact_quality_points]  WITH CHECK ADD  CONSTRAINT [FK_fact_quality_points_dim_quality_quest] FOREIGN KEY([quality_quest_id])
REFERENCES [mart].[dim_quality_quest] ([quality_quest_id])
GO

--Agg Tables used as agent_info + agent_logg
CREATE TABLE dbo.quality_info(
	[quality_id] [int] IDENTITY(1,1) NOT NULL,
	[quality_name] nvarchar(200) NOT NULL,
	[quality_type] nvarchar(200) NOT NULL,
	[original_id] int NOT NULL,
	
)

ALTER TABLE dbo.quality_info ADD CONSTRAINT
	PK_quality_info PRIMARY KEY CLUSTERED 
	(
	quality_id ASC
	)
GO

CREATE TABLE dbo.quality_logg(
	[quality_id] [int] NOT NULL,
	[date_from] [smalldatetime] NOT NULL,
	[interval] [int] NOT NULL,
	[agent_id] [int] NOT NULL,
	[quest_type_id] [int] NULL,
	[score] [real] NULL
)

ALTER TABLE dbo.quality_logg ADD CONSTRAINT
[PK_quality_logg] PRIMARY KEY CLUSTERED 
(
	[date_from] ASC,
	[agent_id] ASC,
	[quality_id] ASC,
	[interval] ASC
)

ALTER TABLE [dbo].[quality_logg]  WITH CHECK ADD  CONSTRAINT [FK_quality_logg_quality_info] FOREIGN KEY([quality_id])
REFERENCES [dbo].[quality_info] ([quality_id])
GO