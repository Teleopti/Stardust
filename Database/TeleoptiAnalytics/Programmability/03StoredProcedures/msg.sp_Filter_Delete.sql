IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Filter_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Filter_Delete]
GO



----------------------------------------------------------------------------
-- Delete a single record from Filter
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Filter_Delete]
	@FilterId uniqueidentifier
AS

DELETE	Msg.Filter
WHERE 	FilterId = @FilterId


GO

