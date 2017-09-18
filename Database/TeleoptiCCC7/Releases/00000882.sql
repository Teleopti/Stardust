UPDATE Readmodel.AgentState
SET 
	FirstName = p.FirstName,
	LastName = p.LastName,
	EmploymentNumber = p.EmploymentNumber
FROM Readmodel.AgentState a
JOIN dbo.Person p
	ON a.PersonId = p.Id

UPDATE ReadModel.AgentState
SET
	TeamName = t.Name
FROM ReadModel.AgentState a
JOIN dbo.Team t
	on t.Id = a.TeamId

UPDATE ReadModel.AgentState
SET
	SiteName = s.Name
FROM ReadModel.AgentState a
JOIN dbo.Site s
	on s.Id = a.SiteId