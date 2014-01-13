IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadPersonForShiftTrade]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadPersonForShiftTrade]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jonas & Maria
-- Create date: 2013-09-06
-- Description:	Load PersonId´s for possible shift trades
-- =============================================
CREATE PROCEDURE [ReadModel].[LoadPersonForShiftTrade] 
	@shiftTradeDate smalldatetime, 
	@myTeamId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

    
	SELECT pp.Parent as PersonId, pp.Team as TeamId, t.Site as SiteId, s.BusinessUnit as BusinessUnitId
	FROM PersonPeriodWithEndDate pp
		INNER JOIN Team t ON pp.Team = t.id
		INNER JOIN Site s ON t.Site = s.Id
		INNER JOIN Person p ON pp.Parent = p.Id
	WHERE p.WorkflowControlSet IS NOT NULL
		AND pp.Team = @myTeamId
		AND (@shiftTradeDate BETWEEN StartDate AND EndDate) 
		/* We should do the name search from a separate SP */
		--AND Parent in 
		--(
		--	SELECT Distinct personid FROM [ReadModel].[FindPerson] 
		--	WHERE SearchValue like '%' 
		--		AND (SearchType in ('FirstName','LastName','EmployeeNumber'))
		--)
END

GO


