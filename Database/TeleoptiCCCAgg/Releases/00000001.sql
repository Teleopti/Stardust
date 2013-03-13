CREATE TABLE [dbo].[t_agent_report](
	[id_nr] [int] NOT NULL,
	[date] [smalldatetime] NOT NULL,
	[interval] [smalldatetime] NOT NULL,
	[agt_id] [int] NOT NULL,
	[agt_name] [varchar](50) NULL,
	[acd_dn_number] [varchar](10) NOT NULL,
	[calls_answd] [int] NULL,
	[non_actvy_calls] [int] NULL,
	[acd_calls_xferd] [int] NULL,
	[in_dn_calls] [int] NULL,
	[out_dn_calls] [int] NULL,
	[dn_calls_xferd] [int] NULL,
	[short_calls] [int] NULL,
	[total_acd_talk_time] [int] NULL,
	[total_not_ready_time] [int] NULL,
	[total_in_dn_time] [int] NULL,
	[total_out_dn_time] [int] NULL,
	[total_wait_time] [int] NULL,
	[total_hold_time] [int] NULL,
	[total_walk_time] [int] NULL,
	[total_busy_time] [int] NULL,
	[total_login_time] [int] NULL,
	[total_consult_time] [int] NULL,
	[total_staff_time] [int] NULL,
 CONSTRAINT [PK_t_agent_report] PRIMARY KEY NONCLUSTERED 
(
	[id_nr] ASC,
	[date] ASC,
	[interval] ASC,
	[agt_id] ASC,
	[acd_dn_number] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

GO
/****** Object:  Table [dbo].[t_log_alcatel_realtime]    Script Date: 10/02/2008 18:11:13 ******/

GO

GO
CREATE TABLE [dbo].[t_log_alcatel_realtime](
	[agent_id] [int] NULL,
	[state_id] [int] NULL,
	[start_time] [datetime] NULL,
	[end_time] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_log_avaya_realtime]    Script Date: 10/02/2008 18:11:18 ******/

GO

GO
CREATE TABLE [dbo].[t_log_avaya_realtime](
	[main_object_id] [int] NOT NULL,
	[time_stamp] [int] NOT NULL,
	[extension] [int] NOT NULL,
	[workmode_direction] [int] NULL,
	[ag_duration] [int] NULL,
	[auxreason] [int] NULL,
	[da_inqueue] [int] NULL,
	[workskill] [int] NULL,
	[acdonhold] [int] NULL,
	[acd] [int] NULL,
	[logid] [int] NOT NULL,
	[updated] [bit] NOT NULL CONSTRAINT [DF_t_avaya_realtime_updateted]  DEFAULT ((1)),
 CONSTRAINT [PK_t_log_avaya_realtime] PRIMARY KEY CLUSTERED 
(
	[main_object_id] ASC,
	[time_stamp] ASC,
	[logid] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_log_callguide_realtime]    Script Date: 10/02/2008 18:11:22 ******/

GO

GO
CREATE TABLE [dbo].[t_log_callguide_realtime](
	[main_object_id] [int] NOT NULL,
	[agent_id] [int] NOT NULL,
	[agent_name] [nvarchar](200) NULL,
	[event] [nvarchar](200) NULL,
	[agent_status] [nvarchar](200) NULL,
	[work_level] [nvarchar](200) NULL,
	[updated] [bit] NOT NULL,
	[workmode_code] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_log_oasis_realtime]    Script Date: 10/02/2008 18:11:52 ******/

GO

GO
CREATE TABLE [dbo].[t_log_oasis_realtime](
	[main_object_id] [int] NOT NULL,
	[session_id] [int] NULL,
	[operator] [nvarchar](50) NOT NULL,
	[workstation] [nvarchar](50) NULL,
	[state] [int] NULL,
	[time_in_state] [int] NULL,
	[call_centre] [nvarchar](50) NULL,
	[workstation_location] [nvarchar](50) NULL,
	[team_name] [nvarchar](50) NULL,
	[ip] [nvarchar](50) NULL,
	[updated] [bit] NULL,
 CONSTRAINT [PK_t_log_oasis_realtime] PRIMARY KEY CLUSTERED 
(
	[main_object_id] ASC,
	[operator] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_log_solidus_realtime]    Script Date: 10/02/2008 18:11:56 ******/

GO

GO
CREATE TABLE [dbo].[t_log_solidus_realtime](
	[main_object_id] [int] NOT NULL,
	[agent_id] [int] NOT NULL,
	[loggedon] [int] NOT NULL CONSTRAINT [DF_t_log_solidus_realtime_loggon]  DEFAULT ((0)),
	[voice] [int] NOT NULL CONSTRAINT [DF_t_log_solidus_realtime_voice]  DEFAULT ((0)),
	[email] [int] NOT NULL CONSTRAINT [DF_t_log_solidus_realtime_email]  DEFAULT ((0)),
	[media] [int] NOT NULL CONSTRAINT [DF_t_log_solidus_realtime_media]  DEFAULT ((0)),
	[updated] [int] NOT NULL CONSTRAINT [DF_t_log_solidus_realtime_updated]  DEFAULT ((1)),
	[event_id] [int] NULL,
 CONSTRAINT [PK_t_log_solidus_realtime] PRIMARY KEY CLUSTERED 
(
	[main_object_id] ASC,
	[agent_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_log_symposium_realtime]    Script Date: 10/02/2008 18:12:01 ******/

GO

GO
CREATE TABLE [dbo].[t_log_symposium_realtime](
	[main_object_id] [int] NOT NULL,
	[agent_id] [int] NOT NULL,
	[state] [int] NOT NULL,
	[supervisor_id] [int] NOT NULL,
	[time_in_state] [int] NOT NULL,
	[answering_skillset] [int] NOT NULL,
	[dn_in_time_in_state] [int] NOT NULL,
	[dn_out_time_in_state] [int] NOT NULL,
	[position_id] [int] NOT NULL,
	[updated] [bit] NOT NULL CONSTRAINT [DF_t_log_symposium_realtime_updated]  DEFAULT ((1)),
 CONSTRAINT [PK_t_log_symposium_realtime] PRIMARY KEY CLUSTERED 
(
	[main_object_id] ASC,
	[agent_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[agent_logg_intraday]    Script Date: 10/02/2008 18:10:07 ******/

GO

GO
CREATE TABLE [dbo].[agent_logg_intraday](
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
 CONSTRAINT [PK_agent_logg_intraday_1__11] PRIMARY KEY CLUSTERED 
(
	[date_from] ASC,
	[agent_id] ASC,
	[queue] ASC,
	[interval] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_log_NET_data_avaya_agent]    Script Date: 10/02/2008 18:11:28 ******/

GO

GO
CREATE TABLE [dbo].[t_log_NET_data_avaya_agent](
	[main_id] [int] NOT NULL,
	[node_id] [int] NOT NULL,
	[time_stamp] [int] NOT NULL,
	[extension] [int] NOT NULL,
	[workmode_direction] [int] NULL,
	[ag_duration] [int] NULL,
	[auxreason] [int] NULL,
	[da_inqueue] [int] NULL,
	[workskill] [int] NULL,
	[acdonhold] [int] NULL,
	[acd] [int] NULL,
	[logid] [int] NOT NULL,
	[updated] [bit] NOT NULL CONSTRAINT [DF_t_log_net_avaya_realtime_updateted]  DEFAULT ((1)),
 CONSTRAINT [PK_t_net_log_avaya_realtime] PRIMARY KEY CLUSTERED 
(
	[main_id] ASC,
	[time_stamp] ASC,
	[logid] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_log_NET_data_callguide_agent]    Script Date: 10/02/2008 18:11:32 ******/

GO

GO
CREATE TABLE [dbo].[t_log_NET_data_callguide_agent](
	[main_id] [int] NOT NULL,
	[node_id] [int] NOT NULL,
	[agent_id] [int] NOT NULL,
	[agent_name] [nvarchar](200) NULL,
	[event] [nvarchar](200) NULL,
	[agent_status] [nvarchar](200) NULL,
	[work_level] [nvarchar](200) NULL,
	[updated] [bit] NOT NULL,
	[workmode_code] [nvarchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_log_NET_data_genesys_ctia_agent]    Script Date: 10/02/2008 18:11:34 ******/

GO

GO
CREATE TABLE [dbo].[t_log_NET_data_genesys_ctia_agent](
	[main_id] [int] NOT NULL,
	[agent_id] [nvarchar](200) NOT NULL,
	[agent_status] [nvarchar](200) NOT NULL,
	[updated] [bit] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_log_NET_data_solidus_agent]    Script Date: 10/02/2008 18:11:39 ******/

GO

GO
CREATE TABLE [dbo].[t_log_NET_data_solidus_agent](
	[main_id] [int] NOT NULL,
	[agent_id] [int] NOT NULL,
	[loggedon] [int] NOT NULL CONSTRAINT [DF_t_log_net_solidus_realtime_loggon]  DEFAULT ((0)),
	[voice] [int] NOT NULL CONSTRAINT [DF_t_log_net_solidus_realtime_voice]  DEFAULT ((0)),
	[email] [int] NOT NULL CONSTRAINT [DF_t_log_net_solidus_realtime_email]  DEFAULT ((0)),
	[media] [int] NOT NULL CONSTRAINT [DF_t_log_net_solidus_realtime_media]  DEFAULT ((0)),
	[updated] [int] NOT NULL CONSTRAINT [DF_t_log_net_solidus_realtime_updated]  DEFAULT ((1)),
	[event_id] [int] NULL,
 CONSTRAINT [PK_t_log_net_solidus_realtime] PRIMARY KEY CLUSTERED 
(
	[main_id] ASC,
	[agent_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_log_NET_data_solidus_logg]    Script Date: 10/02/2008 18:11:42 ******/

GO

GO
CREATE TABLE [dbo].[t_log_NET_data_solidus_logg](
	[main_id] [int] NOT NULL,
	[agent_id] [int] NOT NULL,
	[event_type_id] [int] NOT NULL,
	[parameter] [nvarchar](100) NOT NULL,
	[value] [nvarchar](100) NOT NULL,
	[time] [datetime] NOT NULL CONSTRAINT [DF_t_log_net_solidus_logg_time]  DEFAULT (getdate())
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_log_NET_data_symposium_agent]    Script Date: 10/02/2008 18:11:46 ******/

GO

GO
CREATE TABLE [dbo].[t_log_NET_data_symposium_agent](
	[main_id] [int] NOT NULL,
	[agent_id] [int] NOT NULL,
	[state] [int] NOT NULL,
	[supervisor_id] [int] NOT NULL,
	[time_in_state] [int] NOT NULL,
	[answering_skillset] [int] NOT NULL,
	[dn_in_time_in_state] [int] NOT NULL,
	[dn_out_time_in_state] [int] NOT NULL,
	[position_id] [int] NOT NULL,
	[updated] [bit] NOT NULL CONSTRAINT [DF_t_log_NET_data_symposium_agent_updated]  DEFAULT ((1))
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_agent_reason]    Script Date: 10/02/2008 18:10:57 ******/

GO

GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[t_agent_reason](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Reason_id] [int] NULL,
	[Reason_name] [varchar](250) NULL
) ON [PRIMARY]
GO

GO
/****** Object:  Table [dbo].[t_agent_status_logg]    Script Date: 10/02/2008 18:11:11 ******/

GO

GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[t_agent_status_logg](
	[Agent_id] [varchar](50) NOT NULL,
	[Logon_id] [varchar](100) NULL,
	[Time_from] [datetime] NOT NULL,
	[Time_to] [datetime] NULL,
	[State] [int] NOT NULL,
	[Reason_id] [int] NOT NULL,
 CONSTRAINT [PK_t_agent_status_logg] PRIMARY KEY CLUSTERED 
(
	[Agent_id] ASC,
	[Time_from] ASC,
	[State] ASC,
	[Reason_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

GO
/****** Object:  Table [dbo].[t_rta_display_data]    Script Date: 10/02/2008 18:12:09 ******/

GO

GO
CREATE TABLE [dbo].[t_rta_display_data](
	[emp_id] [int] NOT NULL,
	[skill_id] [int] NOT NULL,
	[login_id] [nvarchar](50) NOT NULL,
	[sign] [nvarchar](50) NULL,
	[f_name] [nvarchar](50) NULL,
	[l_name] [nvarchar](50) NULL,
	[name] [nvarchar](101) NULL,
	[unit_id] [int] NULL,
	[unit_name] [nvarchar](30) NOT NULL,
	[sched_activity_id] [int] NULL,
	[sched_activity_name] [nvarchar](60) NULL,
	[sched_activity_color] [int] NULL,
	[next_activity_name] [nvarchar](60) NULL,
	[next_activity_start_time] [smalldatetime] NULL,
	[state_group_id] [int] NULL,
	[state_group_name] [nvarchar](25) NULL,
	[log_object_id] [int] NOT NULL CONSTRAINT [DF_t_rta_display_data_log_object_id]  DEFAULT ((1)),
 CONSTRAINT [PK_t_rta_dispaly_data] PRIMARY KEY CLUSTERED 
(
	[emp_id] ASC,
	[skill_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[acd_type]    Script Date: 10/02/2008 18:09:39 ******/

GO

GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[acd_type](
	[acd_type_id] [int] NOT NULL,
	[acd_type_desc] [varchar](50) NOT NULL,
 CONSTRAINT [PK_acd_type] PRIMARY KEY CLUSTERED 
(
	[acd_type_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

GO
/****** Object:  Table [dbo].[ccc_system_info]    Script Date: 10/02/2008 18:10:18 ******/

GO

GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ccc_system_info](
	[id] [int] NOT NULL,
	[desc] [varchar](50) NOT NULL,
	[int_value] [int] NULL,
	[varchar_value] [char](10) NULL,
 CONSTRAINT [PK_ccc_system_info] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

GO
/****** Object:  Table [dbo].[goal_results]    Script Date: 10/02/2008 18:10:21 ******/

GO

GO
CREATE TABLE [dbo].[goal_results](
	[queue] [int] NOT NULL,
	[date_from] [smalldatetime] NOT NULL,
	[interval] [int] NOT NULL,
	[goal_id] [int] NOT NULL,
	[answ_call_cnt] [int] NULL,
	[aband_call_cnt] [int] NULL,
 CONSTRAINT [PK_TMPgoal_results] PRIMARY KEY CLUSTERED 
(
	[queue] ASC,
	[date_from] ASC,
	[interval] ASC,
	[goal_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[agent_logg]    Script Date: 10/02/2008 18:09:59 ******/

GO

GO
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
 CONSTRAINT [PK_agent_logg_1__11] PRIMARY KEY CLUSTERED 
(
	[date_from] ASC,
	[agent_id] ASC,
	[queue] ASC,
	[interval] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[project_logg]    Script Date: 10/02/2008 18:10:33 ******/

GO

GO
CREATE TABLE [dbo].[project_logg](
	[queue] [int] NOT NULL,
	[date_from] [smalldatetime] NOT NULL,
	[interval] [int] NOT NULL,
	[minutes] [int] NULL,
	[contacts] [int] NULL,
	[orders] [int] NULL,
 CONSTRAINT [PK_project_logg] PRIMARY KEY CLUSTERED 
(
	[queue] ASC,
	[date_from] ASC,
	[interval] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[queue_logg]    Script Date: 10/02/2008 18:10:50 ******/

GO

GO
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
 CONSTRAINT [PK_queue_logg_1__11] PRIMARY KEY CLUSTERED 
(
	[queue] ASC,
	[date_from] ASC,
	[interval] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[queue_by_day_logg]    Script Date: 10/02/2008 18:10:41 ******/

GO

GO
CREATE TABLE [dbo].[queue_by_day_logg](
	[queue] [int] NOT NULL,
	[date_from] [smalldatetime] NOT NULL,
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
 CONSTRAINT [PK_queue_by_day_logg] PRIMARY KEY CLUSTERED 
(
	[queue] ASC,
	[date_from] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[agent_state_logg]    Script Date: 10/02/2008 18:10:10 ******/

GO

GO
CREATE TABLE [dbo].[agent_state_logg](
	[date_from] [smalldatetime] NOT NULL,
	[state_id] [int] NOT NULL,
	[agent_id] [int] NOT NULL,
	[interval] [int] NOT NULL,
	[state_dur] [int] NULL,
 CONSTRAINT [PK_agent_state_logg] PRIMARY KEY CLUSTERED 
(
	[date_from] ASC,
	[state_id] ASC,
	[agent_id] ASC,
	[interval] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[log_object]    Script Date: 10/02/2008 18:10:25 ******/

GO

GO
SET ANSI_PADDING ON
GO
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

GO
/****** Object:  Table [dbo].[acd_type_detail]    Script Date: 10/02/2008 18:09:41 ******/

GO

GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[acd_type_detail](
	[acd_type_id] [int] NOT NULL,
	[detail_id] [int] NOT NULL,
	[detail_name] [varchar](50) NOT NULL,
	[proc_name] [varchar](50) NOT NULL,
 CONSTRAINT [PK_acd_type_detail] PRIMARY KEY CLUSTERED 
(
	[acd_type_id] ASC,
	[detail_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

GO
/****** Object:  Table [dbo].[agent_by_day_logg]    Script Date: 10/02/2008 18:09:48 ******/

GO

GO
CREATE TABLE [dbo].[agent_by_day_logg](
	[date_from] [smalldatetime] NOT NULL,
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
 CONSTRAINT [PK_agent_by_day_logg] PRIMARY KEY CLUSTERED 
(
	[date_from] ASC,
	[agent_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[queues]    Script Date: 10/02/2008 18:10:53 ******/

GO

GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[queues](
	[queue] [int] IDENTITY(1,1) NOT NULL,
	[orig_desc] [varchar](50) NULL,
	[log_object_id] [int] NOT NULL,
	[orig_queue_id] [int] NULL,
	[display_desc] [varchar](50) NULL,
 CONSTRAINT [PK_queues] PRIMARY KEY NONCLUSTERED 
(
	[queue] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

GO
/****** Object:  Table [dbo].[service_goals]    Script Date: 10/02/2008 18:10:55 ******/

GO

GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[service_goals](
	[goal_id] [int] IDENTITY(1,1) NOT NULL,
	[log_object_id] [int] NOT NULL,
	[goal_desc] [char](30) NULL,
	[goal_sec] [int] NOT NULL,
 CONSTRAINT [PK_service_goalsNEW] PRIMARY KEY NONCLUSTERED 
(
	[goal_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

GO
/****** Object:  Table [dbo].[agent_info]    Script Date: 10/02/2008 18:09:51 ******/

GO

GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[agent_info](
	[Agent_id] [int] IDENTITY(1,1) NOT NULL,
	[Agent_name] [varchar](50) NOT NULL,
	[is_active] [bit] NULL,
	[log_object_id] [int] NOT NULL,
	[orig_agent_id] [varchar](50) NOT NULL,
 CONSTRAINT [PK_agent_info] PRIMARY KEY NONCLUSTERED 
(
	[Agent_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

GO
/****** Object:  Table [dbo].[log_object_add_hours]    Script Date: 10/02/2008 18:10:27 ******/

GO

GO
CREATE TABLE [dbo].[log_object_add_hours](
	[log_object_id] [int] NOT NULL,
	[datetime_from] [smalldatetime] NOT NULL,
	[datetime_to] [smalldatetime] NOT NULL,
	[add_hours] [int] NOT NULL,
 CONSTRAINT [PK_log_object_add_hours] PRIMARY KEY CLUSTERED 
(
	[log_object_id] ASC,
	[datetime_from] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[log_object_detail]    Script Date: 10/02/2008 18:10:30 ******/

GO

GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[log_object_detail](
	[log_object_id] [int] NOT NULL,
	[detail_id] [int] NOT NULL,
	[detail_desc] [varchar](50) NOT NULL,
	[proc_name] [varchar](50) NULL,
	[int_value] [int] NULL,
	[date_value] [smalldatetime] NULL,
 CONSTRAINT [PK_log_object_detail] PRIMARY KEY CLUSTERED 
(
	[log_object_id] ASC,
	[detail_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

GO
/****** Object:  Table [dbo].[agent_states]    Script Date: 10/02/2008 18:10:16 ******/

GO

GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[agent_states](
	[state_id] [int] IDENTITY(1,1) NOT NULL,
	[log_object_id] [int] NOT NULL,
	[orig_state_id] [int] NOT NULL,
	[state_name] [varchar](255) NULL,
	[is_paus] [bit] NOT NULL CONSTRAINT [DF_agent_states_is_paus]  DEFAULT (0),
	[is_wrap] [bit] NOT NULL CONSTRAINT [DF_agent_states_is_wrap]  DEFAULT (0),
	[is_admin] [bit] NOT NULL CONSTRAINT [DF_agent_states_is_admin]  DEFAULT (0),
	[is_active] [bit] NOT NULL CONSTRAINT [DF_agent_states_is_active]  DEFAULT (0),
	[changed_by] [int] NULL,
	[changed_date] [smalldatetime] NULL,
 CONSTRAINT [PK_agent_states] PRIMARY KEY CLUSTERED 
(
	[state_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

GO
/****** Object:  ForeignKey [FK_acd_type_detail_acd_type]    Script Date: 10/02/2008 18:09:41 ******/
ALTER TABLE [dbo].[acd_type_detail]  WITH CHECK ADD  CONSTRAINT [FK_acd_type_detail_acd_type] FOREIGN KEY([acd_type_id])
REFERENCES [dbo].[acd_type] ([acd_type_id])
GO
ALTER TABLE [dbo].[acd_type_detail] CHECK CONSTRAINT [FK_acd_type_detail_acd_type]
GO
/****** Object:  ForeignKey [FK_agent_by_day_logg_agent_info]    Script Date: 10/02/2008 18:09:48 ******/
ALTER TABLE [dbo].[agent_by_day_logg]  WITH CHECK ADD  CONSTRAINT [FK_agent_by_day_logg_agent_info] FOREIGN KEY([agent_id])
REFERENCES [dbo].[agent_info] ([Agent_id])
GO
ALTER TABLE [dbo].[agent_by_day_logg] CHECK CONSTRAINT [FK_agent_by_day_logg_agent_info]
GO
/****** Object:  ForeignKey [FK_agent_info_log_object]    Script Date: 10/02/2008 18:09:51 ******/
ALTER TABLE [dbo].[agent_info]  WITH CHECK ADD  CONSTRAINT [FK_agent_info_log_object] FOREIGN KEY([log_object_id])
REFERENCES [dbo].[log_object] ([log_object_id])
GO
ALTER TABLE [dbo].[agent_info] CHECK CONSTRAINT [FK_agent_info_log_object]
GO
/****** Object:  ForeignKey [FK_agent_logg_agent_info]    Script Date: 10/02/2008 18:09:59 ******/
ALTER TABLE [dbo].[agent_logg]  WITH CHECK ADD  CONSTRAINT [FK_agent_logg_agent_info] FOREIGN KEY([agent_id])
REFERENCES [dbo].[agent_info] ([Agent_id])
GO
ALTER TABLE [dbo].[agent_logg] CHECK CONSTRAINT [FK_agent_logg_agent_info]
GO
/****** Object:  ForeignKey [FK_agent_logg_queues]    Script Date: 10/02/2008 18:09:59 ******/
ALTER TABLE [dbo].[agent_logg]  WITH CHECK ADD  CONSTRAINT [FK_agent_logg_queues] FOREIGN KEY([queue])
REFERENCES [dbo].[queues] ([queue])
GO
ALTER TABLE [dbo].[agent_logg] CHECK CONSTRAINT [FK_agent_logg_queues]
GO
/****** Object:  ForeignKey [FK_agent_state_logg_agent_info]    Script Date: 10/02/2008 18:10:10 ******/
ALTER TABLE [dbo].[agent_state_logg]  WITH CHECK ADD  CONSTRAINT [FK_agent_state_logg_agent_info] FOREIGN KEY([agent_id])
REFERENCES [dbo].[agent_info] ([Agent_id])
GO
ALTER TABLE [dbo].[agent_state_logg] CHECK CONSTRAINT [FK_agent_state_logg_agent_info]
GO
/****** Object:  ForeignKey [FK_agent_state_logg_agent_states]    Script Date: 10/02/2008 18:10:10 ******/
ALTER TABLE [dbo].[agent_state_logg]  WITH CHECK ADD  CONSTRAINT [FK_agent_state_logg_agent_states] FOREIGN KEY([state_id])
REFERENCES [dbo].[agent_states] ([state_id])
GO
ALTER TABLE [dbo].[agent_state_logg] CHECK CONSTRAINT [FK_agent_state_logg_agent_states]
GO
/****** Object:  ForeignKey [FK_agent_states_log_object]    Script Date: 10/02/2008 18:10:16 ******/
ALTER TABLE [dbo].[agent_states]  WITH CHECK ADD  CONSTRAINT [FK_agent_states_log_object] FOREIGN KEY([log_object_id])
REFERENCES [dbo].[log_object] ([log_object_id])
GO
ALTER TABLE [dbo].[agent_states] CHECK CONSTRAINT [FK_agent_states_log_object]
GO
/****** Object:  ForeignKey [FK_goal_results_queues]    Script Date: 10/02/2008 18:10:21 ******/
ALTER TABLE [dbo].[goal_results]  WITH CHECK ADD  CONSTRAINT [FK_goal_results_queues] FOREIGN KEY([queue])
REFERENCES [dbo].[queues] ([queue])
GO
ALTER TABLE [dbo].[goal_results] CHECK CONSTRAINT [FK_goal_results_queues]
GO
/****** Object:  ForeignKey [FK_goal_results_service_goals]    Script Date: 10/02/2008 18:10:21 ******/
ALTER TABLE [dbo].[goal_results]  WITH CHECK ADD  CONSTRAINT [FK_goal_results_service_goals] FOREIGN KEY([goal_id])
REFERENCES [dbo].[service_goals] ([goal_id])
GO
ALTER TABLE [dbo].[goal_results] CHECK CONSTRAINT [FK_goal_results_service_goals]
GO
/****** Object:  ForeignKey [FK_log_object_acd_type]    Script Date: 10/02/2008 18:10:25 ******/
ALTER TABLE [dbo].[log_object]  WITH CHECK ADD  CONSTRAINT [FK_log_object_acd_type] FOREIGN KEY([acd_type_id])
REFERENCES [dbo].[acd_type] ([acd_type_id])
GO
ALTER TABLE [dbo].[log_object] CHECK CONSTRAINT [FK_log_object_acd_type]
GO
/****** Object:  ForeignKey [FK_log_object_add_hours_log_object]    Script Date: 10/02/2008 18:10:27 ******/
ALTER TABLE [dbo].[log_object_add_hours]  WITH CHECK ADD  CONSTRAINT [FK_log_object_add_hours_log_object] FOREIGN KEY([log_object_id])
REFERENCES [dbo].[log_object] ([log_object_id])
GO
ALTER TABLE [dbo].[log_object_add_hours] CHECK CONSTRAINT [FK_log_object_add_hours_log_object]
GO
/****** Object:  ForeignKey [FK_log_object_detail_log_object]    Script Date: 10/02/2008 18:10:30 ******/
ALTER TABLE [dbo].[log_object_detail]  WITH CHECK ADD  CONSTRAINT [FK_log_object_detail_log_object] FOREIGN KEY([log_object_id])
REFERENCES [dbo].[log_object] ([log_object_id])
GO
ALTER TABLE [dbo].[log_object_detail] CHECK CONSTRAINT [FK_log_object_detail_log_object]
GO
/****** Object:  ForeignKey [FK_project_logg_queues]    Script Date: 10/02/2008 18:10:33 ******/
ALTER TABLE [dbo].[project_logg]  WITH CHECK ADD  CONSTRAINT [FK_project_logg_queues] FOREIGN KEY([queue])
REFERENCES [dbo].[queues] ([queue])
GO
ALTER TABLE [dbo].[project_logg] CHECK CONSTRAINT [FK_project_logg_queues]
GO
/****** Object:  ForeignKey [FK_queue_by_day_logg_queues]    Script Date: 10/02/2008 18:10:41 ******/
ALTER TABLE [dbo].[queue_by_day_logg]  WITH CHECK ADD  CONSTRAINT [FK_queue_by_day_logg_queues] FOREIGN KEY([queue])
REFERENCES [dbo].[queues] ([queue])
GO
ALTER TABLE [dbo].[queue_by_day_logg] CHECK CONSTRAINT [FK_queue_by_day_logg_queues]
GO
/****** Object:  ForeignKey [FK_queue_logg_queues]    Script Date: 10/02/2008 18:10:50 ******/
ALTER TABLE [dbo].[queue_logg]  WITH CHECK ADD  CONSTRAINT [FK_queue_logg_queues] FOREIGN KEY([queue])
REFERENCES [dbo].[queues] ([queue])
GO
ALTER TABLE [dbo].[queue_logg] CHECK CONSTRAINT [FK_queue_logg_queues]
GO
/****** Object:  ForeignKey [FK_queues_log_object]    Script Date: 10/02/2008 18:10:53 ******/
ALTER TABLE [dbo].[queues]  WITH CHECK ADD  CONSTRAINT [FK_queues_log_object] FOREIGN KEY([log_object_id])
REFERENCES [dbo].[log_object] ([log_object_id])
GO
ALTER TABLE [dbo].[queues] CHECK CONSTRAINT [FK_queues_log_object]
GO
/****** Object:  ForeignKey [FK_service_goals_log_object1]    Script Date: 10/02/2008 18:10:56 ******/
ALTER TABLE [dbo].[service_goals]  WITH CHECK ADD  CONSTRAINT [FK_service_goals_log_object1] FOREIGN KEY([log_object_id])
REFERENCES [dbo].[log_object] ([log_object_id])
GO
ALTER TABLE [dbo].[service_goals] CHECK CONSTRAINT [FK_service_goals_log_object1]
GO
