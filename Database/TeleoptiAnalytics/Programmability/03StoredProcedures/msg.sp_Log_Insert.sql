IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Log_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Log_Insert]
GO



----------------------------------------------------------------------------
-- Insert a single record into Log
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Log_Insert]
	@LogId uniqueidentifier OUTPUT,
	@ProcessId int,
	@Description text,
	@Exception text,
	@Message text,
	@StackTrace text,
	@ChangedBy nvarchar(10),
	@ChangedDateTime datetime
AS
BEGIN
--reset input to SQL Server time
SELECT @ChangedDateTime = GETDATE()

IF(@LogId = '00000000-0000-0000-0000-000000000000')
	BEGIN
		SET @LogId = newid()
    	INSERT Msg.[Log](LogId, ProcessId, Description, Exception, Message, StackTrace, ChangedBy, ChangedDateTime)
		VALUES (@LogId, @ProcessId, @Description, @Exception, @Message, @StackTrace, @ChangedBy, @ChangedDateTime)
	END
ELSE
	IF(EXISTS(SELECT 1 FROM Msg.[Log] WHERE LogId = @LogId))
		BEGIN
			UPDATE	Msg.[Log]
			SET	ProcessId = @ProcessId,
				Description = @Description,
				Exception = @Exception,
				Message = @Message,
				StackTrace = @StackTrace,
				ChangedBy = @ChangedBy,
				ChangedDateTime = @ChangedDateTime
			WHERE LogId = @LogId
		END
	ELSE
		BEGIN
			INSERT Msg.[Log](LogId, ProcessId, Description, Exception, Message, StackTrace, ChangedBy, ChangedDateTime)
			VALUES (@LogId, @ProcessId, @Description, @Exception, @Message, @StackTrace, @ChangedBy, @ChangedDateTime)
		END
END



GO

