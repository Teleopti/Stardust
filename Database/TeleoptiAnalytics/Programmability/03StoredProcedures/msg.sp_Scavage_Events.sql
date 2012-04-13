/****** Object:  StoredProcedure [msg].[sp_Scavage_Events]    Script Date: 04/07/2009 10:43:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Scavage_Events]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Scavage_Events]
GO

CREATE PROCEDURE [msg].[sp_Scavage_Events]
AS
BEGIN
	DELETE FROM msg.[Log] WHERE LogId NOT IN (SELECT top 1000 LogId FROM msg.[Log] ORDER BY ChangedDateTime DESC)
	DELETE FROM Msg.[Event] WHERE EventId NOT IN (SELECT top 1000 EventId FROM Msg.[Event] ORDER BY ChangedDateTime DESC)
	DELETE FROM Msg.Receipt WHERE ReceiptId NOT IN (SELECT top 1000 ReceiptId FROM Msg.Receipt ORDER BY ChangedDateTime DESC)

END

GO
