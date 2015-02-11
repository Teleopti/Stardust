/************* CUSTOM SCHEMA ********************************/
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'custom')
EXEC sys.sp_executesql N'CREATE SCHEMA [custom]'
GO
/******************** CUSTOM TABLES **********************/
/****** Object:  Table [custom].[dim_call_outcome]    Script Date: 2015-01-15 10:47:55 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[dim_call_outcome]') AND type in (N'U'))
CREATE TABLE [custom].[dim_call_outcome](
	[call_outcome_id] [int] IDENTITY(1,1) NOT NULL,
	[call_outcome] [nvarchar](100) NULL,
	[call_result] [nvarchar](100) NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_call_outcome_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_call_outcome_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_dim_call_outcome] PRIMARY KEY CLUSTERED 
(
	[call_outcome_id] ASC
)
) ON [PRIMARY]

GO
/****** Object:  Table [custom].[dim_call_type]    Script Date: 2015-01-15 10:47:55 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[dim_call_type]') AND type in (N'U'))
CREATE TABLE [custom].[dim_call_type](
	[call_type_id] [int] IDENTITY(1,1) NOT NULL,
	[call_subtype] [nvarchar](100) NULL,
	[call_type] [nvarchar](100) NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_call_type_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_call_type_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_dim_call_type] PRIMARY KEY CLUSTERED 
(
	[call_type_id] ASC
)
) ON [PRIMARY]

GO
/****** Object:  Table [custom].[dim_campaign_type]    Script Date: 2015-01-15 10:47:55 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[dim_campaign_type]') AND type in (N'U'))
CREATE TABLE [custom].[dim_campaign_type](
	[campaign_type_id] [int] IDENTITY(1,1) NOT NULL,
	[campaign_type] [nvarchar](100) NULL,
	[subscriber_type] [nvarchar](100) NULL,
	[call_type] [nvarchar](100) NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_campaign_type_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_campaign_type_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_dim_campaign_type] PRIMARY KEY CLUSTERED 
(
	[campaign_type_id] ASC
)
) ON [PRIMARY]

GO
/****** Object:  Table [custom].[dim_service]    Script Date: 2015-01-15 10:47:55 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[dim_service]') AND type in (N'U'))
CREATE TABLE [custom].[dim_service](
	[service_id] [int] IDENTITY(1,1) NOT NULL,
	[service_name] [nvarchar](100) NULL,
	[service_type] [nvarchar](100) NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_service_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_service_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_dim_service] PRIMARY KEY CLUSTERED 
(
	[service_id] ASC
)
) ON [PRIMARY]

GO
/****** Object:  Table [custom].[dim_TRX_type]    Script Date: 2015-01-15 10:47:55 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[dim_TRX_type]') AND type in (N'U'))
CREATE TABLE [custom].[dim_TRX_type](
	[TRX_type_id] [int] IDENTITY(1,1) NOT NULL,
	[TRX_type] [nvarchar](100) NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_trx_type_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_trx_type_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_dim_TRX] PRIMARY KEY CLUSTERED 
(
	[TRX_type_id] ASC
)
) ON [PRIMARY]

GO
/****** Object:  Table [custom].[fact_3Gbundles]    Script Date: 2015-01-15 10:47:55 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[fact_3Gbundles]') AND type in (N'U'))
CREATE TABLE [custom].[fact_3Gbundles](
	[local_date_id] [int] NOT NULL,
	[acd_login_id] [int] NOT NULL,
	[service_id] [int] NOT NULL,
	[number_of_calls] [int] NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_3Gbundlesrvey_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_3Gbundles_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_fact_3Gbundles] PRIMARY KEY CLUSTERED 
(
	[local_date_id] ASC,
	[acd_login_id] ASC,
	[service_id] ASC
)
) ON [PRIMARY]

GO
/****** Object:  Table [custom].[fact_agent_customer_survey]    Script Date: 2015-01-15 10:47:55 ******/
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
) ON [PRIMARY]

