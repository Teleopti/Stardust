IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadBudgetAllowanceReadModel]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadBudgetAllowanceReadModel]
GO

-- =============================================
-- Author:		RobinK
-- Create date: 2011-09-26
-- Description:	Lads the read model for allowance for a budget group
-- ChangeLog:	Date		Who				Description	
--				2013-05-23	ErikS			Added distinct to disregard duplicate rows in ReadModel
--				2013-05-24	DeeFlex & ErikS	Improved last change
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
		budgetgroupid uniqueidentifier null,
		startdate smalldatetime not null,
		enddate smalldatetime null
	);

	--Get the BudgetGroup of interest. Create a fictive EndDate for each PersonPeriod
	INSERT INTO #temppersonbudgetgroup
	SELECT
		personid		= p.id,
		budgetgroupid	= pp.budgetgroup,
		startdate		= pp.startdate,
		enddate			= pp.enddate
	FROM person p
	INNER JOIN dbo.PersonPeriodWithEndDate pp
		ON pp.parent=p.id
	WHERE p.isdeleted=0

	--Return calculated result to client
	SELECT
		sp.PayloadId,
		sp.BelongsToDate,
		SUM(sp.ContractTime) as TotalContractTime,
		Count(*) as HeadCounts
	FROM ReadModel.ScheduleProjectionReadOnly sp
	INNER JOIN #temppersonbudgetgroup t	
		ON t.PersonId = sp.PersonId
		AND t.budgetgroupid=@BudgetGroupId
		AND sp.BelongsToDate BETWEEN t.startdate AND t.enddate
		AND EXISTS (
			SELECT DISTINCT Absence
			FROM dbo.budgetabsencecollection bac
			INNER JOIN dbo.customshrinkage cs
				ON cs.Id = bac.CustomShrinkage AND cs.Parent=@BudgetGroupId
				AND bac.Absence=sp.PayloadId
				)
	WHERE sp.ScenarioId=@ScenarioId
	AND sp.BelongsToDate BETWEEN @DateFrom AND @DateTo
	GROUP BY sp.BelongsToDate,sp.PayloadId

END

GO

