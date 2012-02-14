IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Subscriber_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Subscriber_Delete]
GO



----------------------------------------------------------------------------
-- Delete a single record from Subscriber
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Subscriber_Delete]
	@SubscriberId uniqueidentifier
AS

DELETE	Msg.Subscriber
WHERE 	SubscriberId = @SubscriberId


GO

