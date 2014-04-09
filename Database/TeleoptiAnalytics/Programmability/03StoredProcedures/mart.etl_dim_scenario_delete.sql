/****** Object:  StoredProcedure [mart].[etl_dim_scenario_delete]    Script Date: 10/29/2009 13:36:28 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_scenario_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_scenario_delete]
GO


/****** Object:  StoredProcedure [mart].[etl_dim_scenario_delete]    Script Date: 10/29/2009 13:36:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		JN
-- Create date: 2009-10-29
-- Description:	Clean scenarios in datamart without any data.
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_scenario_delete] 
AS
return 0

GO