/****** Object:  UserDefinedFunction [mart].[GroupPageCodeIsBusinessHierarchy]    Script Date: 09/28/2010 10:27:23 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[GroupPageCodeIsBusinessHierarchy]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[GroupPageCodeIsBusinessHierarchy]
GO

/****** Object:  UserDefinedFunction [mart].[GroupPageCodeIsBusinessHierarchy]    Script Date: 09/28/2010 10:27:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		HG & JN
-- Create date: 2010-09-28
-- Description:	Check if a Group Page Code exists in bla bla...
-- =============================================
CREATE FUNCTION [mart].[GroupPageCodeIsBusinessHierarchy]
(
	@group_page_code uniqueidentifier
)
RETURNS bit
AS
BEGIN

	IF EXISTS(SELECT * FROM mart.dim_group_page WHERE group_page_code = @group_page_code)
	BEGIN
		RETURN 0
	END
	RETURN 1
END

GO


