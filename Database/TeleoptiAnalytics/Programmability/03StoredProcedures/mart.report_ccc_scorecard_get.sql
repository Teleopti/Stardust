IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_ccc_scorecard_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_ccc_scorecard_get]
GO

CREATE proc [mart].[report_ccc_scorecard_get]
(
	@UserID uniqueidentifier,
	@DateUtc smalldatetime
)
as

--The Date as Utc time doesn't feel good. But kind of tricky to solve, leaving that for now. Might revisit later. /Robin
SET NOCOUNT ON

SELECT 
	s.scorecard_code ID, 
	s.scorecard_name Name, 
	s.period Period
from mart.dim_person p WITH (NOLOCK)
inner join mart.dim_team t
on p.team_id=t.team_id
inner join mart.dim_scorecard s
on s.scorecard_id=t.scorecard_id
WHERE (s.scorecard_code IS NOT NULL) AND
	  p.person_code = @UserID AND
	  @DateUtc BETWEEN p.valid_from_date AND p.valid_to_date

GO

