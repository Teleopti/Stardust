IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RTA].[rta_load_datasources]') AND type in (N'P', N'PC'))
DROP PROCEDURE [RTA].[rta_load_datasources]
GO

-- =============================================
-- Author:		Robin K
-- Create date: 2010-05-20
-- Description:	Load data sources for use in RTA
-- =============================================
CREATE PROCEDURE RTA.rta_load_datasources AS
BEGIN
	SELECT datasource_id,source_id FROM mart.sys_datasource
END
GO

