IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_acd_login_person_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_acd_login_person_delete]
GO

-- =============================================
-- Description:	Delete reference between acd_login and person
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_acd_login_person_delete]
@acd_login_id int,
@person_id int
AS
BEGIN

DELETE FROM [mart].[bridge_acd_login_person]
      WHERE
			acd_login_id = @acd_login_id
	  AND	person_id = @person_id

END

GO
