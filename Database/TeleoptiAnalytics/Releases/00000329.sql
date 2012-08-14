/* 
Trunk initiated: 
2011-05-30 
10:16
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: AndersF
--Date: 2011-06-01  
--Desc: It should be 25
----------------  
update [RTA].[ExternalAgentState]
set [StateCode] = LEFT([StateCode],25)
 
ALTER TABLE [RTA].[ExternalAgentState] ALTER COLUMN [StateCode] nvarchar(25)

GO

----------------  
--Name: Jonas N
--Date: 2011-06-03  
--Desc: Add missing ETL job steps to database (job step execution history).
----------------  
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (65, N'stg_skill')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (66, N'stg_schedule_preference')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (67, N'stg_group_page_person')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (68, N'stg_overtime')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (69, N'dim_group_page')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (70, N'dim_overtime')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (71, N'fact_schedule_preference')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (72, N'Performance Manager user check')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (73, N'Maintenance')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (74, N'bridge_group_page_person')

----------------  
--Name: David Jonsson
--Date: 2011-04-05
--Desc: Adding clustered indexes to all tables (Azure pre-req)
---------------- 
--update FK-names badly formatted
---------------- 
declare @fkname nvarchar(2000)
SELECT @fkname=k.name
FROM sys.foreign_keys k
INNER JOIN sys.schemas s
ON k.schema_id=s.schema_id
WHERE OBJECT_NAME(parent_object_id)='aspnet_users'
AND s.name='dbo'
AND OBJECT_NAME(referenced_object_id)='aspnet_applications'

exec sp_rename @objname = @fkname, @newname = 'FK_aspnet_User_aspnet_applications', @objtype = 'OBJECT'
go

declare @fkname nvarchar(2000)
SELECT @fkname=k.name
FROM sys.foreign_keys k
INNER JOIN sys.schemas s
ON k.schema_id=s.schema_id
WHERE OBJECT_NAME(parent_object_id)='aspnet_Membership'
AND s.name='dbo'
AND OBJECT_NAME(referenced_object_id)='aspnet_users'

exec sp_rename @objname = @fkname, @newname = 'FK_aspnet_Membership_aspnet_Users', @objtype = 'OBJECT'
go

declare @fkname nvarchar(2000)
SELECT @fkname=k.name
FROM sys.foreign_keys k
INNER JOIN sys.schemas s
ON k.schema_id=s.schema_id
WHERE OBJECT_NAME(parent_object_id)='aspnet_Membership'
AND s.name='dbo'
AND OBJECT_NAME(referenced_object_id)='aspnet_applications'

exec sp_rename @objname = @fkname, @newname = 'FK_aspnet_Membership_aspnet_applications', @objtype = 'OBJECT'
go

--------------------
--aspnet_Users
--------------------
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_aspnet_Membership_aspnet_Users]') AND parent_object_id = OBJECT_ID(N'[dbo].[aspnet_Membership]'))
ALTER TABLE [dbo].[aspnet_Membership] DROP CONSTRAINT [FK_aspnet_Membership_aspnet_Users]
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[aspnet_Users]') AND name = N'PK_aspnet_Users')
ALTER TABLE [dbo].[aspnet_Users] DROP CONSTRAINT [PK_aspnet_Users]

ALTER TABLE [dbo].[aspnet_Users] ADD  CONSTRAINT [PK_aspnet_Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
) ON [MART]

ALTER TABLE [dbo].[aspnet_Membership]  WITH CHECK ADD  CONSTRAINT [FK_aspnet_Membership_aspnet_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[aspnet_Users] ([UserId])
ALTER TABLE [dbo].[aspnet_Membership] CHECK CONSTRAINT [FK_aspnet_Membership_aspnet_Users]
GO

--------------------
--aspnet_applications
--------------------
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_aspnet_User_aspnet_applications]') AND parent_object_id = OBJECT_ID(N'[dbo].[aspnet_Users]'))
ALTER TABLE [dbo].[aspnet_Users] DROP CONSTRAINT [FK_aspnet_User_aspnet_applications]
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_aspnet_Membership_aspnet_applications]') AND parent_object_id = OBJECT_ID(N'[dbo].[aspnet_Membership]'))
ALTER TABLE [dbo].[aspnet_Membership] DROP CONSTRAINT [FK_aspnet_Membership_aspnet_applications]
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[aspnet_applications]') AND name = N'PK_aspnet_applications')
ALTER TABLE [dbo].[aspnet_applications] DROP CONSTRAINT [PK_aspnet_applications]

ALTER TABLE [dbo].[aspnet_applications] ADD  CONSTRAINT [PK_aspnet_applications] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC
) ON [MART]

ALTER TABLE [dbo].[aspnet_Membership]  WITH CHECK ADD  CONSTRAINT [FK_aspnet_Membership_aspnet_applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[aspnet_applications] ([ApplicationId])
ALTER TABLE [dbo].[aspnet_Membership] CHECK CONSTRAINT [FK_aspnet_Membership_aspnet_applications]

ALTER TABLE [dbo].[aspnet_Users]  WITH CHECK ADD  CONSTRAINT [FK_aspnet_User_aspnet_applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[aspnet_applications] ([ApplicationId])
ALTER TABLE [dbo].[aspnet_Users] CHECK CONSTRAINT [FK_aspnet_User_aspnet_applications]
GO

--------------------
--aspnet_Membership
--------------------
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[aspnet_Membership]') AND name = N'PK_aspnet_Membership')
ALTER TABLE [dbo].[aspnet_Membership] DROP CONSTRAINT [PK_aspnet_Membership]
ALTER TABLE [dbo].[aspnet_Membership] ADD  CONSTRAINT [PK_aspnet_Membership] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
) ON [MART]
GO

---------------- 
--update existing index (heaps) => clustered
---------------- 
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[report_control_collection]') AND name = N'PK_report_control_collection')
ALTER TABLE [mart].[report_control_collection] DROP CONSTRAINT [PK_report_control_collection]
GO

ALTER TABLE [mart].[report_control_collection] ADD  CONSTRAINT [PK_report_control_collection] PRIMARY KEY CLUSTERED 
(
	[control_collection_id] ASC
) ON [MART]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[msg].[Event]') AND name = N'PK_Event')
ALTER TABLE [msg].[Event] DROP CONSTRAINT [PK_Event]
GO

ALTER TABLE [msg].[Event] ADD  CONSTRAINT [PK_Event] PRIMARY KEY CLUSTERED 
(
	[EventId] ASC
) ON [MSG]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[msg].[Filter]') AND name = N'PK_Filter')
ALTER TABLE [msg].[Filter] DROP CONSTRAINT [PK_Filter]
GO

ALTER TABLE [msg].[Filter] ADD  CONSTRAINT [PK_Filter] PRIMARY KEY CLUSTERED 
(
	[FilterId] ASC
)  ON [MSG]
GO

---------------- 
--Add new indexes for some that are missing them
---------------- 
CREATE CLUSTERED INDEX [CIX_stg_permisssion_report] ON [stage].[stg_permission_report] 
(
	[person_code] ASC
) ON [STAGE]
GO

CREATE CLUSTERED INDEX [CIX_stg_queue_date] ON [stage].[stg_queue] 
(
	[date] ASC
) ON [STAGE]
GO

CREATE CLUSTERED INDEX [CIX_pm_user] ON [mart].[pm_user] 
(
	[user_name] ASC
) ON [MART]
GO


CREATE CLUSTERED INDEX [CIX_stg_acd_login_person] ON [stage].[stg_acd_login_person] 
(
	[acd_login_code] ASC
) ON [STAGE]
GO

CREATE CLUSTERED INDEX [CIX_stg_overtime] ON [stage].[stg_overtime] 
(
	[overtime_code] ASC
) ON [STAGE]
GO

ALTER TABLE mart.dim_overtime ADD CONSTRAINT
	PK_dim_overtime PRIMARY KEY CLUSTERED 
	(
	overtime_id
	)  ON MART
GO


-- =============================================
--remove guid Id => BigInt Id
-- =============================================
DROP TABLE [RTA].[ExternalAgentState]
GO

CREATE TABLE [RTA].[ExternalAgentState](
	[Id] [bigint] identity(1,1)  NOT NULL,
	[LogOn] [nvarchar](50) NOT NULL,
	[StateCode] [nvarchar](50) NOT NULL,
	[TimeInState] [bigint] NOT NULL,
	[TimestampValue] [datetime] NOT NULL,
	[PlatformTypeId] [uniqueidentifier] NULL,
	[DataSourceId] [int] NULL,
	[BatchId] [datetime] NULL,
	[IsSnapshot] [bit] NOT NULL
)
CREATE CLUSTERED INDEX [IXC_ExternalAgentState_LogOn_Timestamp] ON [RTA].[ExternalAgentState] 
(
	[LogOn] ASC,
	[TimestampValue] ASC
) ON [RTA]
GO

ALTER TABLE [RTA].[ExternalAgentState] ADD  CONSTRAINT [DF_ExternalAgentState_IsSnapshot]  DEFAULT ((0)) FOR [IsSnapshot]
GO

-- ==============================================
--Agg
-- ==============================================
CREATE TABLE [dbo].[acd_type](
	[acd_type_id] [int] NOT NULL,
	[acd_type_desc] [varchar](50) NOT NULL,
 CONSTRAINT [PK_acd_type] PRIMARY KEY CLUSTERED 
(
	[acd_type_id] ASC
)
)
--
CREATE TABLE [dbo].[log_object](
	[log_object_id] [int] NOT NULL,
	[acd_type_id] [int] NOT NULL,
	[log_object_desc] [varchar](50) NOT NULL,
	[logDB_name] [varchar](50) NOT NULL,
	[intervals_per_day] [int] NOT NULL,
	[default_service_level_sec] [int] NULL,
	[default_short_call_treshold] [int] NULL,
 CONSTRAINT [PK_log_object] PRIMARY KEY CLUSTERED 
(
	[log_object_id] ASC
)
)

ALTER TABLE [dbo].[log_object]  WITH CHECK ADD  CONSTRAINT [FK_log_object_acd_type] FOREIGN KEY([acd_type_id])
REFERENCES [dbo].[acd_type] ([acd_type_id])
GO
ALTER TABLE [dbo].[log_object] CHECK CONSTRAINT [FK_log_object_acd_type]
--
CREATE TABLE [dbo].[agent_info](
	[Agent_id] [int] NOT NULL,
	[Agent_name] [varchar](50) NOT NULL,
	[is_active] [bit] NULL,
	[log_object_id] [int] NOT NULL,
	[orig_agent_id] [varchar](50) NOT NULL,
 CONSTRAINT [PK_agent_info] PRIMARY KEY CLUSTERED 
(
	[Agent_id] ASC
)
)

ALTER TABLE [dbo].[agent_info]  WITH CHECK ADD  CONSTRAINT [FK_agent_info_log_object] FOREIGN KEY([log_object_id])
REFERENCES [dbo].[log_object] ([log_object_id])
GO
ALTER TABLE [dbo].[agent_info] CHECK CONSTRAINT [FK_agent_info_log_object]
--
CREATE TABLE [dbo].[ccc_system_info](
	[id] [int] NOT NULL,
	[desc] [varchar](50) NOT NULL,
	[int_value] [int] NULL,
	[varchar_value] [char](10) NULL,
 CONSTRAINT [PK_ccc_system_info] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)
)
--
CREATE TABLE [dbo].[queues](
	[queue] [int] NOT NULL,
	[orig_desc] [varchar](50) NULL,
	[log_object_id] [int] NOT NULL,
	[orig_queue_id] [int] NULL,
	[display_desc] [varchar](50) NULL,
 CONSTRAINT [PK_queues] PRIMARY KEY CLUSTERED 
(
	[queue] ASC
)
)
ALTER TABLE [dbo].[queues]  WITH CHECK ADD  CONSTRAINT [FK_queues_log_object] FOREIGN KEY([log_object_id])
REFERENCES [dbo].[log_object] ([log_object_id])
GO
ALTER TABLE [dbo].[queues] CHECK CONSTRAINT [FK_queues_log_object]
--
CREATE TABLE [dbo].[agent_logg](
	[queue] [int] NOT NULL,
	[date_from] [smalldatetime] NOT NULL,
	[interval] [int] NOT NULL,
	[agent_id] [int] NOT NULL,
	[agent_name] [nvarchar](50) NULL,
	[avail_dur] [int] NULL,
	[tot_work_dur] [int] NULL,
	[talking_call_dur] [int] NULL,
	[pause_dur] [int] NULL,
	[wait_dur] [int] NULL,
	[wrap_up_dur] [int] NULL,
	[answ_call_cnt] [int] NULL,
	[direct_out_call_cnt] [int] NULL,
	[direct_out_call_dur] [int] NULL,
	[direct_in_call_cnt] [int] NULL,
	[direct_in_call_dur] [int] NULL,
	[transfer_out_call_cnt] [int] NULL,
	[admin_dur] [int] NULL,
 CONSTRAINT [PK_agent_logg] PRIMARY KEY CLUSTERED 
(
	[queue] ASC,
	[date_from] ASC,
	[interval] ASC,
	[agent_id] ASC
)
)

GO
ALTER TABLE [dbo].[agent_logg]  WITH CHECK ADD  CONSTRAINT [FK_agent_logg_agent_info] FOREIGN KEY([agent_id])
REFERENCES [dbo].[agent_info] ([Agent_id])
GO
ALTER TABLE [dbo].[agent_logg] CHECK CONSTRAINT [FK_agent_logg_agent_info]
GO
ALTER TABLE [dbo].[agent_logg]  WITH CHECK ADD  CONSTRAINT [FK_agent_logg_queues] FOREIGN KEY([queue])
REFERENCES [dbo].[queues] ([queue])
GO
ALTER TABLE [dbo].[agent_logg] CHECK CONSTRAINT [FK_agent_logg_queues]
--
CREATE TABLE [dbo].[queue_logg](
	[queue] [int] NOT NULL,
	[date_from] [smalldatetime] NOT NULL,
	[interval] [int] NOT NULL,
	[offd_direct_call_cnt] [int] NULL,
	[overflow_in_call_cnt] [int] NULL,
	[aband_call_cnt] [int] NULL,
	[overflow_out_call_cnt] [int] NULL,
	[answ_call_cnt] [int] NULL,
	[queued_and_answ_call_dur] [int] NULL,
	[queued_and_aband_call_dur] [int] NULL,
	[talking_call_dur] [int] NULL,
	[wrap_up_dur] [int] NULL,
	[queued_answ_longest_que_dur] [int] NULL,
	[queued_aband_longest_que_dur] [int] NULL,
	[avg_avail_member_cnt] [int] NULL,
	[ans_servicelevel_cnt] [int] NULL,
	[wait_dur] [int] NULL,
	[aband_short_call_cnt] [int] NULL,
	[aband_within_sl_cnt] [int] NULL,
 CONSTRAINT [PK_queue_logg] PRIMARY KEY CLUSTERED 
(
	[queue] ASC,
	[date_from] ASC,
	[interval] ASC
)
)

GO
ALTER TABLE [dbo].[queue_logg]  WITH CHECK ADD  CONSTRAINT [FK_queue_logg_queues] FOREIGN KEY([queue])
REFERENCES [dbo].[queues] ([queue])
GO
ALTER TABLE [dbo].[queue_logg] CHECK CONSTRAINT [FK_queue_logg_queues]
 
GO 
 


PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (329,'7.1.329') 
