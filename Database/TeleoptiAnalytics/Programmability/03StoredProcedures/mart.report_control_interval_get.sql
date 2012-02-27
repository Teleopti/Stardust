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
--@interval_type int =3--DEFAULT TIMME OM INGET ANNAT ANGES KJ
as
/*
Last modfied:20080528
20080404 KJ Added parameter @param, 1= startinterval 2=endinterval
20080528 KJ Added parameter 6,7,8
20080910 Added parameter @bu_id KJ
2012-02-15 Changed to uniqueidentifier as report_id - Ola
*/
/*
IF @interval_type IN(3,6,7,8)--'Hour' + 'Day', 'Week', 'Month'
BEGIN
	IF  @param = 1
	BEGIN
		SELECT 
			id		= MIN(interval_id),
			name	= left(hour_name,5)
		FROM
			dim_interval 
		GROUP BY hour_name
	END
	ELSE
	BEGIN
		SELECT 
			id		= MAX(interval_id),
			name	= RIGHT(hour_name,5)
		FROM
			dim_interval 
		GROUP BY hour_name
	END
END


IF @interval_type = 4--'Half Hour'
BEGIN
	IF  @param = 1
	BEGIN
		SELECT 
			id		= MIN(interval_id),
			name	= left(halfhour_name,5)
		FROM
			dim_interval 
		GROUP BY halfhour_name
	END
	ELSE
	BEGIN
		SELECT 
			id		= MAX(interval_id),
			name	= RIGHT(halfhour_name,5)
		FROM
			dim_interval 
		GROUP BY halfhour_name
	END

END

*/

--IF @interval_type = 5--'Interval'
--BEGIN
	IF  @param = 1
	BEGIN
		SELECT 
			id		= MIN(interval_id),
			name	= left(interval_name,5)
		FROM
			mart.dim_interval 
		GROUP BY interval_name
	END
	ELSE
	BEGIN
		SELECT 
			id		= MAX(interval_id),
			name	= RIGHT(interval_name,5)
		FROM
			mart.dim_interval 
		GROUP BY interval_name
	END
--END

GO

