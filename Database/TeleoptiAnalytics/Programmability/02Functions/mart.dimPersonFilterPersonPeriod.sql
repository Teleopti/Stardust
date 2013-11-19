IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dimPersonFilterPersonPeriod]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[dimPersonFilterPersonPeriod]
GO


-- =============================================
-- Author:		David
-- Create date: 2013-11-17
-- Description:	Returns a table with Person information where valid_to_date and valid_to_interval_id are:
-- 1) using an adapted eternity date 2059-12-31 => Max from dim_date. This to make dim_person more "joinable".
-- 2) converting UTC From+To into each agents localized From+To
-----------------------------------------------
-- update log
-----------------------------------------------
-- When			Who	What
-- 2013-11-19	DJ	Adding valid_from_date_local+valid_to_date_local to dim_person
-- =============================================
/*
set statistics IO on
set statistics TIME on
SELECT * FROM [mart].[dimPersonFilterPersonPeriod]('2001-02-01','2014-12-31','47A3D4AA-3CD8-4235-A7EB-9B5E015B2560')
select * FROM [mart].[PersonCodeToId]('47A3D4AA-3CD8-4235-A7EB-9B5E015B2560','2001-02-01','2015-12-31',null,null)
SELECT * FROM [mart].[dim_person] WHERE person_code = '47A3D4AA-3CD8-4235-A7EB-9B5E015B2560'
*/
CREATE FUNCTION [mart].[dimPersonFilterPersonPeriod] 
(
@date_from smalldatetime,
@date_to smalldatetime,
@person_code uniqueidentifier
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
	valid_from_date_local,
	valid_to_date_local
FROM mart.dim_person
WHERE person_code = @person_code
AND
--------------
--Trying to explain the personPeriod filter:
--@date_from=A
--@date_to	=B
--personPeriods: 1-4 (personPeriod.valid_from_date_local, personPeriod.valid_to_date_local)
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