----------------  
--Name: Real Team
--Desc: Modified the actual agent state table 
----------------  

Alter table [RTA].[ActualAgentState] ADD TeamId uniqueidentifier NULL , SiteId uniqueidentifier NULL
