-- Do this if check because some customers already have this unique constraint
IF OBJECT_ID('dbo.UQ_BudgetDay', 'UQ') IS NULL 
BEGIN
	-- Delete potential duplicates for budget days
	WITH cte AS (
	  SELECT Id, UpdatedOn, [Day], Scenario, BudgetGroup, row_number() OVER(PARTITION BY [Day], Scenario, BudgetGroup 
		ORDER BY UpdatedOn DESC) AS [rn]
	  FROM BudgetDay
	)
	SELECT * INTO #duplicates FROM cte WHERE [rn] > 1

	DELETE cesb
	FROM CustomEfficiencyShrinkageBudget cesb
	INNER JOIN #duplicates d ON d.Id = cesb.Parent
	
	DELETE csb
	FROM CustomShrinkageBudget csb
	INNER JOIN #duplicates d ON d.Id = csb.Parent

	DELETE bd
	FROM BudgetDay bd
	INNER JOIN #duplicates d ON bd.Id = d.Id
	
	DROP TABLE #duplicates

	ALTER TABLE dbo.BudgetDay ADD CONSTRAINT UQ_BudgetDay UNIQUE
		(
		BudgetGroup,
		Scenario,
		Day
		)
END

GO