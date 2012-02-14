IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[Timepart]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[Timepart]
GO



CREATE FUNCTION [mart].[Timepart] (@DATE datetime, @TIME_PART varchar(1))
	RETURNS varchar(5)
	AS
	BEGIN
	/*
	Created by Charlotte Lundin Teleopti AB
	
	Returns a string HH:MI containing hour and minutes.
	
	*/
		declare @r varchar(6)

DECLARE @time datetime
SET @time= @date

DECLARE @hour char(2)
SET @hour= 
		CASE 	WHEN len( datepart(hh,@time) ) =1 	
			THEN 	'0'+convert(char(1),datepart(hh,@time) )
			ELSE 	convert(char(2),datepart(hh,@time))
		END

DECLARE @minute char(2)

SET @minute= 
		CASE 	WHEN len( datepart(mi,@time) ) =1 	
			THEN 	'0'+convert(char(1),datepart(mi,@time) )
			ELSE 	convert(char(2),datepart(mi,@time))
		END

SET  @r = @hour + @TIME_PART + @minute

RETURN(@r)

END


GO

