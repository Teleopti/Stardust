IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[PermittedTeamsMultipleTeams]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[PermittedTeamsMultipleTeams]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		JN
-- Create date: 2011-10-21
-- Description:	
-- Revisions:	
------------------------------------------------

-- =============================================


CREATE FUNCTION [mart].[PermittedTeamsMultipleTeams]
(
	@person_code uniqueidentifier,
	@report_id int,
	@site_id int,
	@team_set nvarchar(max)
)
RETURNS @teams TABLE (id int NOT NULL)
AS
BEGIN	

/*Get all agents/persons that user has permission to see. */
INSERT INTO @teams 
	SELECT * FROM mart.AllOwnedTeams(@person_code, @report_id)

IF @team_set IS NOT NULL AND(-2 NOT IN (SELECT id FROM mart.SplitStringInt(@team_set)))
	DELETE FROM @teams WHERE id NOT IN (SELECT id FROM mart.SplitStringInt(@team_set))
IF @site_id <> -2
	DELETE FROM @teams WHERE id NOT IN(SELECT team_id FROM mart.dim_team WHERE site_id=@site_id)

RETURN

END



GO


