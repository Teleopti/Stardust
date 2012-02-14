IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[PermittedTeams]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[PermittedTeams]
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		HG & JN
-- Create date: 2010-09-28
-- Description:	
-- Revisions:	2011-01-24 ME: Removed parameter
--				@agent_id since it is not used
------------------------------------------------

-- =============================================


CREATE FUNCTION [mart].[PermittedTeams]
(
	@person_code uniqueidentifier,
	@report_id int,
	@site_id int,
	@team_id int
)
RETURNS @teams TABLE (id int NOT NULL)
AS
BEGIN	

/*Get all agents/persons that user has permission to see. */
INSERT INTO @teams SELECT * FROM mart.AllOwnedTeams(@person_code, @report_id)

IF @team_id <> -2
           DELETE FROM @teams WHERE id <>@team_id
IF @site_id <> -2
           DELETE FROM @teams WHERE id NOT IN(SELECT team_id FROM mart.dim_team WHERE site_id=@site_id)

RETURN

END

GO


