IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_job_get_aggdatabase]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_job_get_aggdatabase]
GO
-- =============================================
-- Author:		KJ & MS
-- Create date: 2015-01-16
-- Description:	Gets the name of the agg database for index maintenance
-- =============================================
CREATE PROCEDURE [mart].[etl_job_get_aggdatabase] 
AS
	SELECT [target_customName] FROM [mart].[sys_crossdatabaseview_target]