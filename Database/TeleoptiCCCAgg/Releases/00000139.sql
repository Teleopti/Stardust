----------------  
--Name: Anders F  
--Date: 2009-08-13  
--Desc: Because we cannot allow agents or queues with identical orig id's on the same log object.
----------------  
CREATE UNIQUE INDEX uix_orig_agent_id ON dbo.agent_info(log_object_id,orig_agent_id)
GO
CREATE UNIQUE INDEX uix_orig_queue_id ON dbo.queues(log_object_id,orig_queue_id)
GO
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (139,'7.0.139') 
