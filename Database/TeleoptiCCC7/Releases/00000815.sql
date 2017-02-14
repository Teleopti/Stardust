WITH cte AS (
  SELECT PersonId, row_number() OVER(PARTITION BY PersonId ORDER BY PersonId) AS [rn]
  FROM ReadModel.CurrentSchedule
)
DELETE cte WHERE [rn] > 1
GO

UPDATE ReadModel.CurrentSchedule SET Valid = 0
GO

ALTER TABLE ReadModel.CurrentSchedule ADD CONSTRAINT PK_PersonId PRIMARY KEY CLUSTERED (PersonId)
GO
