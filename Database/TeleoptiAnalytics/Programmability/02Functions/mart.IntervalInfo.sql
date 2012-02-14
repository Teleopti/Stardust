IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[IntervalInfo]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[IntervalInfo]
GO

-- =============================================
-- Author:		JN
-- Create date: 2007-12-12
-- Description:	Function that takes the inparameters @interval_per_day
--				and @interval. Returns a table with the following columns:
--				interval_name, halfhour_name, hour_name, interval_start, interval_end
-- =============================================
CREATE FUNCTION [mart].[IntervalInfo]
(
	@interval_per_day int,	
	@interval int
)
RETURNS @retIntervalInfo TABLE 
(
	interval_name nvarchar(11) not null,
	halfhour_name nvarchar(11) not null,
	hour_name nvarchar(5) not null,
	interval_start smalldatetime not null,
	interval_end smalldatetime not null
)
AS
BEGIN
	declare @min_per_int int, @hour int, @int_per_hour int, @int_per_halfhour int
	declare @time_part_1 nvarchar(5), @time_part_2 nvarchar(5)
	declare @interval_name nvarchar(11), @halfhour_name nvarchar(11), @hour_name nvarchar(11)
	declare @interval_start smalldatetime, @interval_end smalldatetime

	select @min_per_int = 1440 / @interval_per_day
	select @int_per_hour = 60 / @min_per_int
	select @int_per_halfhour = @int_per_hour / 2

	-- Get interval_start and interval_end	
	select @interval_start = dateadd(mi,@interval*@min_per_int,'1900-01-01')
	select @interval_end = dateadd(mi,(@interval+1)*@min_per_int,'1900-01-01')

	-- Get interval_name
	select @time_part_1 = SUBSTRING(convert(nvarchar(40),convert(smalldatetime,dateadd(mi,@interval*@min_per_int,'1900-01-01')),108),1,5)
	select @time_part_2 = SUBSTRING(convert(nvarchar(40),convert(smalldatetime,dateadd(mi,(@interval+1)*@min_per_int,'1900-01-01')),108),1,5)
	select @interval_name = @time_part_1 + '-' + @time_part_2

	-- Get hour_name
	select @hour = datepart(hour, dateadd(mi,@interval*@min_per_int,'1900-01-01'))
	select @hour_name = SUBSTRING(convert(nvarchar(40),convert(smalldatetime,dateadd(hour,@hour,'1900-01-01')),108),1,5)

	-- Get halfhour_name
	if (@interval % @int_per_halfhour <> 0)
	begin
		-- decrease interval to nearest half hour in back in time
		while (@interval % @int_per_halfhour <> 0)
		begin
			select @interval = @interval - 1
		end
	end

	select @time_part_1 = SUBSTRING(convert(nvarchar(40),convert(smalldatetime,dateadd(mi,@interval*@min_per_int,'1900-01-01')),108),1,5)
	select @time_part_2 = SUBSTRING(convert(nvarchar(40),convert(smalldatetime,dateadd(mi,(@interval*@min_per_int)+30,'1900-01-01')),108),1,5)
	select @halfhour_name = @time_part_1 + '-' + @time_part_2

	-- return table
	insert into @retIntervalInfo
		select @interval_name, @halfhour_name, @hour_name, @interval_start, @interval_end
	RETURN 
END

GO

