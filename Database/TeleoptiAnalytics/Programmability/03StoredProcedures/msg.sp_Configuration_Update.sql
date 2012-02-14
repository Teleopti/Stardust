IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Configuration_Update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Configuration_Update]
GO




----------------------------------------------------------------------------
-- Update a single record in Configuration
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Configuration_Update]
	@ConfigurationId int,
	@ConfigurationType nchar(50),
	@ConfigurationName nchar(50),
	@ConfigurationValue nvarchar(255),
	@ConfigurationDataType nvarchar(50),
	@ChangedBy nvarchar(20),
	@ChangedDateTime datetime
AS

UPDATE	Msg.Configuration
SET	ConfigurationType = @ConfigurationType,
	ConfigurationName = @ConfigurationName,
	ConfigurationValue = @ConfigurationValue,
	ConfigurationDataType = @ConfigurationDataType,
	ChangedBy = @ChangedBy,
	ChangedDateTime = @ChangedDateTime
WHERE 	ConfigurationId = @ConfigurationId



GO

