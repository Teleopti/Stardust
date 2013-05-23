IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadBudgetAllowanceReadModel]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadBudgetAllowanceReadModel]
GO

-- =============================================
-- Author:		RobinK
-- Create date: 2011-09-26
-- Description:	Lads the read model for allowance for a budget group
-- ChangeLog:	Date		Who		Description	
--				2013-05-23	Eka		Added distinct to disregard duplicate rows in ReadModel
-- =============================================

CREATE PROCEDURE [ReadModel].[LoadBudgetAllowanceReadModel]
(@BudgetGroupId		uniqueidentifier,
@ScenarioId			uniqueidentifier,
@DateFrom			smalldatetime,
@DateTo				smalldatetime
)
AS
BEGIN
	SET NOCOUNT ON;

	CREATE TABLE #temppersonbudgetgroup
	(
		personid uniqueidentifier not null,
		budgetgroupid uniqueidentifier not null,
		startdate smalldatetime not null,
		enddate smalldatetime null,
		leavingdate smalldatetime null
	);

	--Get the BudgetGroup of interest. Create a fictive EndDate for each PersonPeriod
	INSERT INTO #temppersonbudgetgroup
	SELECT
		personid		= p.id,
		budgetgroupid	= pp.budgetgroup,
		startdate		= ISNULL(pp.startdate,'1900-01-01'),
		enddate			= (
							SELECT TOP 1 DATEADD(d,-1,startdate)
							FROM personperiod
							WHERE parent=p.id
							AND startdate>pp.startdate
							ORDER BY startdate ASC
							),
		leavingdate		= p.terminaldate
	FROM person p
	INNER JOIN personperiod pp
		ON pp.parent=p.id
	WHERE p.isdeleted=0
	AND pp.budgetgroup=@BudgetGroupId

	--fix EndDate for all PersonPeriods
	UPDATE #temppersonbudgetgroup SET enddate='2059-12-31' WHERE enddate is null
	UPDATE #temppersonbudgetgroup SET enddate=leavingdate WHERE leavingdate<enddate
	DELETE FROM #temppersonbudgetgroup WHERE startdate>enddate
	
	--Return calculated result to client
	SELECT
		result.PayloadId,
		result.BelongsToDate,
		SUM(result.ContractTime) as TotalContractTime,
		MIN(result.InsertedOn) as LeastUpdate
	FROM (
			SELECT DISTINCT sp.PayloadId, sp.BelongsToDate, sp.ContractTime, sp.InsertedOn 
			FROM ReadModel.ScheduleProjectionReadOnly sp
			INNER JOIN #temppersonbudgetgroup t	
				ON t.PersonId = sp.PersonId
				AND sp.BelongsToDate BETWEEN t.startdate AND t.enddate	
			INNER JOIN dbo.budgetabsencecollection bac ON bac.Absence=sp.PayloadId 
			INNER JOIN dbo.customshrinkage cs ON cs.Id = bac.CustomShrinkage AND cs.Parent=@BudgetGroupId

			WHERE sp.ScenarioId=@ScenarioId
			AND sp.BelongsToDate BETWEEN @DateFrom AND @DateTo
		) result
	GROUP BY result.BelongsToDate,result.PayloadId
END

GO

