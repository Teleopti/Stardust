IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'dbo.GroupPageHierarchyCTE'))
DROP VIEW dbo.GroupPageHierarchyCTE
GO

-- recursive cte to draw GroupPage hierarchy
create view dbo.GroupPageHierarchyCTE
as

WITH GroupPageHierarchyCTE(level,PersonGroup,Parent,TabId)
AS (
	SELECT
		1,
		e.Id,
		e.Parent,
		e.TabId
	FROM GroupPageParent e
	WHERE Parent IS NULL

	UNION ALL

	SELECT
		cte.level+1,
		e.Id,
		e.Parent,
		cte.TabId
	FROM GroupPageParent e
	INNER JOIN GroupPageHierarchyCTE cte
		ON e.Parent = cte.PersonGroup
	WHERE e.Parent IS NOT NULL
)
select * from GroupPageHierarchyCTE
GO