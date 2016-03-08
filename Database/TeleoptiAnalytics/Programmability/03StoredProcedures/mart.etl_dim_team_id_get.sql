IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_team_id_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_team_id_get]
GO

-- =============================================
-- Author:		Karin
-- Create date: 2016-02-24
-- Description:	Get a team_id from given team_code. If not exits hten create it first.
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_team_id_get]
@team_code uniqueidentifier, 
@team_name nvarchar(100),
@site_id int,
@business_unit_id int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @team_id int
	DECLARE @scorecard_id int

    SELECT @team_id = team_id 
	FROM mart.dim_team WITH (NOLOCK)
	WHERE team_code = @team_code

	IF @team_id IS NULL
	BEGIN
		SELECT @scorecard_id  = isnull(scorecard_id,-1) FROM mart.dim_scorecard WITH (NOLOCK) WHERE business_unit_id=@business_unit_id
		
		INSERT INTO mart.dim_team(team_code,team_name,scorecard_id,site_id,business_unit_id,datasource_id)
		VALUES (@team_code,@team_name,@scorecard_id,@site_id,@business_unit_id, 1)

		SET @team_id = @@IDENTITY
	END

	SELECT @team_id
END

GO


