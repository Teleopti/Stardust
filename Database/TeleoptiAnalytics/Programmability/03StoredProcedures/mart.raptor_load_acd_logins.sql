IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_load_acd_logins]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_load_acd_logins]
GO

CREATE PROCEDURE mart.[raptor_load_acd_logins] 
AS
BEGIN
    SET NOCOUNT ON;
    
	SELECT	acd_login_id			AcdLogOnMartId,
        acd_login_agg_id		AcdLogOnAggId, 
        acd_login_original_id	AcdLogOnOriginalId,
        acd_login_name			AcdLogOnName,
        is_active				Active,
        datasource_id			DataSourceId 
	FROM mart.dim_acd_login 
	WHERE acd_login_id > -1
END
GO

