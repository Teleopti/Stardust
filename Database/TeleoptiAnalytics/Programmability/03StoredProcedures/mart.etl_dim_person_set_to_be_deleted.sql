IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_person_set_to_be_deleted]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_person_set_to_be_deleted]
GO

-- =============================================
-- Description:	Set to be deleted flag for person period
-- =============================================

CREATE PROCEDURE [mart].[etl_dim_person_set_to_be_deleted]
	(@person_period_code uniqueidentifier)
AS
BEGIN
	UPDATE [mart].[dim_person]
	SET [to_be_deleted] = 1
	WHERE [person_period_code] = @person_period_code
END

GO
