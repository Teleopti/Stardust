/****** Object:  StoredProcedure [msg].[sp_Subscriber_Insert]    Script Date: 04/07/2009 10:43:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Subscriber_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Subscriber_Insert]
GO


----------------------------------------------------------------------------
-- Insert a single record into Subscriber
----------------------------------------------------------------------------
CREATE PROC [msg].[sp_Subscriber_Insert]
	@SubscriberId uniqueidentifier OUTPUT,
	@UserId int,
	@ProcessId int,
	@IPAddress nvarchar(15),
	@Port int,
	@ChangedBy nvarchar(10),
	@ChangedDateTime datetime
AS
BEGIN
--reset input to SQL Server time
SELECT @ChangedDateTime = GETDATE()

IF(@SubscriberId = '00000000-0000-0000-0000-000000000000')
	BEGIN
		SET @SubscriberId = newid()
		INSERT Msg.Subscriber(SubscriberId, UserId, ProcessId, IPAddress, Port, ChangedBy, ChangedDateTime)
		VALUES (@SubscriberId, @UserId, @ProcessId, @IPAddress, @Port, @ChangedBy, @ChangedDateTime)
	END
ELSE
	IF((SELECT COUNT(*) FROM Msg.Subscriber WHERE SubscriberId = @SubscriberId) > 0)
		BEGIN
			UPDATE	Msg.Subscriber
			SET	UserId = @UserId,
				ProcessId = @ProcessId,
				ChangedBy = @ChangedBy,
				ChangedDateTime = @ChangedDateTime
			WHERE 	SubscriberId = @SubscriberId
		END
	ELSE
		BEGIN
			INSERT Msg.Subscriber(SubscriberId, UserId, ProcessId, IPAddress, Port, ChangedBy, ChangedDateTime)
			VALUES (@SubscriberId, @UserId, @ProcessId, @IPAddress, @Port, @ChangedBy, @ChangedDateTime)
		END
END

GO

