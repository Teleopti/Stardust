IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Configuration_Select_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Configuration_Select_All]
GO




----------------------------------------------------------------------------
-- Select a single record from Configuration
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Configuration_Select_All]
AS

SELECT	ConfigurationId,
	ConfigurationType,
	ConfigurationName,
	ConfigurationValue,
	ConfigurationDataType,
	ChangedBy,
	ChangedDateTime
FROM	Msg.Configuration



GO

