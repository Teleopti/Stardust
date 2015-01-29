----------------  
--Name: Real Team
--Desc: Added index on teamId, SiteId and businessUnitID 
----------------  

CREATE INDEX IX_ActualAgentState_TeamID ON [RTA].[ActualAgentState](
	[TeamId]
)

CREATE INDEX IX_ActualAgentState_BusinessUnitID ON [RTA].[ActualAgentState](
	[BusinessUnitId]
)

CREATE INDEX IX_ActualAgentState_SiteID ON [RTA].[ActualAgentState](
	[SiteId]
)

