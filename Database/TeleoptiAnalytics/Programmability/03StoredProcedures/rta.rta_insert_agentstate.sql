IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RTA].[rta_insert_agentstate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [RTA].[rta_insert_agentstate]
GO

-- =============================================
-- Author:		Robin K
-- Create date: 2008-11-24
-- Description:	Insert data into agent state for RTA
-- =============================================
CREATE PROCEDURE [RTA].[rta_insert_agentstate]
@LogOn nvarchar(50),
@StateCode nvarchar(50),
@TimeInState bigint,
@Timestamp datetime,
@PlatformTypeId uniqueidentifier,
@DataSourceId int,
@BatchId datetime,
@IsSnapshot bit
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [RTA].[ExternalAgentState] (LogOn,StateCode,TimeInState,TimestampValue,PlatformTypeId,DataSourceId,BatchId,IsSnapshot)
		VALUES (@LogOn,@StateCode,@TimeInState,@Timestamp,@PlatformTypeId,@DataSourceId,@BatchId,@IsSnapshot)
END

GO
