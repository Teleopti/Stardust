IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadBudgetAllowanceReadModel]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadBudgetAllowanceReadModel]
GO

-- =============================================
-- Author:		RobinK
-- Create date: 2011-09-26
-- Description:	Loads the read model for allowance for a budget group
-- ChangeLog:	Date		Who				Description	
--				2013-05-23	ErikS			Added distinct to disregard duplicate rows in ReadModel
--				2013-05-24	DeeFlex & ErikS	Improved last change
--				2014-10-06	Ola				Removed temptable to reduce writes, reads and cpu usage (bug #30425 )
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

	SELECT
		sp.PayloadId,
		sp.BelongsToDate,
		SUM(sp.ContractTime) as TotalContractTime,
		Count(*) as HeadCounts
	FROM ReadModel.ScheduleProjectionReadOnly sp
	INNER JOIN  dbo.PersonPeriodWithEndDate t	
		ON t.parent = sp.PersonId
		AND t.budgetgroup=@BudgetGroupId
		AND sp.BelongsToDate BETWEEN t.startdate AND t.enddate
			AND EXISTS (
			SELECT DISTINCT Absence
			FROM dbo.budgetabsencecollection bac
			INNER JOIN dbo.customshrinkage cs
				ON cs.Id = bac.CustomShrinkage AND cs.Parent=@BudgetGroupId
				AND bac.Absence=sp.PayloadId
				)
	INNER JOIN Person p ON p.Id = t.Parent AND p.IsDeleted = 0
	WHERE sp.ScenarioId=@ScenarioId
	AND sp.BelongsToDate BETWEEN @DateFrom AND @DateTo

	GROUP BY sp.BelongsToDate,sp.PayloadId

END

GO

