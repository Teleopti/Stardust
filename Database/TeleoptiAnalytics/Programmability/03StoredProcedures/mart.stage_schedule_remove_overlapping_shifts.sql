IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Mart].[stage_schedule_remove_overlapping_shifts]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Mart].[stage_schedule_remove_overlapping_shifts]
GO


-- =============================================
-- Author:		DavidJ
-- Create date: 2014-01-07
-- Description:	Remove overlapping shifts
-- Update date: 
-- =============================================
CREATE PROCEDURE [mart].[stage_schedule_remove_overlapping_shifts]
AS
BEGIN
SET NOCOUNT ON

	--remove potential duplicates from stage
	delete a
	from
	(select schedule_date_utc,person_code,scenario_code,interval_id,shift_start,
		ROW_NUMBER() over (partition by schedule_date_utc,person_code,scenario_code,interval_id
		order by shift_start desc) RowNumber --keep latest shift_start day. E.g "today" wins over "yesterday"
	from stage.stg_schedule) as a
	where a.RowNumber > 1
END
GO
