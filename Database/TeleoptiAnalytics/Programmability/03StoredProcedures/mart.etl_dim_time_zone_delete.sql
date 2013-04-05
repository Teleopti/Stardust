IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_time_zone_delete.sql]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_time_zone_delete.sql]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2013-04-05
-- Description:	Deletes data that depnds on mart.dim_time_zone
--				
-- =============================================
-- Change Log:
-- Date			By		Description
-- =============================================
--
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_time_zone_delete.sql]

AS
--bridge_group_page_person
DELETE FROM b
FROM mart.bridge_time_zone b
   INNER JOIN mart.dim_time_zone AS dim
    ON b.time_zone_id = dim.time_zone_id
WHERE dim.to_be_deleted = 1

-- dim_time_zone
DELETE FROM mart.dim_time_zone
WHERE to_be_deleted = 1

GO