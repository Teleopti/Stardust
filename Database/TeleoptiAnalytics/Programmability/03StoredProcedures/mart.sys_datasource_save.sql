IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_datasource_save]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_datasource_save]
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		JN
-- Create date: 2009-12-04
-- Description:	Set time zone id on datasource or set to inactive.
-- =============================================
CREATE PROCEDURE [mart].[sys_datasource_save]
	@datasource_id int,
	@time_zone_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF (@time_zone_id = -1)
	BEGIN
		-- Set data source inactive
		UPDATE mart.sys_datasource
		SET inactive = 1,
			update_date = GETDATE()
		WHERE datasource_id = @datasource_id
	END
	ELSE
	BEGIN
		-- Set time zone
		UPDATE mart.sys_datasource
		SET time_zone_id = @time_zone_id,
			update_date = GETDATE()
		WHERE datasource_id = @datasource_id
	END
END

GO


