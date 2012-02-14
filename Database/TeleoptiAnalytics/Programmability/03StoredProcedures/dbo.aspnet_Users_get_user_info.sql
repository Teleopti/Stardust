IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[aspnet_Users_get_user_info]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[aspnet_Users_get_user_info]
GO

CREATE PROCEDURE [dbo].[aspnet_Users_get_user_info]
	@user_name nvarchar(50),
	@windows_authentication bit
AS

IF (@windows_authentication = 1)
BEGIN
	SELECT 
		u.UserName, 
		u.LanguageId as LangID, 
		u.CultureId as CultureID, 
		u.UserID, 
		u.FirstName + ' ' + u.LastName AS PersonName 
	FROM 
		dbo.aspnet_Users u 
	INNER JOIN 
		dbo.aspnet_Applications a 
	ON 
		u.ApplicationID = a.ApplicationID 
	WHERE LOWER(u.UserName) = LOWER(@user_name) AND 
		a.ApplicationName = '/'
END
ELSE
BEGIN
	SELECT 
		u.AppLoginName as UserName,
		u.LanguageId as LangID, 
		u.CultureId as CultureID, 
		u.UserID, 
		u.FirstName + ' ' + u.LastName AS PersonName 
	FROM 
		dbo.aspnet_Users u 
	INNER JOIN 
		dbo.aspnet_Applications a 
	ON 
		u.ApplicationID = a.ApplicationID 
	WHERE 
		LOWER(u.AppLoginName) = LOWER(@user_name) AND 
		a.ApplicationName = '/'
END
GO