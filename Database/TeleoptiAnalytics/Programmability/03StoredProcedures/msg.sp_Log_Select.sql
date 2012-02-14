IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Log_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Log_Select]
GO



----------------------------------------------------------------------------
-- Select a single record from Log
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Log_Select]
	@LogId uniqueidentifier
AS

SELECT	LogId,
	ProcessId,
	Description,
	Exception,
	Message,
	StackTrace,
	ChangedBy,
	ChangedDateTime
FROM	Msg.Log
WHERE 	LogId = @LogId


GO

