--Example call:
--EXEC [dbo].[LoadAllRtaMappings]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadAllRtaMappings]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[LoadAllRtaMappings]
GO

CREATE PROCEDURE [dbo].[LoadAllRtaMappings]
AS

SELECT
	*
FROM
	v_RtaMapping

GO
