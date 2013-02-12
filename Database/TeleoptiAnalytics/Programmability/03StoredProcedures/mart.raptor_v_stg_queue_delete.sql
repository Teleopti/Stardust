/****** Object:  StoredProcedure [dbo].[raptor_v_stg_queue_delete]    Script Date: 02/02/2009 15:13:42 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_v_stg_queue_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_v_stg_queue_delete]
GO
/****** Object:  StoredProcedure [mart].[raptor_v_stg_queue_delete]    Script Date: 02/02/2009 15:13:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<KJ>
-- Create date: <2009-02-02>
-- Description:	<Delete data from v_stg_queue>
-- =============================================
CREATE PROCEDURE [mart].[raptor_v_stg_queue_delete]

AS
BEGIN
	DELETE FROM mart.v_stg_queue
END
