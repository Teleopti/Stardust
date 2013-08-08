IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_shift_length_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_shift_length_load]
GO


-- =============================================
-- Author:		ChLu
-- Create date: 2008-01-31
-- Description:	
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_shift_length_load]
WITH EXECUTE AS OWNER	
AS

--------------------------------------------------------------------------
-- Not Defined shift length
SET IDENTITY_INSERT mart.dim_shift_length ON

INSERT INTO mart.dim_shift_length
	(
	shift_length_id, 
	shift_length_m, 
	shift_length_h, 
	datasource_id
	)
SELECT 
	shift_length_id				= -1, 
	shift_length_m				= -1, 
	shift_length_h				= -1,  
	datasource_id				= 1
WHERE NOT EXISTS (SELECT * FROM mart.dim_shift_length where shift_length_id = -1)

SET IDENTITY_INSERT mart.dim_shift_length OFF

-- Insert new shift categories
INSERT INTO mart.dim_shift_length
	(
	shift_length_m,
	datasource_id
	)
SELECT DISTINCT
	shift_length_m	= s.shift_length_m,	 
	datasource_id	= 1
FROM
	Stage.stg_schedule s
WHERE 
	NOT EXISTS (SELECT shift_length_id FROM mart.dim_shift_length d WHERE d.shift_length_m = s.shift_length_m and datasource_id = 1)



---------------------------------------------------------------------------
-- update changes on shift_length
UPDATE mart.dim_shift_length
SET 
	shift_length_h				= convert(float,dim_shift_length.shift_length_m)/60,
	update_date					= getdate()	

GO

