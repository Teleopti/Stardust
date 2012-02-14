IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[DimPersonAdapted]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[DimPersonAdapted]
GO


-- =============================================
-- Author:		Jonas
-- Create date: 2010-03-24
-- Description:	Returns a table with Person information where valid_to_date and 
-- valid_to_interval_id are adapted to eternuty date 2059-12-31. This to make dim_person more "joinable".
-----------------------------------------------
-- update log
-----------------------------------------------
-- When			Who	What
-- 2010-11-08	DJ	Adding Time_zone_id to the result set
-- =============================================
CREATE FUNCTION [mart].[DimPersonAdapted] 
(
)
RETURNS 
@dim_person TABLE 
(
	person_id int, 
	person_name nvarchar(200),
	valid_from_date smalldatetime, 
	valid_to_date smalldatetime, 
	valid_from_date_id int, 
	valid_to_date_id int, 
	valid_from_interval_id int, 
	valid_to_interval_id int,
	team_id int,
	team_name nvarchar(100),
	site_id int,
	time_zone_id int
)
AS
BEGIN

DECLARE @interval_length int, @intervals_per_day int, @max_date smalldatetime
SELECT @interval_length = DATEDIFF(mi, interval_start, interval_end) FROM mart.dim_interval WHERE interval_id = 0
SET @intervals_per_day = 1440/@interval_length
SELECT @max_date = MAX(date_date) FROM mart.dim_date WHERE date_date <> '20591231'

INSERT INTO @dim_person (person_id, person_name, valid_from_date, valid_to_date, team_id, team_name, site_id, time_zone_id)
SELECT	person_id, 
		person_name,
		valid_from_date,
		CASE WHEN valid_to_date = '20591231'
			THEN @max_date
			ELSE	CASE WHEN (DATEDIFF(mi, convert(smalldatetime, convert(int, (convert(float, valid_to_date)))), valid_to_date)) = 0
						THEN DATEADD(d, -1, valid_to_date)
						ELSE valid_to_date
					END
		END AS 'valid_to_date',
		team_id,
		team_name,
		site_id,
		time_zone_id
FROM mart.dim_person

UPDATE @dim_person
SET valid_from_date_id = d1.date_id,
	valid_to_date_id = d2.date_id,
	valid_from_interval_id = ((datepart(hh, p.valid_from_date)*60) + datepart(mi, p.valid_from_date)) / @interval_length, 
valid_to_interval_id = CASE WHEN (((datepart(hh, p.valid_to_date)*60) + datepart(mi, p.valid_to_date)) / @interval_length) = 0
						THEN @intervals_per_day -1
						ELSE (((datepart(hh, p.valid_to_date)*60) + datepart(mi, p.valid_to_date)) / @interval_length) -1
					END
FROM @dim_person p
INNER JOIN mart.dim_date d1
	ON d1.date_date = convert(smalldatetime, convert(int, (convert(float, p.valid_from_date))))
INNER JOIN mart.dim_date d2
	ON d2.date_date = convert(smalldatetime, convert(int, (convert(float, p.valid_to_date))))
	
RETURN 

END

GO