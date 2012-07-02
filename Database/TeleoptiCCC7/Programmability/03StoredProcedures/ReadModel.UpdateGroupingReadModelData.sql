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
	FROM [ReadModel].[GroupingReadOnly] INNER JOIN PartTimePercentage  
	ON PartTimePercentage.Id = GroupId  
	INNER JOIN #Remids on #Remids.remid = GroupId 
	
	UPDATE [ReadModel].[GroupingReadOnly]
	SET GroupName  = Name
	FROM [ReadModel].[GroupingReadOnly] INNER JOIN RuleSetBag  
	ON RuleSetBag.Id = GroupId 
	INNER JOIN #Remids on #Remids.remid = GroupId 
	
	UPDATE [ReadModel].[GroupingReadOnly]
	SET GroupName  = Name
	FROM [ReadModel].[GroupingReadOnly] INNER JOIN Contract  
	ON  Contract.Id = GroupId 
	INNER JOIN #Remids on #Remids.remid = GroupId

	UPDATE [ReadModel].[GroupingReadOnly]
	SET GroupName  = Name
	FROM [ReadModel].[GroupingReadOnly] INNER JOIN ContractSchedule  
	ON  ContractSchedule.Id = GroupId 
	INNER JOIN #Remids on #Remids.remid = GroupId
	
	UPDATE [ReadModel].[GroupingReadOnly]
	SET GroupName  = Name
	FROM [ReadModel].[GroupingReadOnly] INNER JOIN BudgetGroup  
	ON  BudgetGroup.Id = GroupId 
	INNER JOIN #Remids on #Remids.remid = GroupId

	UPDATE [ReadModel].[GroupingReadOnly]
	SET GroupName  = Name
	FROM [ReadModel].[GroupingReadOnly] INNER JOIN Skill  
	ON  Skill.Id = GroupId 
	INNER JOIN #Remids on #Remids.remid = GroupId

	UPDATE [ReadModel].[GroupingReadOnly]
	SET GroupName  =  t.Name + ' ' + s.Name 
	FROM [ReadModel].[GroupingReadOnly] INNER JOIN PersonPeriod pp ON pp.Id = GroupId 
	INNER JOIN Team t ON pp.Team = t.Id
	INNER JOIN Site s ON s.Id = t.Site
	INNER JOIN #Remids on #Remids.remid = GroupId
	WHERE t.IsDeleted = 0 AND s.IsDeleted = 0
	
END
GO