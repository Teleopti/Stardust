IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_acd_login_person_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_acd_login_person_insert]
GO

-- =============================================
-- Description:	Insert new reference between acd_login and person
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_acd_login_person_insert]
@acd_login_id int,
@person_id int,
@team_id int,
@business_unit_id int,
@datasource_id smallint,
@insert_date smalldatetime,
@update_date smalldatetime,
@datasource_update_date datetime
AS
BEGIN

INSERT INTO [mart].[bridge_acd_login_person]
        ([acd_login_id]
        ,[person_id]
        ,[team_id]
        ,[business_unit_id]
        ,[datasource_id]
        ,[insert_date]
        ,[update_date]
        ,[datasource_update_date])
VALUES
	(@acd_login_id,
	@person_id,
	@team_id,
	@business_unit_id,
	@datasource_id,
	@insert_date,
	@update_date,
	@datasource_update_date)
END

GO


