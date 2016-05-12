IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[UpdateScheduleProjectionReadOnly]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [ReadModel].[UpdateScheduleProjectionReadOnly]
GO

CREATE PROCEDURE [ReadModel].[UpdateScheduleProjectionReadOnly]
	@PersonId uniqueidentifier,
	@ScenarioId uniqueidentifier,
	@BelongsToDate datetime,
	@PayloadId uniqueidentifier,
	@StartDateTime datetime,
	@EndDateTime datetime,
	@WorkTime bigint,
	@ContractTime bigint,
	@Name nvarchar(50),
	@ShortName nvarchar(25),
	@DisplayColor int,
	@PayrollCode nvarchar(20),
	@InsertedOn datetime
AS
SET NOCOUNT ON

INSERT INTO ReadModel.ScheduleProjectionReadOnly (
	ScenarioId,
	PersonId,
	BelongsToDate,
	PayloadId,
	StartDateTime,
	EndDateTime,
	WorkTime,
	ContractTime,
	Name,
	ShortName,
	DisplayColor,
	PayrollCode,
	InsertedOn
)
VALUES (
	@ScenarioId,
	@PersonId,
	@BelongsToDate,
	@PayloadId,
	@StartDateTime,
	@EndDateTime,
	@WorkTime,
	@ContractTime,
	@Name,
	@ShortName,
	@DisplayColor,
	@PayrollCode,
	@InsertedOn
)

GO