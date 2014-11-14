IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[UpdateGroupingReadModelData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[UpdateGroupingReadModelData]
GO

-- =============================================
-- Author:		<Asad Mirza>
-- Create date: <2012-05-18>
-- Description:	<This procedure will update the default pages >

-- =============================================

CREATE PROCEDURE ReadModel.UpdateGroupingReadModelData
@remids nvarchar(max)
AS
BEGIN
	
CREATE TABLE #Remids(remid uniqueidentifier)
INSERT INTO #Remids SELECT * FROM SplitStringString(@remids) 

UPDATE [ReadModel].[GroupingReadOnly]
SET GroupName  = Name
FROM [ReadModel].[GroupingReadOnly] g
INNER JOIN (SELECT i.Id, i.Name
			FROM PartTimePercentage i
			INNER JOIN #Remids on #Remids.remid = i.Id
			UNION
			SELECT i.Id, i.Name
			FROM RuleSetBag i
			INNER JOIN #Remids on #Remids.remid = i.Id
			UNION
			SELECT i.Id, i.Name
			FROM Contract i
			INNER JOIN #Remids on #Remids.remid = i.Id
			UNION
			SELECT i.Id, i.Name
			FROM ContractSchedule i
			INNER JOIN #Remids on #Remids.remid = i.Id
			UNION
			SELECT i.Id, i.Name
			FROM BudgetGroup i
			INNER JOIN #Remids on #Remids.remid = i.Id
			UNION
			SELECT i.Id, i.Name
			FROM Skill i
			INNER JOIN #Remids on #Remids.remid = i.Id
			UNION
			SELECT i.Id, i.Name
			FROM Skill i
			INNER JOIN #Remids on #Remids.remid = i.Id
			) as n ON g.GroupId = n.Id

END
GO