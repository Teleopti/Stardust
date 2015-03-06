IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RTA].[rta_get_last_batch]') AND type in (N'P', N'PC'))
DROP PROCEDURE [RTA].[rta_get_last_batch]
GO

-- =============================================
-- Author:			Erik S
-- Create date:		2013-09-02
-- Description:		Get agents from last batch
-- =============================================
--EXEC [RTA].[rta_get_last_batch] @datasource_id = 1, @batch_id = "2013-09-03 09:27:43"
CREATE PROCEDURE [RTA].[rta_get_last_batch]
	@datasource_id nvarchar(10),
	@batch_id datetime
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @toDayDateOnly smalldatetime
	DECLARE @toDayDateId int
	DECLARE @maxDateId int
	SET @toDayDateOnly = DATEADD(dd, 0, DATEDIFF(dd, 0, GETUTCDATE()))
	SELECT @toDayDateId = date_id FROM mart.dim_date WHERE date_date = @toDayDateOnly
	SELECT @maxDateId = MAX(date_id) FROM mart.dim_date WHERE date_id > 0
	
	SELECT @toDayDateId = ISNULL(@toDayDateId, @maxDateId)
	
	SELECT
		AAS.BusinessUnitId,
		AAS.PersonId,
		AAS.StateCode,
		AAS.PlatformTypeId,
		AAS.State,
		AAS.StateId,
		AAS.Scheduled,
		AAS.ScheduledId,
		AAS.StateStart,
		AAS.ScheduledNext,
		AAS.ScheduledNextId,
		AAS.NextStart
	FROM RTA.ActualAgentState AAS
	
	WHERE AAS.OriginalDataSourceId = @datasource_id
	AND (
		AAS.BatchId < @batch_id
		OR 
		AAS.BatchId IS NULL
		)
END