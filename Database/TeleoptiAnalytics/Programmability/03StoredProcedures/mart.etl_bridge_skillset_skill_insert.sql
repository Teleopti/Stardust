IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_skillset_skill_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_skillset_skill_insert]
GO

-- =============================================
-- Description:	Insert new reference between skills and skillset 
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_skillset_skill_insert]
@skillset_id int,
@skill_id int,
@business_unit_id int,
@datasource_id smallint,
@insert_date smalldatetime,
@update_date smalldatetime,
@datasource_update_date datetime
AS
BEGIN

INSERT INTO [mart].[bridge_skillset_skill]
	([skillset_id],
	[skill_id],
	[business_unit_id],
	[datasource_id],
	[insert_date],
	[update_date],
	[datasource_update_date])
VALUES
	(@skillset_id,
	@skill_id,
	@business_unit_id,
	@datasource_id,
	@insert_date,
	@update_date,
	@datasource_update_date)
END

GO


