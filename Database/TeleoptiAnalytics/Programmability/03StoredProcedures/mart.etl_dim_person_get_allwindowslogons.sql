IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_person_get_allwindowslogons]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_person_get_allwindowslogons]
GO

CREATE PROCEDURE [mart].[etl_dim_person_get_allwindowslogons]
AS
BEGIN
	
SET NOCOUNT ON
	
SELECT DISTINCT person_code, windows_domain, windows_username
  FROM [mart].[dim_person] WITH (NOLOCK)

END

GO
