-- Do this if check because some customers already have this unique constraint
IF OBJECT_ID('dbo.UQ_BudgetDay', 'UQ') IS NULL 
BEGIN
	-- Delete potential duplicates for budget days
	WITH cte AS (
	  SELECT UpdatedOn, [Day], Scenario, BudgetGroup, row_number() OVER(PARTITION BY [Day], Scenario, BudgetGroup 
		ORDER BY UpdatedOn DESC) AS [rn]
	  FROM BudgetDay
	)
	DELETE cte WHERE [rn] > 1

	ALTER TABLE dbo.BudgetDay ADD CONSTRAINT UQ_BudgetDay UNIQUE
		(
		BudgetGroup,
		Scenario,
		Day
		)
END

GO