ALTER TABLE Readmodel.AgentState ALTER COLUMN EmploymentNumber NVARCHAR(50)

IF (SELECT COUNT(1) FROM ReadModel.AgentState) = 0
	INSERT INTO 
	ReadModel.AgentState (PersonId, FirstName, LastName, EmploymentNumber, IsDeleted) 
	SELECT Id, FirstName, LastName, EmploymentNumber, 1 FROM dbo.Person
ELSE
	UPDATE Readmodel.AgentState
	SET 
		FirstName = p.FirstName,
		LastName = p.LastName,
		EmploymentNumber = p.EmploymentNumber
	FROM Readmodel.AgentState a
	INNER JOIN dbo.Person p
		ON a.PersonId = p.Id