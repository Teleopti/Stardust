
-- fix stuff from 820

DELETE dbo.RtaState
WHERE
StateCode = ' (00000000-0000-0000-0000-000000000000)'
GO

ALTER TABLE [dbo].[RtaState] DROP CONSTRAINT [UQ_StateCode_PlatFormTypeId_BusinessUnit]
GO

UPDATE dbo.[RtaState] 
SET
StateCode = REPLACE(REPLACE(StateCode, ' (00000000-0000-0000-0000-000000000000)', ''), ' (00000000-0000-0000-0000-000000000000)', '')
GO

DELETE dbo.[RtaState]
FROM dbo.RtaState s
WHERE s.Id NOT IN (
	SELECT MIN(CAST(s2.Id AS BINARY(16)))
	FROM dbo.[RtaState] s2 
	WHERE 
	s2.StateCode = s.StateCode AND
	s2.BusinessUnit = s.BusinessUnit
	)

ALTER TABLE [dbo].[RtaState] ADD CONSTRAINT [UQ_StateCode_BusinessUnit] UNIQUE NONCLUSTERED 
(
	[StateCode] ASC,
	[BusinessUnit] ASC
)
GO

