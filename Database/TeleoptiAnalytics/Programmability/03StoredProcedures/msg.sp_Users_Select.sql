IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Users_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Users_Select]
GO



----------------------------------------------------------------------------
-- Select a single record from Users
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Users_Select]
	@UserId int
AS

SELECT	UserId,
	Domain,
	UserName,
	ChangedBy,
	ChangedDateTime
FROM	Msg.Users
WHERE 	UserId = @UserId


GO

