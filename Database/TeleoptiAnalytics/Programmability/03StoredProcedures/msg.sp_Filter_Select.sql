IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Filter_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Filter_Select]
GO



----------------------------------------------------------------------------
-- Select a single record from Filter
----------------------------------------------------------------------------
CREATE PROC [msg].[sp_Filter_Select]
	@FilterId uniqueidentifier
AS

SELECT	FilterId,
	SubscriberId,
	ReferenceObjectId,
	ReferenceObjectType,
	DomainObjectId,
	DomainObjectType,
	EventStartDate,
	EventEndDate,
	ChangedBy,
	ChangedDateTime
FROM	Msg.Filter
WHERE 	FilterId = @FilterId


GO

