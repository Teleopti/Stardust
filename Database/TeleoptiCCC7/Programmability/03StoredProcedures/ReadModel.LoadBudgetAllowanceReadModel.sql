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
--				2015-07-21  RobW			Removed slow join to PersonPeriodWithEndDate, instead use PersonPeriod table now that it has enddate
--				2017-12-04	RobinK			Changed to nolock for joined tables
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
	INNER JOIN  dbo.PersonPeriod t with (nolock) 
		ON t.parent = sp.PersonId
		AND t.budgetgroup=@BudgetGroupId
		AND sp.BelongsToDate BETWEEN t.startdate AND t.enddate
			AND EXISTS (
			SELECT DISTINCT Absence
			FROM dbo.budgetabsencecollection bac with (nolock)
			INNER JOIN dbo.customshrinkage cs with (nolock)
				ON cs.Id = bac.CustomShrinkage AND cs.Parent=@BudgetGroupId
				AND bac.Absence=sp.PayloadId
				)
	INNER JOIN Person p with (nolock) ON p.Id = t.Parent AND p.IsDeleted = 0
	WHERE sp.ScenarioId=@ScenarioId
	AND sp.BelongsToDate BETWEEN @DateFrom AND @DateTo

	GROUP BY sp.BelongsToDate,sp.PayloadId

END

GO

