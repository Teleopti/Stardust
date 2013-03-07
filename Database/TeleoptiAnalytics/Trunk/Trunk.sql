IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RTA].[ActualAgentState]') AND type in (N'P', N'PC'))
DROP TABLE [RTA].[ActualAgentState]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [RTA].[ActualAgentState](
 [PersonId] [uniqueidentifier] NOT NULL,
 [StateCode] [nvarchar](500) NOT NULL,
 [PlatformTypeId] [uniqueidentifier] NOT NULL,
 [State] [nvarchar](500) NOT NULL,
 [StateId] [uniqueidentifier] NOT NULL,
 [Scheduled] [nvarchar](500) NOT NULL,
 [ScheduledId] [uniqueidentifier] NOT NULL,
 [StateStart] [datetime] NOT NULL,
 [ScheduledNext] [nvarchar](500) NOT NULL,
 [ScheduledNextId] [uniqueidentifier] NOT NULL,
 [NextStart] [datetime] NOT NULL,
 [AlarmName] [nvarchar](500) NOT NULL,
 [AlarmId] [uniqueidentifier] NOT NULL,
 [Color] [int] NOT NULL,
 [AlarmStart] [datetime] NOT NULL,
 [StaffingEffect] [float] NOT NULL,
 [ReceivedTime] [datetime] NOT NULL,
 CONSTRAINT [PK_ActualAgentState] PRIMARY KEY CLUSTERED 
(
 [PersonId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
----------------  
--Name: CS
--Date: 2013-02-11  
--Desc: PBI #22024 
----------------  

-- Add column for activity preference

ALTER TABLE stage.stg_schedule_preference ADD
	activity_code uniqueidentifier NULL
GO


----------------  
--Name: Karin and David
--Date: 2013-03-04
--Desc: #22446
---------------- 
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_job_delayed]') AND type in (N'U'))
BEGIN
	CREATE TABLE mart.etl_job_delayed
		(
			Id int identity (1,1) not null,
			stored_procedured nvarchar(300) not null,
			parameter_string nvarchar(1000) not null,
			insert_date smalldatetime not null,
			execute_date smalldatetime null
		)
	ALTER TABLE mart.etl_job_delayed ADD CONSTRAINT
		PK_etl_job_delayed PRIMARY KEY CLUSTERED 
		(
		Id
		)
	ALTER TABLE [mart].[etl_job_delayed] ADD  CONSTRAINT [DF_etl_job_delayed_insert_date]  DEFAULT (getdate()) FOR [insert_date]
END