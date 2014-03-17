/****** Object:  StoredProcedure [ReadModel].[LoadUserDefinedTab]    Script Date: 01/27/2012 13:39:18 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadUserDefinedTab]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadUserDefinedTab]
GO

/****** Object:  StoredProcedure [ReadModel].[LoadUserDefinedTab]    Script Date: 01/27/2012 13:39:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Ola Håkansson
-- Create date: 2012-01-20
-- Description:	Loads (fast) the people in the user defined groups
-- exec ReadModel.LoadUserDefinedTab '056F325C-7234-46C7-BB72-9FDE00B36D0A', 'D93A93B5-E729-48FC-933E-9F74012D955D', '2011-01-01', 1053
-- exec ReadModel.LoadUserDefinedTab '41586380-a574-4220-81f6-9fe300b06055','D93A93B5-E729-48FC-933E-9F74012D955D', '2012-01-01', 1053
-- sp_executesql N'exec ReadModel.LoadUserDefinedTab @tabid=@p0, @bu=@p1,  @ondate=@p2, @culture=@p3',N'@p0 uniqueidentifier,@p1 uniqueidentifier,@p2 datetime,@p3 int',@p0='4BB56B89-5174-4FE2-AF8E-9B5E015BCEE4',@p1='928DD0BC-BF40-412E-B970-9B5E015AADEA',@p2='2012-05-18 00:00:00',@p3=2057
-- exec sp_executesql N'exec ReadModel.LoadUserDefinedTab @tabid=@p0, @bu=@p1,  @ondate=@p2, @culture=@p3',N'@p0 uniqueidentifier,@p1 uniqueidentifier,@p2 datetime,@p3 int',@p0='E0D0EEE9-AB80-4313-9014-9FA900C2F4BD',@p1='FC8E2ED7-F39C-4ADB-8108-9EE800B7FFB8',@p2='2012-05-18 00:00:00',@p3=1033
-- =============================================

CREATE PROCEDURE [ReadModel].[LoadUserDefinedTab]
@tabid uniqueidentifier,
@bu uniqueidentifier,
@ondate datetime,
@culture int 
AS
SET NOCOUNT ON
DECLARE @dynamicSQL nvarchar(4000)
DECLARE @collation nvarchar(50)
	
	--Set collation
SELECT @collation = cc.[Collation]
FROM ReadModel.CollationCulture() cc
inner join fn_helpcollations() fn
on cc.[Collation] = fn.name collate database_default
WHERE cc.Culture = @culture

--no match? Go for database_default
SELECT @collation = ISNULL(@collation,'database_default')
--re-init @dynamicSQL
SELECT @dynamicSQL=''

  CREATE TABLE #result(NodeId uniqueidentifier NOT NULL, ParentId uniqueidentifier NULL, 
  Node nvarchar(100) NULL, BusinessUnitId uniqueidentifier NULL, Level int)
 
  
  INSERT INTO #result SELECT Id, null, Name , BusinessUnit, 0
  FROM GroupPage WHERE IsDeleted = 0
  AND Id = @tabid
  
  INSERT INTO #result SELECT Id, Parent, PersonGroupBase.Name, BusinessUnitId, 1
  FROM RootPersonGroup INNER JOIN #result ON Parent = NodeId
  INNER JOIN PersonGroupBase ON  PersonGroupBase = PersonGroupBase.Id
  ORDER BY Name, Id
  ;
  with t1(id,parent,name,BusinessUnitId, level) as (SELECT Id, Parent, PersonGroupBase.Name, BusinessUnitId, Level +1
  FROM ChildPersonGroup INNER JOIN #result ON Parent = NodeId
  INNER JOIN PersonGroupBase ON  PersonGroupBase = PersonGroupBase.Id
  union all
  SELECT b.Id, c.Parent, b.Name, BusinessUnitId, Level +1
  FROM ChildPersonGroup c INNER JOIN t1 ON t1.id = c.Parent
  INNER JOIN PersonGroupBase b ON  PersonGroupBase = b.Id
  )
  INSERT INTO #result 
  select * from t1 ORDER BY Name, Id
 
 CREATE TABLE #endResult(NodeId uniqueidentifier NOT NULL, PersonId uniqueidentifier NULL, 
  TeamId uniqueidentifier NULL, SiteId uniqueidentifier NULL,  BusinessUnitId uniqueidentifier NULL,
  Node nvarchar(100) NULL, ParentId uniqueidentifier NULL, FirstName nvarchar(200), LastName nvarchar(200),
  EmploymentNumber nvarchar(200), Level int, Show bit)
  
  INSERT INTO #endResult
  SELECT NodeId, Person PersonId, null TeamId, null SiteId, BusinessUnitId,
  Node, ParentId, FirstName, LastName, EmploymentNumber, Level,
  case when ISNULL(TerminalDate, '2100-01-01') >= @ondate then 1 else 0 end Show
  FROM #result
  left join PersonGroup on NodeId = PersonGroup
  left JOIN Person ON Person = Person.Id WHERE Person.IsDeleted = 0 OR Person.IsDeleted is null
  ORDER BY level
	
	UPDATE #endResult
	SET TeamId = Team.Id, SiteId = Site.Id
	FROM #endResult
	INNER  JOIN PersonPeriodWithEndDate pp ON PersonId = pp.Parent AND StartDate <= @ondate AND EndDate >= @ondate
	INNER JOIN Team ON Team.Id = pp.Team
	INNER JOIN Site ON Site.id = Site and Site.BusinessUnit = @bu
	
  --SELECT * FROM #endResult where firstname = 'mikko' ORDER BY Level, Node, LastName, FirstName 
  
SELECT @dynamicSQL =	'SELECT * FROM #endResult ORDER BY Level, Node collate ' + @collation +
							', NodeId, LastName collate ' + @collation +
							', FirstName collate ' + @collation 
		
		EXEC sp_executesql @dynamicSQL
  
  DROP TABLE #result
  DROP TABLE #endResult
  


GO


