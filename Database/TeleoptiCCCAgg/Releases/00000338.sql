--AndersF 20111021 Cope with unicode agent names

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_update_stat_54]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_update_stat_54]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_SERGEL_insert_service_logg]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_SERGEL_insert_service_logg]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_insert_queue_logg_54]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_insert_queue_logg_54]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_insert_goal_results_54]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_insert_goal_results_54]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_insert_agent_logg_54]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_insert_agent_logg_54]
GO


alter table dbo.agent_info alter column agent_name nvarchar(50) not null

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[agent_info]') AND name = N'uix_orig_agent_id')
DROP INDEX [uix_orig_agent_id] ON [dbo].[agent_info] WITH ( ONLINE = OFF )
GO
alter table dbo.agent_info alter column orig_agent_id nvarchar(50) not null
GO
CREATE UNIQUE NONCLUSTERED INDEX [uix_orig_agent_id] ON [dbo].[agent_info] 
(
	[log_object_id] ASC,
	[orig_agent_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

alter table dbo.queues alter column orig_desc nvarchar(50) null
GO
alter table dbo.queues alter column display_desc nvarchar(50) null
GO
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (338,'7.1.338') 
