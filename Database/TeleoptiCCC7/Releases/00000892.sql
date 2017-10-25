
MERGE INTO ReadModel.AgentState AS T
USING
(
	SELECT 
		Id, 
		FirstName, 
		LastName, 
		EmploymentNumber
	FROM 
		dbo.Person
) AS S (
	PersonId,
	FirstName,
	LastName,
	EmploymentNumber
) ON
	T.PersonId = S.PersonId
WHEN NOT MATCHED THEN
INSERT
(
	PersonId,
	FirstName,
	LastName,
	EmploymentNumber,
	IsDeleted
)
VALUES (
	S.PersonId,
	S.FirstName,
	S.LastName,
	S.EmploymentNumber,
	1
)
WHEN MATCHED THEN
UPDATE SET
	FirstName = S.FirstName,
	LastName = S.LastName,
	EmploymentNumber = S.EmploymentNumber
;
