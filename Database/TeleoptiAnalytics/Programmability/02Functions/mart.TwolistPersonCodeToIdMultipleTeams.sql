IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[TwolistPersonCodeToIdMultipleTeams]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[TwolistPersonCodeToIdMultipleTeams]
GO

-- =================================================
-- Author:		ME
-- Create date: 2011-01-19
-- Description:	Converts a person_code to person_id
-- =================================================
CREATE FUNCTION [mart].[TwolistPersonCodeToIdMultipleTeams]
(
	@person_codes nvarchar(max),
	@date_from datetime,
	@date_to datetime,
	@site_id int,
	@team_set nvarchar(max)
)
RETURNS
@ids TABLE
(
	id int
)

As
BEGIN 

	--20131120 Reset @date_from in case of night shifts and person_period change
	SET @date_from = DATEADD(d,-1,@date_from)

	DECLARE @teams TABLE
	(
		team_id INT
	)
	
	INSERT INTO @teams
	SELECT id FROM mart.SplitStringInt(@team_set)
	
	INSERT INTO @ids
	SELECT dp.person_id
		FROM mart.dim_person dp
		INNER JOIN [mart].[DimPersonLocalized](@date_from,@date_to) dpl
		ON		dp.person_id = dpl.person_id
				AND dpl.valid_from_date_local <= @date_to
				AND dpl.valid_to_date_local >= @date_from
		INNER JOIN mart.SplitStringGuid(@person_codes) ssg
		ON		ssg.id = dp.person_code
		WHERE	((dp.team_id IN (SELECT team_id FROM @teams)) OR (-2 IN (SELECT team_id FROM @teams) OR @team_set IS NULL))
				AND
				(dp.site_id = @site_id OR (@site_id < -1 OR @site_id IS NULL))
					
	IF (SELECT COUNT(*) FROM mart.SplitStringGuid(@person_codes) ssg WHERE ssg.id = '00000000-0000-0000-0000-000000000001') > 0
	BEGIN
		INSERT INTO @ids
		SELECT -1
	END
	
	IF (SELECT COUNT(*) FROM mart.SplitStringGuid(@person_codes) ssg WHERE ssg.id = '00000000-0000-0000-0000-000000000002') > 0
	BEGIN
		INSERT INTO @ids
		SELECT -2
	END

RETURN

END



GO