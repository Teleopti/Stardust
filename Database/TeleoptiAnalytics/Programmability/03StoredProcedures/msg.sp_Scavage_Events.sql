/****** Object:  StoredProcedure [msg].[sp_Scavage_Events]    Script Date: 04/07/2009 10:43:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Scavage_Events]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Scavage_Events]
GO

CREATE PROCEDURE [msg].[sp_Scavage_Events]
AS
BEGIN

	-- Create temporary Event table
	CREATE TABLE #tempEvent
	([EventId] uniqueidentifier NOT NULL)
	
	CREATE TABLE #tempLog
	([LogId] uniqueidentifier NOT NULL)
	
	INSERT INTO #tempLog SELECT TOP 1000 LogId FROM Msg.[Log] ORDER BY ChangedDateTime DESC	
	INSERT INTO #tempEvent SELECT TOP 1000 EventId FROM Msg.[Event] ORDER BY ChangedDateTime DESC	

	DELETE FROM msg.[Log] WHERE LogId NOT IN (SELECT LogId FROM #tempLog)
	DELETE FROM Msg.[Event] WHERE EventId NOT IN (SELECT EventId FROM #tempEvent)
	DELETE FROM Msg.Receipt WHERE EventId NOT IN (SELECT EventId FROM #tempEvent)

END

GO
