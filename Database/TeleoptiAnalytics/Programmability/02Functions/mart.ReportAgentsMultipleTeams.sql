IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[ReportAgentsMultipleTeams]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[ReportAgentsMultipleTeams]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =======================================================
-- Author:		JN
-- Create date: 2011-10-21
--				2012-01-09 Added BU
-- Description:	Returns the agents relevant for reporting,
--				based on chosen filters and permissions.
--				Teams and groups parameters are sets of items.
-- =======================================================
CREATE FUNCTION [mart].[ReportAgentsMultipleTeams]
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
	@report_id int,
	@business_unit_code uniqueidentifier
)
RETURNS
@agents TABLE
(
	person_id int 
)

As
BEGIN 

	IF (mart.GroupPageCodeIsBusinessHierarchy(@group_page_code) = 1)
	BEGIN
		-- Business hierarchy
		INSERT INTO @agents
			SELECT 
				aoa.id 
			FROM 
				mart.AllOwnedAgents(@person_code, @report_id, @business_unit_code) aoa
			INNER JOIN 
				mart.PersonCodeToId(@agent_code, @date_from, @date_to, @site_id, @team_set) AS pcti
			ON 
				pcti.id = aoa.id
	END
	ELSE IF (mart.GroupPageCodeIsBusinessHierarchy(@group_page_code) = 0)
	BEGIN
		-- Grouping
		INSERT INTO @agents
			SELECT 
				aoa.id 
			FROM 
				mart.AllOwnedAgents(@person_code, @report_id, @business_unit_code) aoa
			INNER JOIN 
				mart.bridge_group_page_person p 
			ON 
				p.person_id = aoa.id
			INNER JOIN 
				mart.dim_group_page gp 
			ON 
				gp.group_page_id = p.group_page_id
			INNER JOIN 
				mart.PersonCodeToId(@group_page_agent_code, @date_from, @date_to, @site_id, @team_set) AS pcti
			ON 
				pcti.id = aoa.id
			WHERE 
				gp.group_page_code = @group_page_code AND
				gp.group_id IN (SELECT id FROM mart.SplitStringInt(@group_page_group_set))
	END
	
	RETURN
	
END

GO