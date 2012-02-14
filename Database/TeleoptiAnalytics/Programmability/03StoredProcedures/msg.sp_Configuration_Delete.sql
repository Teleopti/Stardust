IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Configuration_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Configuration_Delete]
GO




----------------------------------------------------------------------------
-- Delete a single record from Configuration
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Configuration_Delete]
	@ConfigurationId int
AS

DELETE	Msg.Configuration
WHERE 	ConfigurationId = @ConfigurationId



GO

