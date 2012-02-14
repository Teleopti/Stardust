IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Users_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Users_Insert]
GO



----------------------------------------------------------------------------
-- Insert a single record into Users
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Users_Insert]
	@UserId int OUTPUT,
	@Domain nvarchar(100),
	@UserName nvarchar(100),
	@ChangedBy nvarchar(20),
	@ChangedDateTime datetime
AS
IF(@UserId = 0)
BEGIN
		DECLARE @RowsInTable INT 
		DECLARE @RowsForUserInTable INT
		SET @RowsInTable = (SELECT COUNT(*) FROM Msg.Users) 
		IF(@RowsInTable = 0)
			BEGIN
				SET @UserId = 1
				INSERT Msg.Users(UserId, Domain, UserName, ChangedBy, ChangedDateTime)
				VALUES (@UserId, @Domain, @UserName, @ChangedBy, @ChangedDateTime)
			END
		ELSE
			BEGIN
				SET @RowsForUserInTable = (SELECT COUNT(*) FROM Msg.Users WHERE UserName = @UserName) 
				IF(@RowsForUserInTable = 0)
					BEGIN
						SET @UserId = (SELECT MAX(UserId) + 1 FROM Msg.Users)
						INSERT Msg.Users(UserId, Domain, UserName, ChangedBy, ChangedDateTime)
						VALUES (@UserId, @Domain, @UserName, @ChangedBy, @ChangedDateTime)
					END
				ELSE
					BEGIN
						SET @UserId = (SELECT MAX(UserId) FROM Msg.Users WHERE UserName = @UserName)
						UPDATE	Msg.Users
						SET	Domain = @Domain,
							UserName = @UserName,
							ChangedBy = @ChangedBy,
							ChangedDateTime = @ChangedDateTime
						WHERE UserId = @UserId
					END
			END
	END
ELSE
BEGIN
	UPDATE	Msg.Users
	SET	Domain = @Domain,
		UserName = @UserName,
		ChangedBy = @ChangedBy,
		ChangedDateTime = @ChangedDateTime
	WHERE 	UserId = @UserId
END


GO

