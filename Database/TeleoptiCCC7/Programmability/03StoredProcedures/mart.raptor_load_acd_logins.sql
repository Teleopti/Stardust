IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_load_acd_logins]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_load_acd_logins]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2009-01-20
-- Description:	Used by Freemimum to get an empty result set E.g fake the Analytics ACD-logins syncing
-- Change:		
-- =============================================

CREATE PROCEDURE mart.[raptor_load_acd_logins] 
AS

--Create teporaty table
CREATE TABLE #dim_acd_login(
	[acd_login_id] [int] IDENTITY(1,1) NOT NULL,
	[acd_login_agg_id] [int] NULL,
	[acd_login_original_id] [nvarchar](50) NULL,
	[acd_login_name] [nvarchar](100) NULL,
	[log_object_name] [nvarchar](100) NULL,
	[is_active] [bit] NULL,
	[datasource_id] [smallint] NULL
)

--Select empty result set for Freemimum
SELECT	acd_login_id			AcdLogOnMartId,
		acd_login_agg_id		AcdLogOnAggId, 
		acd_login_original_id	AcdLogOnOriginalId, 
		acd_login_name			AcdLogOnName,
		is_active				Active,
		datasource_id			DataSourceId
FROM #dim_acd_login
GO

