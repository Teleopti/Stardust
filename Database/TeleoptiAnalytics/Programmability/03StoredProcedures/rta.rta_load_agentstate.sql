IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RTA].[rta_load_agentstate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [RTA].[rta_load_agentstate]
GO

-- =============================================
-- Author:		Robin K
-- Create date: 2008-11-25
-- Description:	Load agent states for RTA
--
-- ChangeLog:	Date		Who		Description
--
--				2010-03-24	DJ		Use JOIN instead of IN, add WITH(NOLOCK)
--				2010-12-20  AF		Only latest snapshot. Will limit use of total alarm time in RTA...
-- =============================================
--EXEC [RTA].[rta_load_agentstate] '2010-12-19 23:00:00','2010-12-20 23:00:00','2001,2002,0063,2000,0019,0068,0085,0202,0238,2003'
CREATE PROCEDURE [RTA].[rta_load_agentstate]
	@start_date datetime,
	@end_date datetime,
	@externallogonlist varchar(max)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE	@TempExternalLogOnList table
	(
	ID_num int IDENTITY(1,1),
	ExternalLogOn nvarchar(50)
	)
	
	INSERT INTO @TempExternalLogOnList
	SELECT * FROM mart.SplitStringString(@ExternalLogOnList)
	UNION ALL
	SELECT ''	--Add end of Snapshot
	
    SELECT	EAS.Id,
			EAS.LogOn [ExternalLogOn],
			EAS.StateCode,
			EAS.TimeInState,
			EAS.TimestampValue [Timestamp],
			EAS.PlatformTypeId,
			EAS.DataSourceId, 
			EAS.BatchId, 
			EAS.IsSnapshot
    FROM RTA.ExternalAgentState EAS WITH(NOLOCK)
    INNER JOIN @TempExternalLogOnList tmp
    ON EAS.LogOn = tmp.ExternalLogOn
    WHERE TimestampValue BETWEEN @start_date AND @end_date
    AND NOT EXISTS (SELECT 1
					FROM RTA.ExternalAgentState EAS2 WITH(NOLOCK)
					WHERE EAS2.LogOn = EAS.LogOn
					AND EAS2.PlatformTypeId = EAS.PlatformTypeId
					AND EAS2.DataSourceId = EAS.DataSourceId
					AND EAS2.TimestampValue > EAS.TimestampValue
					AND EAS2.IsSnapshot = 1
					)
    ORDER BY BatchId, TimestampValue, LogOn DESC;

END
GO

