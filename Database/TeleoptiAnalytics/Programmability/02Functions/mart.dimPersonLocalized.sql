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

DECLARE @dim_person_local TABLE (
	person_id int NOT NULL,
	[valid_from_date_local] [smalldatetime] NOT NULL,
	[valid_from_date_id] [int] NOT NULL,
	[valid_from_interval_id] [int] NOT NULL,
	[valid_to_date] [smalldatetime] NOT NULL,
	[valid_to_date_id] [int] NOT NULL,
	[valid_to_interval_id] [int] NOT NULL,
	[valid_from_date_id_local] [int] NULL,
	[valid_from_interval_id_local] [smallint] NULL,
	[valid_to_date_id_local] [int] NOT NULL,
	[valid_to_interval_id_local] [smallint] NOT NULL
)

INSERT INTO @dim_person_local
SELECT 	dp.person_id,
	dp.valid_from_date,
	dp.valid_from_date_id,
	dp.valid_from_interval_id,
	dp.valid_to_date,
	dp.valid_to_date_id,
	dp.valid_to_interval_id,
	b1.local_date_id as valid_from_date_id_local,
	b1.local_interval_id as valid_from_interval_id_local,
	ISNULL(b2.local_date_id,-2) as valid_to_date_id_local,
	ISNULL(b2.local_interval_id,0) as valid_to_interval_id_local

FROM
	mart.DimPersonAdapted() dp  --this one transforms the eternity date to max(date) from table dim_date
--From Date	
INNER JOIN mart.bridge_time_zone b1
	ON
	b1.time_zone_id = dp.time_zone_id
	AND dp.valid_from_date_id = b1.date_id
	AND dp.valid_from_interval_id = b1.interval_id

--To Date	
LEFT OUTER JOIN mart.bridge_time_zone b2
	ON
	b2.time_zone_id = dp.time_zone_id
	AND dp.valid_to_date_id = b2.date_id
	AND dp.valid_to_interval_id = b2.interval_id

--
INSERT INTO @dim_person_localized
SELECT 
	localized.person_id,
	d1.date_date + i1.interval_start as valid_from_date_local,
	d2.date_date + i2.interval_start as valid_to_date_local
FROM @dim_person_local localized
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

--Trim everything outside the period asked for
DELETE FROM @dim_person_localized
WHERE NOT
--------------
--Trying to explain the personPeriod filter:
--@date_from=A
--@date_to	=B
--personPeriods: 1-4 (personPeriod.valid_from_date, personPeriod.valid_to_date)
--------------
(
--                     A---------------------------------------------------------B		
--				1------|--------1
		(@date_from	>= valid_from_date_local AND @date_from <= valid_to_date_local)

	OR
--                               2-----------------23-------------3            		
		(valid_from_date_local > @date_from	AND valid_to_date_local < @date_to)

--                                                                  4-------------|----------4		
	OR
		(@date_to >= valid_from_date_local AND @date_to <= valid_to_date_local)
)


RETURN 

END

GO