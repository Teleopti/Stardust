IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[pm_user_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[pm_user_delete]
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		jonas n
-- Create date: 2010-06-21
-- Description:	Truncate table mart.pm_user
-- =============================================
CREATE PROCEDURE [mart].[pm_user_delete] 
AS
BEGIN
    DELETE FROM mart.pm_user
END

GO


