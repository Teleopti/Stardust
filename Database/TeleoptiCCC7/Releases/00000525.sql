SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Fan Zhang, Zhiping Lan, Yanyi Wan
-- Create date: 2015-02-05
-- Description:	Filter schedule with search support.
-- =============================================
CREATE PROCEDURE [ReadModel].[LoadPersonForScheduleSearch] 
	-- teamIdList should be comma separated uids for team.	
	@scheduleDate smalldatetime, 
	@teamIdList varchar(max),
	@name nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @namesearch nvarchar(max);
	set @namesearch = '%' + @name + '%';
	
	DECLARE @teamids table
	(
		Team uniqueidentifier
	)
	
	INSERT INTO @teamids
	SELECT * FROM dbo.SplitStringString(@teamIdList)

	SELECT pp.Parent as PersonId, pp.Team as TeamId, t.Site as SiteId, s.BusinessUnit as BusinessUnitId
	FROM PersonPeriodWithEndDate pp
		INNER JOIN Team t ON pp.Team = t.id
		INNER JOIN Site s ON t.Site = s.Id
		INNER JOIN Person p ON pp.Parent = p.Id
		INNER JOIN @teamids tids ON tids.Team = t.id
	WHERE p.WorkflowControlSet IS NOT NULL		
		AND (@scheduleDate BETWEEN StartDate AND EndDate) 	
		AND ((@namesearch is null or @namesearch = '') or (CONCAT(p.LastName, p.FirstName) like @namesearch) or (CONCAT(p.FirstName, p.LastName) like @namesearch)) 	
END
GO

