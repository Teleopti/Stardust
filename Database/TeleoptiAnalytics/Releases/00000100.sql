/* 
Trunk initiated: 
Thu 05/07/2009 
01:49 PM
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: David Jonsson
--Date: 2009-05-11
--Desc: A duplicate FK found
----------------
ALTER TABLE [mart].[fact_agent_queue] DROP CONSTRAINT [FK_fact_agent_queue_dim_queue1]
 
GO 
 
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (100,'7.0.100') 
