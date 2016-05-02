IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_day_off_insert_not_defined]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_day_off_insert_not_defined]
GO

-- =============================================
-- Author:		Pontus
-- Description:	Insert not defined row
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_day_off_insert_not_defined]
WITH EXECUTE AS OWNER
AS
BEGIN
SET IDENTITY_INSERT [mart].[dim_day_off] ON;
INSERT INTO [mart].[dim_day_off]
		([day_off_id]
		,[day_off_code]
		,[day_off_name]
		,[display_color]
		,[business_unit_id]
		,[datasource_id]
		,[insert_date]
		,[update_date]
		,[datasource_update_date]
		,[display_color_html]
		,[day_off_shortname])
	VALUES
		(-1
		,null
		,'Not Defined'
		,-1
		,-1
		,-1
		,GETUTCDATE()
		,GETUTCDATE()
		,null
		,'#FFFFFF'
		,'Not Defined')
SET IDENTITY_INSERT [mart].[dim_day_off] OFF;
END
GO

