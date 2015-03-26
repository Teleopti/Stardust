IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[SubtractInterval]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[SubtractInterval]
GO

-- =============================================
-- Author:		David
-- Create date: 2014-10-16
-----------------------------------------------
-- update log
-----------------------------------------------
-- When			Who	What
-- YYYY-MM-DD	NN	Desc
-- =============================================
CREATE FUNCTION [mart].[SubtractInterval] 
(
@date_from smalldatetime,
@interval_id smallint,
@substract smallint
)
RETURNS 
@SubtractInterval TABLE 
(
	date_from smalldatetime NOT NULL,
	interval_id smallint NOT NULL
)
AS
BEGIN
	DECLARE @intervals_per_day smallint
	DECLARE @interval_id_out smallint
	DECLARE @date_from_out smalldatetime

	SELECT @intervals_per_day = MAX(interval_id) FROM mart.dim_interval

	--interval before last midnight break
	if @substract>@interval_id
	begin
		set @interval_id_out=@intervals_per_day-(abs(@interval_id-@substract) % @intervals_per_day)
		--less than 24  hours, but still cross midnight => dateadd -1
		if @substract<@intervals_per_day
			set @date_from_out	=dateadd(DAY,-1-(@substract/@intervals_per_day),@date_from)
		--more than 24 hours, no need to add the extra midnight break(s)
		else
			set @date_from_out	=dateadd(DAY,-(@substract/@intervals_per_day),@date_from)
	end

	--within "today"
	else
	begin
		set @interval_id_out=@interval_id-@substract
		set @date_from_out	=@date_from
	end

	INSERT INTO @SubtractInterval
	SELECT
	@date_from_out,
	@interval_id_out

	RETURN 

END

GO