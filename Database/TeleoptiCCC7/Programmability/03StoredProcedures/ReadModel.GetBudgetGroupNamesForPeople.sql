IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[GetBudgetGroupNamesForPeople]') AND type in (N'P', N'PC'))
DROP PROCEDURE  [ReadModel].[GetBudgetGroupNamesForPeople]
GO
-- =============================================
-- Author:		<CodeMonkeys>
-- Create date: <2017-10-24>
-- Description:	<Get Budget Group Names for people>
-- =============================================

CREATE PROCEDURE ReadModel.GetBudgetGroupNamesForPeople 
	@PersonIds varchar(max),
	@StartDate datetime
AS
BEGIN
	SET NOCOUNT ON;

	CREATE TABLE #ids (
		person uniqueidentifier NOT NULL
	);

	INSERT INTO #ids SELECT * FROM SplitStringString(@PersonIds)

	SELECT PersonId, SearchValue as 'BudgetGroupName' FROM [ReadModel].[FindPerson] as p with (NOLOCK)
	INNER JOIN #ids ids ON p.PersonId = ids.person
	WHERE StartDateTime <= @StartDate AND @StartDate <= EndDateTime 
	AND SearchType = 'BudgetGroup'
END
GO