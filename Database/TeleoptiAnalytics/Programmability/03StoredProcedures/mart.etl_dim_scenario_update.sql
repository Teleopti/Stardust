IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_scenario_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_scenario_update]
GO

-- =============================================
-- Author:		Pontus
-- Description:	Update 
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_scenario_update]
	@scenario_code uniqueidentifier, 
	@scenario_name nvarchar(50), 
	@default_scenario bit,
	@business_unit_id int,
	@business_unit_code uniqueidentifier,
	@business_unit_name nvarchar(50),
	@datasource_id smallint,
	@datasource_update_date smalldatetime,
	@is_deleted bit
AS
BEGIN
UPDATE [mart].[dim_scenario]
   SET [scenario_name] = @scenario_name
      ,[default_scenario] = @default_scenario
      ,[business_unit_id] = @business_unit_id
      ,[business_unit_code] = @business_unit_code
      ,[business_unit_name] = @business_unit_name
      ,[datasource_id] = @datasource_id
      ,[update_date] = GETUTCDATE()
      ,[datasource_update_date] = @datasource_update_date
      ,[is_deleted] = @is_deleted
	WHERE scenario_code = @scenario_code AND
		  business_unit_code = @business_unit_code
END
GO
