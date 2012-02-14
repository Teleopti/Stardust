IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Users_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Users_Delete]
GO



----------------------------------------------------------------------------
-- Delete a single record from Users
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Users_Delete]
	@UserId int
AS

DELETE	Msg.Users
WHERE 	UserId = @UserId


GO

