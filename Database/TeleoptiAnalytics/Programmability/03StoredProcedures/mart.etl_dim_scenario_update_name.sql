IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_scenario_update_name]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_scenario_update_name]
GO

-- =============================================
-- Author:		Pontus
-- Description:	Insert 
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_scenario_update_name]
	@scenario_code uniqueidentifier, 
	@scenario_name nvarchar(50),
	@business_unit_code uniqueidentifier
AS
BEGIN
	UPDATE [mart].[dim_scenario]
	SET scenario_name = @scenario_name,
		update_date = GETUTCDATE()
	WHERE scenario_code = @scenario_code AND
		  business_unit_code = @business_unit_code
END
GO
