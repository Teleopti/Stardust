IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_interval_check]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_interval_check]
GO

-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
create PROCEDURE [mart].[etl_dim_interval_check] 
	
AS

SELECT COUNT(*) FROM mart.dim_interval

GO

