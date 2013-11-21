IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[PersonCodeToId]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[PersonCodeToId]
GO

-- =================================================
-- Author:		ME
-- Create date: 2011-01-19
-- Description:	Converts a person_code to person_id 
-- =================================================
CREATE FUNCTION [mart].[PersonCodeToId]
(
	@person_code uniqueidentifier,
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
	DECLARE @id int
	--20131120 Reset @date_from in case of night shifts and person_period change
	SET @date_from = DATEADD(d,-1,@date_from)

	IF @person_code = '00000000-0000-0000-0000-000000000002' --All
	BEGIN
		INSERT INTO @ids
		SELECT dp.person_id
			FROM mart.dim_person dp
			INNER JOIN [mart].[DimPersonLocalized](@date_from,@date_to) dpl
			ON		dp.person_id = dpl.person_id
					AND dpl.valid_from_date_local <= @date_to
					AND dpl.valid_to_date_local >= @date_from
			WHERE	(
						dp.team_id IN (SELECT id FROM mart.SplitStringInt(@team_set))
						OR 
						(
							-2 IN (SELECT id FROM mart.SplitStringInt(@team_set)) 
							OR 
							@team_set IS NULL
						)
					)
					AND (dp.site_id = @site_id OR (@site_id < -1 OR @site_id IS NULL))
	END
	ELSE IF @person_code = '00000000-0000-0000-0000-000000000001' --Not defined
	BEGIN
		INSERT INTO @ids (id)
		VALUES (-1)
	END
	ELSE
		INSERT INTO @ids
		SELECT dp.person_id
			FROM mart.dim_person dp
			INNER JOIN [mart].[dimPersonFilterPersonPeriod](@date_from,@date_to,@person_code) dpl
			ON		dp.person_id = dpl.person_id
					AND dpl.valid_from_date_local <= @date_to
					AND dpl.valid_to_date_local >= @date_from
			WHERE	(
						dp.team_id IN (SELECT id FROM mart.SplitStringInt(@team_set))
						OR 
						(
							-2 IN (SELECT id FROM mart.SplitStringInt(@team_set)) 
							OR 
							@team_set IS NULL
						)
					)
					AND (dp.site_id = @site_id OR (@site_id < -1 OR @site_id IS NULL))
RETURN

END




