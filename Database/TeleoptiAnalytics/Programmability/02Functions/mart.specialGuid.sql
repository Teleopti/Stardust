IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[specialGuid]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[specialGuid]
GO

-- =============================================
-- Author:		David Jonsson
-- Create date: 2011-07-145
-- Description:	This function returns an CCC empty GUID depending of need
-- SELECT [mart].[specialGuid](0)
-- =============================================
CREATE FUNCTION [mart].[specialGuid](@GuidType int)
returns uniqueidentifier
AS
BEGIN
DECLARE @martGuid uniqueidentifier
SELECT @martGuid =
	CASE @GuidType
		WHEN 2 THEN '00000000-0000-0000-0000-000000000002'
		WHEN 1 THEN '00000000-0000-0000-0000-000000000001'
		WHEN 0 THEN '00000000-0000-0000-0000-000000000000'
	END
	
	RETURN @martGuid
END

GO


