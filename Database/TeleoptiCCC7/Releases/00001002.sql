-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-06-27
-- Desc: Fixing PK's. VSTS #75391 
-----------------------------------------------------------
ALTER TABLE dbo.ReplyOptions ADD CONSTRAINT
	PK_ReplyOptions PRIMARY KEY NONCLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.ReplyOptions SET (LOCK_ESCALATION = TABLE)
GO
