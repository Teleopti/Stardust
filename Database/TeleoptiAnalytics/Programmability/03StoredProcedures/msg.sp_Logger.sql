IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Logger]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Logger]
GO



CREATE PROCEDURE [Msg].[sp_Logger]
AS
SELECT LogId, ProcessId, Description, Exception, Message, StackTrace, ChangedBy, ChangedDateTime 
FROM Msg.LOG 
ORDER BY changeddatetime DESC


GO

