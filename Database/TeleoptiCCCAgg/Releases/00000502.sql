
-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-05-08
-- Desc: Fixing PK's. VSTS #75391
-- [dbo].[agent_info] does not have a clustered index defined. This is needed for Azure compability Table: 
-----------------------------------------------------------

ALTER TABLE dbo.agent_by_day_logg
	DROP CONSTRAINT FK_agent_by_day_logg_agent_info
GO
ALTER TABLE dbo.agent_logg
	DROP CONSTRAINT FK_agent_logg_agent_info
GO
ALTER TABLE dbo.agent_state_logg
	DROP CONSTRAINT FK_agent_state_logg_agent_info
GO
ALTER TABLE dbo.agent_info
	DROP CONSTRAINT PK_agent_info
GO
ALTER TABLE dbo.agent_info ADD CONSTRAINT
	PK_agent_info PRIMARY KEY CLUSTERED 
	(
	Agent_id
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.agent_info SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.agent_state_logg ADD CONSTRAINT
	FK_agent_state_logg_agent_info FOREIGN KEY
	(
	agent_id
	) REFERENCES dbo.agent_info
	(
	Agent_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.agent_state_logg SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.agent_logg ADD CONSTRAINT
	FK_agent_logg_agent_info FOREIGN KEY
	(
	agent_id
	) REFERENCES dbo.agent_info
	(
	Agent_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.agent_logg SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.agent_by_day_logg ADD CONSTRAINT
	FK_agent_by_day_logg_agent_info FOREIGN KEY
	(
	agent_id
	) REFERENCES dbo.agent_info
	(
	Agent_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.agent_by_day_logg SET (LOCK_ESCALATION = TABLE)
GO

-----------------------------------------------------------
-- [dbo].[queues] does not have a clustered index defined. This is needed for Azure compability Table: 

ALTER TABLE dbo.agent_logg
	DROP CONSTRAINT FK_agent_logg_queues
GO
ALTER TABLE dbo.goal_results
	DROP CONSTRAINT FK_goal_results_queues
GO
ALTER TABLE dbo.project_logg
	DROP CONSTRAINT FK_project_logg_queues
GO
ALTER TABLE dbo.queue_by_day_logg
	DROP CONSTRAINT FK_queue_by_day_logg_queues
GO
ALTER TABLE dbo.queue_logg
	DROP CONSTRAINT FK_queue_logg_queues
GO
ALTER TABLE dbo.queues
	DROP CONSTRAINT PK_queues
GO
ALTER TABLE dbo.queues ADD CONSTRAINT
	PK_queues PRIMARY KEY CLUSTERED 
	(
	queue
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.queues SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.queue_logg ADD CONSTRAINT
	FK_queue_logg_queues FOREIGN KEY
	(
	queue
	) REFERENCES dbo.queues
	(
	queue
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.queue_logg SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.queue_by_day_logg ADD CONSTRAINT
	FK_queue_by_day_logg_queues FOREIGN KEY
	(
	queue
	) REFERENCES dbo.queues
	(
	queue
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.queue_by_day_logg SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.project_logg ADD CONSTRAINT
	FK_project_logg_queues FOREIGN KEY
	(
	queue
	) REFERENCES dbo.queues
	(
	queue
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.project_logg SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.goal_results ADD CONSTRAINT
	FK_goal_results_queues FOREIGN KEY
	(
	queue
	) REFERENCES dbo.queues
	(
	queue
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.goal_results SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.agent_logg ADD CONSTRAINT
	FK_agent_logg_queues FOREIGN KEY
	(
	queue
	) REFERENCES dbo.queues
	(
	queue
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.agent_logg SET (LOCK_ESCALATION = TABLE)
GO
-----------------------------------------------------------
-- [dbo].[service_goals] does not have a clustered index defined. This is needed for Azure compability Table: 

ALTER TABLE dbo.goal_results
	DROP CONSTRAINT FK_goal_results_service_goals
GO
ALTER TABLE dbo.service_goals
	DROP CONSTRAINT PK_service_goals
GO
ALTER TABLE dbo.service_goals ADD CONSTRAINT
	PK_service_goals PRIMARY KEY CLUSTERED 
	(
	goal_id
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.service_goals SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.goal_results ADD CONSTRAINT
	FK_goal_results_service_goals FOREIGN KEY
	(
	goal_id
	) REFERENCES dbo.service_goals
	(
	goal_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.goal_results SET (LOCK_ESCALATION = TABLE)
GO

-----------------------------------------------------------
-- [dbo].[t_agent_reason] does not have a clustered index defined. This is needed for Azure compability Table: 
ALTER TABLE dbo.t_agent_reason ADD CONSTRAINT
	PK_t_agent_reason PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.t_agent_reason SET (LOCK_ESCALATION = TABLE)
GO


-----------------------------------------------------------
-- [dbo].[t_agent_report] does not have a clustered index defined. This is needed for Azure compability
IF EXISTS (SELECT Name FROM sysindexes WHERE Name = 'DATE')
BEGIN
	DROP INDEX DATE ON dbo.t_agent_report
END
GO
ALTER TABLE dbo.t_agent_report
	DROP CONSTRAINT PK_t_agent_report
GO
ALTER TABLE dbo.t_agent_report ADD CONSTRAINT
	PK_t_agent_report PRIMARY KEY CLUSTERED 
	(
	id_nr,
	date,
	interval,
	agt_id,
	acd_dn_number
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.t_agent_report SET (LOCK_ESCALATION = TABLE)
GO