GO
/****** Object:  Table [custom].[fact_agent_evaluation]    Script Date: 2015-01-15 10:47:55 ******/
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
/****** Object:  Table [custom].[fact_BO_performance]    Script Date: 2015-01-15 10:47:55 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[fact_BO_performance]') AND type in (N'U'))
CREATE TABLE [custom].[fact_BO_performance](
	[local_date_id] [int] NOT NULL,
	[acd_login_id] [int] NOT NULL,
	[number_of_cases] [int] NULL,
	[average_handling_time_s] [int] NULL,
	[productivity] [decimal](24, 2) NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_BO_performance_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_BO_performance_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_fact_BO_performance] PRIMARY KEY CLUSTERED 
(
	[local_date_id] ASC,
	[acd_login_id] ASC
)
) ON [PRIMARY]

GO
/****** Object:  Table [custom].[fact_BO_wrap_up]    Script Date: 2015-01-15 10:47:55 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[fact_BO_wrap_up]') AND type in (N'U'))
CREATE TABLE [custom].[fact_BO_wrap_up](
	[local_date_id] [int] NOT NULL,
	[acd_login_id] [int] NOT NULL,
	[call_type_id] [int] NOT NULL,
	[number_of_calls] [int] NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_fact_wrap_up_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_wrap_up_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_fact_wrap_up] PRIMARY KEY CLUSTERED 
(
	[local_date_id] ASC,
	[acd_login_id] ASC,
	[call_type_id] ASC
)
) ON [PRIMARY]

GO
/****** Object:  Table [custom].[fact_customer_profiles]    Script Date: 2015-01-15 10:47:55 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[fact_customer_profiles]') AND type in (N'U'))
CREATE TABLE [custom].[fact_customer_profiles](
	[local_date_id] [int] NOT NULL,
	[acd_login_id] [int] NOT NULL,
	[profiles_inserted] [int] NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_customer_profiles_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_customer_profiles_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_fact_customer_profiles] PRIMARY KEY CLUSTERED 
(
	[local_date_id] ASC,
	[acd_login_id] ASC
)
) ON [PRIMARY]

GO
/****** Object:  Table [custom].[fact_outbound_performance]    Script Date: 2015-01-15 10:47:55 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[fact_outbound_performance]') AND type in (N'U'))
CREATE TABLE [custom].[fact_outbound_performance](
	[local_date_id] [int] NOT NULL,
	[acd_login_id] [int] NOT NULL,
	[call_outcome_id] [int] NOT NULL,
	[campaign_type_id] [int] NOT NULL,
	[outbound_calls] [int] NULL,
	[outbound_calls_target] [int] NULL,
	[outbound_calls_reached] [int] NULL,
	[outbound_calls_sale] [int] NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_outbound_performance_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_outbound_performance_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_fact_outbound_performance] PRIMARY KEY CLUSTERED 
(
	[local_date_id] ASC,
	[acd_login_id] ASC,
	[call_outcome_id] ASC,
	[campaign_type_id] ASC
)
) ON [PRIMARY]

GO
/****** Object:  Table [custom].[fact_RBT_download]    Script Date: 2015-01-15 10:47:55 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[fact_RBT_download]') AND type in (N'U'))
CREATE TABLE [custom].[fact_RBT_download](
	[local_date_id] [int] NOT NULL,
	[acd_login_id] [int] NOT NULL,
	[trx_type_id] [int] NOT NULL,
	[number_of_downloads] [int] NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_RBT_download_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_RBT_download_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_fact_RBT_download] PRIMARY KEY CLUSTERED 
(
	[local_date_id] ASC,
	[acd_login_id] ASC,
	[trx_type_id] ASC
)
) ON [PRIMARY]

GO

/****** Object:  View [custom].[dim_team_leader]  Script Date: 2015-01-15 10:47:55 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[dim_team_leader]') AND type in (N'U'))
CREATE TABLE [custom].[dim_team_leader](
	[team_leader_id] [int] NOT NULL,
	[team_leader] [nvarchar](100) NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_team_leader_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_team_leader_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_dim_team_leader ] PRIMARY KEY CLUSTERED 
(
	[team_leader_id] ASC
)
) ON [PRIMARY]

GO
/****** Object:  Table [custom].[dim_training]    Script Date: 2015-01-20 15:47:05 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[dim_training]') AND type in (N'U'))
CREATE TABLE [custom].[dim_training](
	[training_id] [int] IDENTITY(1,1) NOT NULL,
	[training_name] [nvarchar](100) NULL,
	[start_date] [date] NULL,
	[end_date] [date] NULL,
	[trainer_id] [int] NULL,
	[trainer] [nvarchar](100) NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_training_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_training_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_dim_training] PRIMARY KEY CLUSTERED 
(
	[training_id] ASC
)
) ON [PRIMARY]

GO
/****** Object:  Table [custom].[dim_violation]    Script Date: 2015-01-20 15:47:05 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[dim_violation]') AND type in (N'U'))
CREATE TABLE [custom].[dim_violation](
	[violation_id] [int] IDENTITY(1,1) NOT NULL,
	[violation] [nvarchar](100) NOT NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_violation_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_violation_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_dim_violation] PRIMARY KEY CLUSTERED 
(
	[violation_id] ASC
)
) ON [PRIMARY]

GO

/****** Object:  Table [custom].[fact_training]    Script Date: 2015-01-20 15:55:34 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[fact_training]') AND type in (N'U'))
CREATE TABLE [custom].[fact_training](
	[start_local_date_id] [int] NOT NULL,
	[employee_acd_login_id] [int] NOT NULL,
	[training_id] [int] NOT NULL,
	[team_leader_id] [int] NOT NULL,
	[team_id] [int] NOT NULL,
	[attendance] [int] NOT NULL,
	[count] [int] NULL,
	[score] [int] NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_training_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_training_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_fact_training] PRIMARY KEY CLUSTERED 
(
	[start_local_date_id] ASC,
	[employee_acd_login_id] ASC,
	[training_id] ASC,
	[team_leader_id] ASC,
	[team_id] ASC,
	[attendance] ASC
)
) ON [PRIMARY]

GO
/****** Object:  Table [custom].[fact_violation]    Script Date: 2015-01-20 15:56:13 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[fact_violation]') AND type in (N'U'))
CREATE TABLE [custom].[fact_violation](
	[violation_local_date_id] [int] NOT NULL,
	[employee_acd_login_id] [int] NOT NULL,
	[violation_id] [int] NOT NULL,
	[provided_by_id] [int] NOT NULL,
	[is_approved] [int] NOT NULL,
	[points] [int] NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_violation_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_violation_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_fact_violation] PRIMARY KEY CLUSTERED 
(
	[violation_local_date_id] ASC,
	[employee_acd_login_id] ASC,
	[violation_id] ASC
)
) ON [PRIMARY]

GO
/****** Object:  Table [custom].[dim_provided_by]    Script Date: 2015-01-20 16:25:22 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[dim_provided_by]') AND type in (N'U'))
CREATE TABLE [custom].[dim_provided_by](
	[provided_by_id] [int] NOT NULL,
	[provided_by] [nvarchar](100) NOT NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_provided_by_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_provided_by_insert_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_dim_provided_by] PRIMARY KEY CLUSTERED 
(
	[provided_by_id] ASC
)
) ON [PRIMARY]

GO
/***************CUSTOM VIEWS*************************************************/
/****** Object:  View [custom].[dim_survey_score]    Script Date: 2015-01-15 10:47:55 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[custom].[dim_survey_score]'))
DROP VIEW [custom].[dim_survey_score]
GO
CREATE  VIEW [custom].[dim_survey_score] AS

SELECT        distinct 
				score=convert(varchar(20),score) ,
				mdx=''
FROM            custom.fact_agent_customer_survey

GO
/****** Object:  View [custom].[dim_attendance_v]    Script Date: 2015-01-20 15:47:05 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[custom].[dim_attendance_v]'))
DROP VIEW [custom].[dim_attendance_v]
GO
CREATE  VIEW [custom].[dim_attendance_v] 
AS

SELECT        
attendance=0,
attendance_YN='N'
UNION
SELECT        
attendance=1,
attendance_YN='Y'

GO
/****** Object:  View [custom].[dim_is_approved_v]    Script Date: 2015-01-20 15:47:05 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[custom].[dim_is_approved_v]'))
DROP VIEW [custom].[dim_is_approved_v]
GO
CREATE  VIEW [custom].[dim_is_approved_v] AS

SELECT        
is_approved=0,
is_approved_YN='N'
UNION
SELECT        
is_approved=1,
is_approved_YN='Y'

GO





