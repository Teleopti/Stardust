IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_skillset_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_skillset_insert]
GO

-- =============================================
-- Description:	Insert new skillset 
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_skillset_insert]
@skillset_code nvarchar(4000), 
@skillset_name nvarchar(4000),
@business_unit_id int,
@datasource_id smallint,
@insert_date smalldatetime,
@update_date smalldatetime,
@datasource_update_date datetime
AS
BEGIN

INSERT INTO [mart].[dim_skillset]
	([skillset_code],
	[skillset_name],
	[business_unit_id],
	[datasource_id],
	[insert_date],
	[update_date],
	[datasource_update_date])
VALUES
	(@skillset_code,
	@skillset_name,
	@business_unit_id,
	@datasource_id,
	@insert_date,
	@update_date,
	@datasource_update_date)
END

GO


