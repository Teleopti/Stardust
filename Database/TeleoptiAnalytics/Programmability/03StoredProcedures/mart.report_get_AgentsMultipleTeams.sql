IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_get_AgentsMultipleTeams]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_get_AgentsMultipleTeams]
GO


-- =======================================================
-- Author:		JN
-- Create date: 2011-10-21
--				2012-01-09 Added BU
-- Description:	Returns the agents relevant for reporting,
--				based on chosen filters and permissions.
--				Teams and groups parameters are sets of items.
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- 2013-12-18	DJ, tried to avoid unexpected tempdb load
-- =======================================================
CREATE PROCEDURE [mart].[report_get_AgentsMultipleTeams]
(
	@date_from datetime,
	@date_to datetime,
	@group_page_code uniqueidentifier,
	@group_page_group_set nvarchar(max),
	@group_page_agent_code uniqueidentifier,
	@site_id int,
	@team_set nvarchar(max),
	@agent_code uniqueidentifier,
	@person_code uniqueidentifier,
	@report_id uniqueidentifier,
	@business_unit_code uniqueidentifier
)

As
BEGIN 

	CREATE TABLE #AllOwnedAgents(id int)
	CREATE TABLE #PersonCodeToId(id int)
	
	INSERT INTO #AllOwnedAgents
	SELECT Id FROM mart.AllOwnedAgents(@person_code, @report_id, @business_unit_code)

	

	IF (mart.GroupPageCodeIsBusinessHierarchy(@group_page_code) = 1)
	BEGIN
			INSERT INTO #PersonCodeToId
			SELECT Id FROM mart.PersonCodeToId(@agent_code, @date_from, @date_to, @site_id, @team_set)

		-- Business hierarchy
			SELECT DISTINCT aoa.id 
			FROM #AllOwnedAgents aoa
			INNER JOIN #PersonCodeToId pcti
				ON pcti.id = aoa.id
	END
	ELSE IF (mart.GroupPageCodeIsBusinessHierarchy(@group_page_code) = 0)
	BEGIN
		
		INSERT INTO #PersonCodeToId
		SELECT Id FROM mart.PersonCodeToId(@group_page_agent_code, @date_from, @date_to, @site_id, @team_set)

		-- Grouping
			SELECT DISTINCT
				aoa.id 
			FROM 
				#AllOwnedAgents aoa
			INNER JOIN 
				mart.bridge_group_page_person p 
			ON 
				p.person_id = aoa.id
			INNER JOIN 
				mart.dim_group_page gp 
			ON 
				gp.group_page_id = p.group_page_id
			INNER JOIN #PersonCodeToId pcti
				ON pcti.id = aoa.id
			WHERE 
				gp.group_page_code = @group_page_code AND
				gp.group_id IN (SELECT id FROM mart.SplitStringInt(@group_page_group_set))
	END


END

GO