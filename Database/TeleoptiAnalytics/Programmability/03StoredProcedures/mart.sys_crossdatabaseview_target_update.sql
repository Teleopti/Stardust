IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_crossdatabaseview_target_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_crossdatabaseview_target_update]
GO


-- =============================================
-- Author:		DJ
-- Create date: 2008-10-03
-- Description:	Updates the table that holds all cross db views
--
-- EXEC sys_crossdatabaseview_target_update @defaultname = 'TeleoptiAnalytics_Stage', @customname= 'TeleoptiAnalytics_Stage_CustomerName'
-- Updates:		20090211 Added new mart schema KJ
-- Interface:	
-- =============================================


CREATE PROCEDURE [mart].[sys_crossdatabaseview_target_update]
@defaultname	varchar(100),
@customname		varchar(100)
AS

IF (dbo.IsAzureDB() = 1 OR mart.AllLogObjectsAreInternal()=1)
SELECT @customname = db_name()

IF EXISTS (SELECT name FROM sys.databases WHERE name = @customname)
BEGIN
	UPDATE mart.sys_crossdatabaseview_target
	SET target_customName			= @customname,
		confirmed					= 1
	WHERE target_defaultName		= @defaultname
END
ELSE
BEGIN
	UPDATE mart.sys_crossdatabaseview_target
	SET target_customName			= @customname,
		confirmed					= 0
	WHERE target_defaultName		= @defaultname
	PRINT 'Warning: No database with name: ['+ @customname +'] exists in this instance!'
END

GO