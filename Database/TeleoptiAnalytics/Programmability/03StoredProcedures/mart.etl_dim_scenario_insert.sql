IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_scenario_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_scenario_insert]
GO

-- =============================================
-- Author:		Pontus
-- Description:	Insert 
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_scenario_insert]
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
	INSERT INTO [mart].[dim_scenario]
           ([scenario_code]
           ,[scenario_name]
           ,[default_scenario]
           ,[business_unit_id]
           ,[business_unit_code]
           ,[business_unit_name]
           ,[datasource_id]
           ,[insert_date]
           ,[update_date]
           ,[datasource_update_date]
           ,[is_deleted])
	SELECT 
            @scenario_code,
			@scenario_name,
			@default_scenario,
			@business_unit_id,
			@business_unit_code,
			@business_unit_name,
			@datasource_id,
			GETUTCDATE(),
			GETUTCDATE(),
			@datasource_update_date,
			@is_deleted
	WHERE NOT EXISTS (SELECT * FROM [mart].[dim_scenario] WHERE scenario_code = @scenario_code AND
			business_unit_code = @business_unit_code)
END
GO
