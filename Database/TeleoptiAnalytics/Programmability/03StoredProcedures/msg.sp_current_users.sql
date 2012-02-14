/****** Object:  StoredProcedure [msg].[sp_current_users]    Script Date: 04/07/2009 10:43:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_current_users]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_current_users]
GO


CREATE PROCEDURE [msg].[sp_current_users]
AS
BEGIN
	select ProcessId, UserName from vCurrentUsers
END

GO

