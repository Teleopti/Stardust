IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_site_id_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_site_id_get]
GO


--exec [mart].[etl_dim_site_id_get] '753439BE-978A-42C8-8291-3687A5B240CA','NewSite',1
-- =============================================
-- Author:		Karin
-- Create date: 2016-02-24
-- Description:	Get a site_id from given site_code. If not exits then create it first.
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
	FROM mart.dim_site WITH (NOLOCK)
	WHERE site_code = @site_code
	AND business_unit_id = @business_unit_id

	IF @site_id IS NULL
	BEGIN
		
		INSERT  mart.dim_site(site_code,site_name,business_unit_id,	datasource_id)
		SELECT @site_code,@site_name,@business_unit_id, 1
		WHERE NOT EXISTS(SELECT 1 FROM mart.dim_site where site_code=@site_code  AND business_unit_id = @business_unit_id)

		SELECT @site_id = site_id 
		FROM mart.dim_site
		WHERE site_code = @site_code
		AND business_unit_id = @business_unit_id
	END

	SELECT @site_id
END

GO


