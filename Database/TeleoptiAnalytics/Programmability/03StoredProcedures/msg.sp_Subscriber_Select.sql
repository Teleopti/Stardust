/****** Object:  StoredProcedure [msg].[sp_Subscriber_Select]    Script Date: 04/07/2009 10:43:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Subscriber_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Subscriber_Select]
GO


----------------------------------------------------------------------------
CREATE PROC [msg].[sp_Subscriber_Select]
	@SubscriberId uniqueidentifier
AS
SELECT	SubscriberId,
		UserId,
		ProcessId,
		IPAddress,
		Port,
		ChangedBy,
		ChangedDateTime
FROM	Msg.Subscriber
WHERE 	SubscriberId = @SubscriberId

GO

