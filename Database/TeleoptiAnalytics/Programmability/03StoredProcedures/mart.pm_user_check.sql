IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[pm_user_check]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[pm_user_check]
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		jonas n
-- Create date: 2010-06-24
-- Description:	Truncate table mart.pm_user
-- =============================================
CREATE PROCEDURE [mart].[pm_user_check] 
@user_name nvarchar(256),
@is_windows_logon bit
AS
BEGIN
	DECLARE @user_found bit

    SELECT @user_found = COUNT(*) 
    FROM mart.pm_user
    WHERE [user_name] = @user_name
		AND is_windows_logon = @is_windows_logon
    
    SELECT @user_found
END

GO


