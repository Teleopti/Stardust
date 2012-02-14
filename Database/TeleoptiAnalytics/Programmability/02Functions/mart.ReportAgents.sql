IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[ReportAgents]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[ReportAgents]
GO

-- =======================================================
-- Author:		ME
-- Create date: 2011-01-26
--				2012-01-09 Mattias E: Added BU
-- Description:	Returns the agents relevant for reporting,
--				based on chosen filters and permissions
-- =======================================================
CREATE FUNCTION [mart].[ReportAgents]
(
	@date_from datetime,
	@date_to datetime,
	@group_page_code uniqueidentifier,
	@group_page_group_id int,
	@group_page_agent_code uniqueidentifier,
	@site_id int,
	@team_id int,
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
		SELECT aoa.id FROM mart.AllOwnedAgents(@person_code, @report_id, @business_unit_code) aoa
		INNER JOIN mart.PersonCodeToId(@agent_code, @date_from, @date_to, @site_id, @team_id) pcti
		ON pcti.id = aoa.id
	END
	ELSE IF (mart.GroupPageCodeIsBusinessHierarchy(@group_page_code) = 0)
	BEGIN
		-- Grouping
		INSERT INTO @agents
		SELECT aoa.id FROM mart.AllOwnedAgents(@person_code, @report_id, @business_unit_code) aoa
		INNER JOIN mart.bridge_group_page_person p ON p.person_id = aoa.id
		INNER JOIN mart.dim_group_page gp ON gp.group_page_id = p.group_page_id
		INNER JOIN mart.PersonCodeToId(@group_page_agent_code, @date_from, @date_to, @site_id, @team_id) pcti
			ON pcti.id = aoa.id
		WHERE gp.group_page_code = @group_page_code AND gp.group_id = @group_page_group_id   
	END
	
	RETURN
	
END