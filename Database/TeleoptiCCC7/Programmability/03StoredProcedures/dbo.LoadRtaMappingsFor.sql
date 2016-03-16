--Example call:
--EXEC [dbo].[LoadRtaMappingsFor] null, null, '12D99B6A-CFA1-4025-8B18-A5CD00C9040E', null, null
--select * from v_RtaMapping

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadRtaMappingsFor]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[LoadRtaMappingsFor]
GO

CREATE PROCEDURE [dbo].[LoadRtaMappingsFor] 
	@statecode1 nvarchar(25) NULL, 
	@statecode2 nvarchar(25) NULL,
	@activity1 uniqueidentifier NULL,
	@activity2 uniqueidentifier NULL,
	@activity3 uniqueidentifier NULL
AS

SELECT * FROM v_RtaMapping m WHERE
	(m.StateCode IS NULL OR m.StateCode IN (@statecode1, @statecode2))
	AND
	(m.ActivityId IS NULL OR m.ActivityId IN (@activity1, @activity2, @activity3))

UNION

SELECT TOP 1 * FROM v_RtaMapping m WHERE m.StateCode = @statecode1

UNION

SELECT TOP 1 * FROM v_RtaMapping m WHERE m.StateCode = @statecode2

GO