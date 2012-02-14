IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Event_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Event_Delete]
GO



----------------------------------------------------------------------------
-- Delete a single record from Event
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Event_Delete]
	@EventId uniqueidentifier
AS

DELETE	Msg.Event
WHERE 	EventId = @EventId


GO

