--Example call:
--EXEC [dbo].[LoadRtaMappingsFor] null, null, '12D99B6A-CFA1-4025-8B18-A5CD00C9040E', null, null
--select * from v_RtaMapping

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadRtaMappingsFor]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[LoadRtaMappingsFor]
GO

CREATE PROCEDURE [dbo].[LoadRtaMappingsFor] 
	@statecode1 nvarchar(25), 
	@statecode2 nvarchar(25),
	@activity1 uniqueidentifier,
	@activity2 uniqueidentifier,
	@activity3 uniqueidentifier
AS

SELECT @statecode1 = ISNULL(@statecode1, 'magic.string')
SELECT @statecode2 = ISNULL(@statecode2, 'magic.string')
SELECT @activity1 = ISNULL(@activity1, '00000000-0000-0000-0000-000000000000')
SELECT @activity2 = ISNULL(@activity2, '00000000-0000-0000-0000-000000000000')
SELECT @activity3 = ISNULL(@activity3, '00000000-0000-0000-0000-000000000000')

SELECT * FROM v_RtaMapping m WHERE
	ISNULL(m.StateCode, 'magic.string') IN (@statecode1, @statecode2)
	AND
	ISNULL(m.ActivityId, '00000000-0000-0000-0000-000000000000') IN (@activity1, @activity2, @activity3)

GO
