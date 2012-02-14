IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Filter_Select_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Filter_Select_All]
GO



----------------------------------------------------------------------------
-- Select a single record from Filter
----------------------------------------------------------------------------
CREATE PROC [msg].[sp_Filter_Select_All]
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


GO

