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
			FROM PartTimePercentage i WITH(NOLOCK)
			INNER JOIN #Remids on #Remids.remid = i.Id
			UNION
			SELECT i.Id, i.Name
			FROM RuleSetBag i WITH(NOLOCK)
			INNER JOIN #Remids on #Remids.remid = i.Id
			UNION
			SELECT i.Id, i.Name
			FROM Contract i WITH(NOLOCK)
			INNER JOIN #Remids on #Remids.remid = i.Id
			UNION
			SELECT i.Id, i.Name
			FROM ContractSchedule i WITH(NOLOCK)
			INNER JOIN #Remids on #Remids.remid = i.Id
			UNION
			SELECT i.Id, i.Name
			FROM BudgetGroup i WITH(NOLOCK)
			INNER JOIN #Remids on #Remids.remid = i.Id
			UNION
			SELECT i.Id, i.Name
			FROM Skill i WITH(NOLOCK)
			INNER JOIN #Remids on #Remids.remid = i.Id
			) as n ON g.GroupId = n.Id

if exists(select top(1) a.id from Team a inner join #Remids on a.id=#Remids.remid)
	begin
		UPDATE [ReadModel].[GroupingReadOnly]
		SET GroupName = s.Name + '/' + t.Name, SiteId = t.Site
		FROM [ReadModel].[GroupingReadOnly]
		INNER JOIN Team t WITH(NOLOCK) ON GroupId = t.Id
		INNER JOIN Site s WITH(NOLOCK) ON s.Id = t.Site
		INNER JOIN #Remids ids ON t.Id = ids.remid
	end
	
if exists(select top(1) a.id from Site a inner join #Remids on a.id=#Remids.remid)
	begin
		UPDATE [ReadModel].[GroupingReadOnly]
		SET GroupName = s.Name + '/' + t.Name
		FROM [ReadModel].[GroupingReadOnly]
		INNER JOIN Team t WITH(NOLOCK) ON GroupId = t.Id
		INNER JOIN Site s WITH(NOLOCK) ON s.Id = t.Site
		INNER JOIN #Remids ids ON s.Id = ids.remid
	end

END
GO