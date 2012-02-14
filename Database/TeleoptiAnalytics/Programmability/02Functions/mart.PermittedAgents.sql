IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[PermittedAgents]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[PermittedAgents]
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		HG & JN
-- Create date: 2010-09-28
--				2012-01-09 Pass DEFAULT BU to AllOwnedAgents
-- Description:	
------------------------------------------------

-- =============================================


CREATE FUNCTION [mart].[PermittedAgents]
(
	@person_code uniqueidentifier,
	@report_id int,
	@site_id int,
	@team_id int,
	@agent_id int,
	@group_page_code uniqueidentifier,
	@group_page_group_id int,
	@group_page_agent_id int
)
RETURNS @agents TABLE (id int NOT NULL)
AS
BEGIN	

IF (mart.GroupPageCodeIsBusinessHierarchy(@group_page_code) = 0)
	BEGIN
		SET @agent_id = @group_page_agent_id
	END

/*Get all agents/persons that user has permission to see. */
INSERT INTO @agents SELECT * FROM mart.AllOwnedAgents(@person_code, @report_id, DEFAULT)

/*Remove other than selected*/
IF @agent_id <> -2
           DELETE FROM @agents WHERE id <> @agent_id
           
IF (@agent_id = -2) AND (@group_page_group_id IS NOT NULL)
BEGIN
           DELETE FROM @agents
           WHERE id NOT IN
                      (
                                 SELECT b.person_id
                                 FROM mart.dim_group_page gp
                                            INNER JOIN mart.bridge_group_page_person b ON gp.group_page_id = b.group_page_id
                                 WHERE gp.group_page_code = @group_page_code
                                            AND gp.group_id = @group_page_group_id       
                      )
END

RETURN

END

GO


