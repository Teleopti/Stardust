IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_site_id_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_site_id_get]
GO

--exec [mart].[etl_dim_site_id_get] '6A21C802-7A34-4917-8DFD-9B5E015AB461','Paris',1
-- =============================================
-- Author:		Karin
-- Create date: 2016-02-24
-- Description:	Get a site_id from given site_code. If not exits hten create it first.
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_site_id_get]
@site_code uniqueidentifier, 
@site_name nvarchar(100),
@business_unit_id int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @site_id int

    SELECT @site_id = site_id 
	FROM mart.dim_site
	WHERE site_code = @site_code

	IF @site_id IS NULL
	BEGIN
		INSERT INTO mart.dim_site(site_code,site_name,business_unit_id,	datasource_id)
		VALUES (@site_code,@site_name,@business_unit_id, 1)

		SET @site_id = @@IDENTITY
	END

	SELECT @site_id
END

GO


