
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
-- exec [ReadModel].[UpdateFindPersonData] 'AAB08A5E-0C0B-4981-B859-A34501025265,0AA78021-7A7B-4AEA-B6A9-A13A00C4F94A,BB76366B-0A8F-42CB-ADDF-A11C00F0F283,3B9ACF9A-6D3F-4C48-B0E7-A13A00C4CB7A,3A545D7E-A93B-4145-8CF9-A54800E0BBB8,59F19897-60D5-4530-BDC0-A14E00911F3E,FDE198E0-7BAE-4737-9EF1-A3570097D6FF,959FBF3F-10FF-439C-AE2D-A22D0088C36E'
--SET NOCOUNT ON
CREATE TABLE #ids(id uniqueidentifier)
INSERT INTO #ids SELECT * FROM SplitStringString(@ids)

if exists(select top(1) a.id from PartTimePercentage a inner join #ids on a.id=#ids.id)
begin
	UPDATE [ReadModel].[FindPerson]
	SET SearchValue = Name
	FROM [ReadModel].[FindPerson] fp
	INNER JOIN PartTimePercentage WITH(NOLOCK) ON PartTimePercentage.Id = SearchValueId
	INNER JOIN #ids on #ids.id = SearchValueId
end

if exists(select top(1) a.id from RuleSetBag a inner join #ids on a.id=#ids.id)
begin
	UPDATE [ReadModel].[FindPerson]
	SET SearchValue = Name
	FROM [ReadModel].[FindPerson] fp
	INNER JOIN RuleSetBag WITH(NOLOCK) ON RuleSetBag.Id = SearchValueId 
	INNER JOIN #ids on #ids.id = SearchValueId
end

if exists(select top(1) a.id from Contract a inner join #ids on a.id=#ids.id)
begin
	UPDATE [ReadModel].[FindPerson]
	SET SearchValue = Name
	FROM [ReadModel].[FindPerson] fp 
	INNER JOIN [Contract] WITH(NOLOCK) ON [Contract].Id = SearchValueId
	INNER JOIN #ids on #ids.id = SearchValueId
end

if exists(select top(1) a.id from ContractSchedule a inner join #ids on a.id=#ids.id)
begin
	UPDATE [ReadModel].[FindPerson]
	SET SearchValue = Name
	FROM [ReadModel].[FindPerson] fp
	INNER JOIN ContractSchedule WITH(NOLOCK) ON ContractSchedule.Id = SearchValueId 
	INNER JOIN #ids on #ids.id = SearchValueId
end

if exists(select top(1) a.id from BudgetGroup a inner join #ids on a.id=#ids.id)
begin
	UPDATE [ReadModel].[FindPerson]
	SET SearchValue = Name
	FROM [ReadModel].[FindPerson] fp
	INNER JOIN BudgetGroup WITH(NOLOCK) ON BudgetGroup.Id = SearchValueId 
	INNER JOIN #ids on #ids.id = SearchValueId
end

if exists(select top(1) a.id from Skill a inner join #ids on a.id=#ids.id)
begin
	UPDATE [ReadModel].[FindPerson]
	SET SearchValue = Name
	FROM [ReadModel].[FindPerson] fp
	INNER JOIN Skill WITH(NOLOCK) ON Skill.Id = fp.SearchValueId
	INNER JOIN #ids on #ids.id = Skill.Id
end

if exists(select top(1) a.id from Team a inner join #ids on a.id=#ids.id)
begin
	UPDATE [ReadModel].[FindPerson]
	SET SearchValue =  s.Name + ' ' + t.Name, SiteId = t.Site
	FROM [ReadModel].[FindPerson] 
	INNER JOIN Team t WITH(NOLOCK) ON TeamId = t.Id
	INNER JOIN Site s WITH(NOLOCK) ON s.Id = t.Site
	INNER JOIN #ids ids ON t.Id = ids.id
end

if exists(select top(1) a.id from Site a inner join #ids on a.id=#ids.id)
begin
	UPDATE [ReadModel].[FindPerson]
	SET SearchValue =  s.Name + ' ' + t.Name
	FROM [ReadModel].[FindPerson] 
	INNER JOIN Team t WITH(NOLOCK) ON TeamId = t.Id
	INNER JOIN Site s WITH(NOLOCK) ON s.Id = t.Site
	INNER JOIN #ids ids ON s.Id = ids.id
end

if exists(select top(1) a.id from ApplicationRole a inner join #ids on a.id=#ids.id)
begin
	UPDATE [ReadModel].[FindPerson]
	SET SearchValue = CASE SUBSTRING( ar.DescriptionText ,1 , 2 )
	WHEN  'xx'    THEN ar.Name
	 ELSE ar.DescriptionText
	 END
	FROM [ReadModel].[FindPerson] 
	INNER JOIN ApplicationRole ar WITH(NOLOCK) ON ar.Id = SearchValueId 
	INNER JOIN #ids on #ids.id = SearchValueId
	WHERE ar.IsDeleted = 0
end
