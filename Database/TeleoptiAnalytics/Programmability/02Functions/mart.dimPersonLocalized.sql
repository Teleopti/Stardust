IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[DimPersonLocalized]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[DimPersonLocalized]
GO


-- =============================================
-- Author:		David
-- Create date: 2010-11-08
-- Description:	Returns a table with Person information where valid_to_date and valid_to_interval_id are:
-- 1) using an adapted eternity date 2059-12-31 => Max from dim_date. This to make dim_person more "joinable".
-- 2) converting UTC From+To into each agents localized From+To
-----------------------------------------------
-- update log
-----------------------------------------------
-- When			Who	What
-- 2010-11-09	DJ	Fixing UTC(+h)
-- =============================================
--SELECT * FROM [mart].[DimPersonLocalized]()
CREATE FUNCTION [mart].[DimPersonLocalized] 
(
@date_from smalldatetime,
@date_to smalldatetime
)
RETURNS 
@dim_person_localized TABLE 
(
	person_id int NOT NULL,
	valid_from_date_local smalldatetime NOT NULL,
	valid_to_date_local smalldatetime NOT NULL
)
AS
BEGIN

INSERT INTO @dim_person_localized
SELECT 
	person_id,
	d1.date_date + i1.interval_start as valid_from_date_local,
	d2.date_date + i2.interval_start as valid_to_date_local
FROM mart.dim_person localized
--From date
INNER JOIN mart.dim_date d1
	ON localized.valid_from_date_id_local = d1.date_id
INNER JOIN mart.dim_interval i1
	ON localized.valid_from_interval_id_local = i1.interval_id

--To date
INNER JOIN mart.dim_date d2
	ON localized.valid_to_date_id_local = d2.date_id
INNER JOIN mart.dim_interval i2
	ON localized.valid_to_interval_id_local = i2.interval_id
WHERE 
--------------
--Trying to explain the personPeriod filter:
--@date_from=A
--@date_to	=B
--personPeriods: 1-4 (personPeriod.valid_from_date, personPeriod.valid_to_date)
--------------
(
--                     A---------------------------------------------------------B		
--				1------|--------1
		(@date_from	>= d1.date_date + i1.interval_start AND @date_from <= d2.date_date + i2.interval_start)

	OR
--                               2-----------------23-------------3            		
		(d1.date_date + i1.interval_start > @date_from	AND d2.date_date + i2.interval_start < @date_to)

--                                                                  4-------------|----------4		
	OR
		(@date_to >= d1.date_date + i1.interval_start AND @date_to <= d2.date_date + i2.interval_start)
)


RETURN 

END

GO