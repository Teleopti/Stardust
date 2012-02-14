IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_ccc_scorecard_get_period]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_ccc_scorecard_get_period]
GO

CREATE proc [mart].[report_ccc_scorecard_get_period]
(
	@ScorecardID uniqueidentifier
)
as

SET NOCOUNT ON

SELECT period FROM mart.dim_scorecard WHERE scorecard_code = @ScorecardID

GO
