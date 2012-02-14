IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Log_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Log_Delete]
GO



----------------------------------------------------------------------------
-- Delete a single record from Log
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Log_Delete]
	@LogId uniqueidentifier
AS

DELETE	Msg.Log
WHERE 	LogId = @LogId


GO

