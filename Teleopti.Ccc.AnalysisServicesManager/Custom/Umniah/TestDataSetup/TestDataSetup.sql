IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'custom')
EXEC sys.sp_executesql N'CREATE SCHEMA [custom]'
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[fact_agent_customer_survey]') AND type in (N'U'))
CREATE TABLE [custom].[fact_agent_customer_survey](
	[survey_id] [int] NOT NULL,
	[local_date_id] [int] NOT NULL,
	[acd_login_id] [int] NOT NULL,
	[number_of_calls] [int] NULL,
	[score] [int] NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_agent_customer_survey_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_agent_customer_survey_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_fact_agent_customer_survey] PRIMARY KEY CLUSTERED 
(
	[survey_id] ASC
)
)  ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[fact_agent_evaluation]') AND type in (N'U'))
CREATE TABLE [custom].[fact_agent_evaluation](
	[evaluation_id] [int] NOT NULL,
	[local_date_id] [int] NULL,
	[acd_login_id] [int] NULL,
	[number_of_evaluations] [int] NULL,
	[quality_score_sum] [int] NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_agent_evaluation_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_agent_evaluation_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_fact_agent_evaluation] PRIMARY KEY CLUSTERED 
(
	[evaluation_id] ASC
)
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[custom].[dim_survey_score]'))
DROP VIEW [custom].[dim_survey_score]
GO

CREATE  VIEW [custom].[dim_survey_score]
AS
SELECT	distinct 
	score=convert(varchar(20),score),
	mdx=''
FROM custom.fact_agent_customer_survey
GO

--Data
/*
INSERT INTO [custom].[fact_agent_evaluation]
SELECT
	[evaluation_id]			= ROW_NUMBER()
	[local_date_id]			= d.date_id,
	[acd_login_id]			= acd.acd_login_id,
	[number_of_evaluations]	= d.date_id % 7 / 3,
	[quality_score_sum]		= d.date_id % 6 * 3
FROM mart.dim_date d
CROSS JOIN mart.dim_acd_login acd
WHERE d.date_id > 0
AND acd.acd_login_id > 0
AND d.date_date between '2014-02-01' and '2014-03-01'

INSERT INTO [custom].[fact_agent_evaluation]
SELECT 
	[evaluation_id]			= d.date_id,
	[local_date_id]			= d.date_id,
	[acd_login_id]			= acd.acd_login_id,
	[number_of_evaluations]	= d.date_id % 7 / 3,
	[quality_score_sum]		= d.date_id % 6 * 3
FROM mart.dim_date d
CROSS JOIN mart.dim_acd_login acd
WHERE d.date_id > 0
AND acd.acd_login_id > 0
AND d.date_date between '2014-02-01' and '2014-03-01'
*/