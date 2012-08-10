IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_interval_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_interval_get]
GO


--EXEC report_control_interval_get @param=N'2', @report_id=4,@person_code='EFE16A67-E352-4817-BDA8-9A7200410E30',@language_id=1053

CREATE Proc [mart].[report_control_interval_get]
@param int ,
@report_id uniqueidentifier,
@person_code uniqueidentifier,
@language_id int,
@bu_id uniqueidentifier
as
/*
Last modfied:20080528
20080404 KJ Added parameter @param, 1= startinterval 2=endinterval
20080528 KJ Added parameter 6,7,8
20080910 Added parameter @bu_id KJ
2012-02-15 Changed to uniqueidentifier as report_id - Ola
2012-08-10 JN Just return interval_id and bit for if the target is a "from" or a "to" interval control.
*/
DECLARE @is_interval_to bit
IF  @param = 1
	SET @is_interval_to = 0
ELSE
	SET @is_interval_to = 1

CREATE TABLE #result (id int, is_interval_to bit)

INSERT INTO #result
	SELECT 
		MIN(interval_id),
		@is_interval_to
	FROM
		mart.dim_interval 
	GROUP BY interval_name

SELECT * 
FROM #result 
ORDER BY id

GO

