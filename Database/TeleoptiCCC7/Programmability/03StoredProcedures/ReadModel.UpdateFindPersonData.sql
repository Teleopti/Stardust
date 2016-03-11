
/****** Object:  StoredProcedure [ReadModel].[UpdateFindPerson]    Script Date: 12/19/2011 14:51:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[UpdateFindPersonData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].UpdateFindPersonData
GO

/****** Object:  StoredProcedure [ReadModel].[UpdateFindPersonData]    Script Date: 12/19/2011 14:51:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [ReadModel].[UpdateFindPersonData]
@ids nvarchar(max)

-- =============================================
-- Author:		Ola
-- Create date: 2012-05-15
-- Description:	Updates the read model for finding persons
-- Change:		

-- =============================================
AS
-- exec [ReadModel].[UpdateFindPersonData] '82A49700-B5B2-4AB3-88B6-9EF500BD080C'
--SET NOCOUNT ON
CREATE TABLE #ids(id uniqueidentifier)
INSERT INTO #ids SELECT * FROM SplitStringString(@ids)

UPDATE [ReadModel].[FindPerson]
SET SearchValue = Name
FROM [ReadModel].[FindPerson] 
INNER JOIN PartTimePercentage ON PartTimePercentage.Id = SearchValueId 
INNER JOIN #ids on #ids.id = SearchValueId

UPDATE [ReadModel].[FindPerson]
SET SearchValue = Name
FROM [ReadModel].[FindPerson] 
INNER JOIN RuleSetBag ON RuleSetBag.Id = SearchValueId 
INNER JOIN #ids on #ids.id = SearchValueId

UPDATE [ReadModel].[FindPerson]
SET SearchValue = Name
FROM [ReadModel].[FindPerson] 
INNER JOIN Contract ON Contract.Id = SearchValueId 
INNER JOIN #ids on #ids.id = SearchValueId

UPDATE [ReadModel].[FindPerson]
SET SearchValue = Name
FROM [ReadModel].[FindPerson] 
INNER JOIN ContractSchedule ON ContractSchedule.Id = SearchValueId 
INNER JOIN #ids on #ids.id = SearchValueId

UPDATE [ReadModel].[FindPerson]
SET SearchValue = Name
FROM [ReadModel].[FindPerson] 
INNER JOIN BudgetGroup ON BudgetGroup.Id = SearchValueId 
INNER JOIN #ids on #ids.id = SearchValueId

UPDATE [ReadModel].[FindPerson]
SET SearchValue = Name
FROM [ReadModel].[FindPerson] 
INNER JOIN Skill ON Skill.Id = SearchValueId 
INNER JOIN #ids on #ids.id = SearchValueId


UPDATE [ReadModel].[FindPerson]
SET SearchValue =  s.Name + ' ' + t.Name 
FROM [ReadModel].[FindPerson] 
INNER JOIN PersonPeriod pp ON pp.Id = SearchValueId
INNER JOIN Team t ON pp.Team = t.Id
INNER JOIN #ids ids ON t.Id = ids.id
INNER JOIN Site s ON s.Id = t.Site
WHERE t.IsDeleted = 0 AND s.IsDeleted = 0


UPDATE [ReadModel].[FindPerson]
SET SearchValue =  s.Name + ' ' + t.Name 
FROM [ReadModel].[FindPerson] 
INNER JOIN PersonPeriod pp ON pp.Id = SearchValueId
INNER JOIN Team t ON pp.Team = t.Id
INNER JOIN Site s ON s.Id = t.Site
INNER JOIN #ids ids ON s.Id = ids.id
WHERE t.IsDeleted = 0 AND s.IsDeleted = 0


UPDATE [ReadModel].[FindPerson]
SET SearchValue = CASE SUBSTRING( ar.DescriptionText ,1 , 2 )
WHEN  'xx'    THEN ar.Name
 ELSE ar.DescriptionText
 END
FROM [ReadModel].[FindPerson] 
INNER JOIN ApplicationRole ar ON ar.Id = SearchValueId 
INNER JOIN #ids on #ids.id = SearchValueId
WHERE ar.IsDeleted = 0