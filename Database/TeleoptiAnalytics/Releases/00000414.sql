TRUNCATE TABLE [mart].[fact_agent_queue]
GO
ALTER TABLE [mart].[fact_agent_queue] DROP CONSTRAINT [PK_fact_agent_queue]
GO
ALTER TABLE [mart].[fact_agent_queue] ADD  CONSTRAINT [PK_fact_agent_queue] PRIMARY KEY CLUSTERED 
(
[date_id] ASC,
[queue_id] ASC,
[interval_id] ASC,
[acd_login_id] ASC
)

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (414,'8.1.414') 
