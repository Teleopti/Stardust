IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Log_Select_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Log_Select_All]
GO



----------------------------------------------------------------------------
-- Select all records from Log
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Log_Select_All]
AS

SELECT	LogId,
	ProcessId,
	Description,
	Exception,
	Message,
	StackTrace,
	ChangedBy,
	ChangedDateTime
FROM	Msg.[Log]


GO

