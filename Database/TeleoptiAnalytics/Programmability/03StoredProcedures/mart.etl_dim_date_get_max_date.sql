IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_date_get_max_date]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_date_get_max_date]
GO

-- =============================================
-- Author:		JN
-- Create date: 2008-09-17
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- Description:	Return tha max date from dim_date
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_date_get_max_date]
	
AS

SELECT 
	isnull(max(date_date), '1999-12-31') as max_date
FROM mart.dim_date
WHERE 
	date_id > -1

GO

