IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Configuration_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Configuration_Insert]
GO




----------------------------------------------------------------------------
-- Insert a single record into Configuration
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Configuration_Insert]
	@ConfigurationId int OUTPUT,
	@ConfigurationType nvarchar(50),
	@ConfigurationName nvarchar(50),
	@ConfigurationValue nvarchar(255),
	@ConfigurationDataType nvarchar(50),
	@ChangedBy nvarchar(10),
	@ChangedDateTime datetime
AS
IF(@ConfigurationId  = 0)
	BEGIN
		DECLARE @RowsInTable INT 
		SET @RowsInTable = (SELECT COUNT(*) FROM Msg.[Configuration]) 
		IF(@RowsInTable = 0)
			SET @ConfigurationId = 1
		ELSE
			SET @ConfigurationId = (SELECT MAX(ConfigurationId) + 1 FROM Msg.[Configuration]) 
		INSERT Msg.Configuration(ConfigurationId, ConfigurationType, ConfigurationName, ConfigurationValue, ConfigurationDataType, ChangedBy, ChangedDateTime)
		VALUES (@ConfigurationId, @ConfigurationType, @ConfigurationName, @ConfigurationValue, @ConfigurationDataType, @ChangedBy, @ChangedDateTime)
	END
ELSE
	BEGIN
		DECLARE @DoesExists INT
		SET @DoesExists = (SELECT Count(*) FROM Msg.Configuration WHERE ConfigurationId = @ConfigurationId)
		IF(@DoesExists = 0)
			BEGIN
				INSERT Msg.Configuration(ConfigurationId, ConfigurationType, ConfigurationName, ConfigurationValue, ConfigurationDataType, ChangedBy, ChangedDateTime)
				VALUES (@ConfigurationId, @ConfigurationType, @ConfigurationName, @ConfigurationValue, @ConfigurationDataType, @ChangedBy, @ChangedDateTime)
			END
		ELSE
			BEGIN
			UPDATE	Msg.Configuration
				SET	ConfigurationType = @ConfigurationType,
					ConfigurationName = @ConfigurationName,
					ConfigurationValue = @ConfigurationValue,
					ConfigurationDataType = @ConfigurationDataType,
					ChangedBy = @ChangedBy,
					ChangedDateTime = @ChangedDateTime
				WHERE 	ConfigurationId = @ConfigurationId
			END
	END



GO

