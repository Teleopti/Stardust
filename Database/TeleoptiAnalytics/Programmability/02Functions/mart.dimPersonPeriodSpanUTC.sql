/****** Object:  UserDefinedFunction [mart].[dimPersonPeriodSpanUTC]    Script Date: 10/27/2009 14:41:32 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dimPersonPeriodSpanUTC]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[dimPersonPeriodSpanUTC]
GO



/****** Object:  UserDefinedFunction [mart].[dimPersonPeriodSpanUTC]    Script Date: 10/27/2009 14:41:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		DJ
-- Create date: 2009-10-22
-- Description: Takes local from- and to date without time part and time zone id.
--				Returns a table with the person periods, from mart.dim_person, that
--				matches.
-- Change log:
--
-- =============================================

/*
--Example:
declare @local_date_from smalldatetime,@local_date_to smalldatetime, @time_zone_id int
set @local_date_from = '2005-01-04'
set @local_date_to = '2010-12-22'
set @time_zone_id = 2

SELECT * FROM mart.[dimPersonPeriodSpanUTC](@local_date_from, @local_date_to,@time_zone_id)
*/


CREATE FUNCTION [mart].[dimPersonPeriodSpanUTC](@local_date_from smalldatetime, @local_date_to smalldatetime, @time_zone_id int)
RETURNS @dimPersonPeriodSpanUTC TABLE (
	[person_id] [int] NOT NULL,
	[person_code] [uniqueidentifier] NULL,
	[team_id] [int] NULL,
	[skillset_id] [int] NULL,
	[valid_from_date_id] [int] NOT NULL,
	[valid_from_interval_id] [int] NOT NULL,
	[valid_to_date_id] [int] NOT NULL,
	[valid_to_interval_id] [int] NOT NULL
) 

AS
BEGIN	

-- Get max date from mart.dim_date so we can replace date_id=-2 (eternity date 2059-12-31)
DECLARE @max_date_id int
SELECT @max_date_id = max(date_id) FROM mart.dim_date

INSERT INTO @dimPersonPeriodSpanUTC
SELECT dp.person_id, 
		dp.person_code, 
		dp.team_id, 
		dp.skillset_id,
		dp.valid_from_date_id,
		dp.valid_from_interval_id,
		CASE 
			WHEN dp.valid_to_date_id = -2
			THEN @max_date_id
			ELSE dp.valid_to_date_id
		END AS 'valid_to_date_id',
		dp.valid_to_interval_id
FROM mart.dim_person dp
INNER JOIN mart.[LocalDateIntervalToUTC](@local_date_from, @local_date_to,@time_zone_id) utc
ON 1 = 1
WHERE (
		(
			-- Border liners.
			-- Lookup and person period ends same date
			(utc.to_date_id = CASE 
								WHEN dp.valid_to_date_id = -2
								THEN @max_date_id
								ELSE dp.valid_to_date_id
							END
				AND utc.to_interval_id <= dp.valid_to_interval_id)
			OR
			-- Loopkup period ends on person period start date
			(utc.to_date_id = dp.valid_from_date_id AND utc.to_interval_id >= dp.valid_from_interval_id)
			OR
			-- Lookup and person periods start same date
			(utc.from_date_id = dp.valid_from_date_id AND utc.from_interval_id >= dp.valid_from_interval_id)
			OR
			-- Lookup period starts on person period end
			(utc.from_date_id = CASE 
									WHEN dp.valid_to_date_id = -2
									THEN @max_date_id
									ELSE dp.valid_to_date_id
								END
				AND utc.from_interval_id <= dp.valid_to_interval_id)
		)
		OR
		(
			-- Other cases.
			-- Lookup period start is inside person period
			(utc.from_date_id > dp.valid_from_date_id AND utc.from_date_id < dp.valid_to_date_id)
			OR
			-- Lookup period end is inside person period
			(utc.to_date_id > dp.valid_from_date_id AND utc.to_date_id < CASE 
																			WHEN dp.valid_to_date_id = -2
																			THEN @max_date_id
																			ELSE dp.valid_to_date_id
																		END)
			OR
			 -- Person period is inside the lookup period
			(utc.from_date_id < dp.valid_from_date_id AND utc.to_date_id > CASE 
																				WHEN dp.valid_to_date_id = -2
																				THEN @max_date_id
																				ELSE dp.valid_to_date_id
																			END)
		)
	)
AND dp.to_be_deleted = 0  --Cosmetics and used yet anywhere else (2009-10-21), Performace?
ORDER BY dp.person_id, dp.valid_from_date_id

RETURN
END

GO


