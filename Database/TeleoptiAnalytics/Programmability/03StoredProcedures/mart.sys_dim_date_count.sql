IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_dim_date_count]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_dim_date_count]
GO

-- =============================================
-- Author:		Jonas & Maria
-- Create date: 2014-10-24
-- Description:	Count the number of days in mart.dim_date. Exclude Not defined and Eternity.
-- =============================================
CREATE PROCEDURE mart.sys_dim_date_count
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DATEDIFF(d, MIN(date_date), Max(date_date)) AS number_of_days,
		MIN(date_date) AS start_date
	FROM mart.dim_date
	WHERE date_id NOT IN (-1, -2)
END
GO
