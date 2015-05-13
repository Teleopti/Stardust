IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_queue_statistics_up_until_date]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_queue_statistics_up_until_date]
GO
-- =============================================
-- Author:		Jonas, Maria, Kunning
-- Create date: 2015-05-13
-- Description:	Return most recent date of the queue statistics
-- ---------------------------------------------
-- ChangeLog:
-- Date			Author	Description
-- ---------------------------------------------

-- =============================================
-- EXEC [mart].[raptor_queue_statistics_up_until_date] '1,2,3,4,5'

CREATE PROCEDURE [mart].[raptor_queue_statistics_up_until_date] 
(@QueueList					varchar(max)
)
AS

BEGIN
	SET NOCOUNT ON;

	DECLARE @mostRecentDate as smalldatetime
	DECLARE @TempList table
	(
		QueueID int
	)

	INSERT INTO @TempList
		SELECT * FROM mart.SplitStringInt(@QueueList)

	SELECT
		@mostRecentDate = MAX(d.date_date)
	FROM
		mart.fact_queue fq WITH (NOLOCK)
	INNER JOIN	mart.dim_date d
		ON fq.date_id = d.date_id
	INNER JOIN	@TempList q
		ON fq.queue_id = q.QueueID

	SELECT @mostRecentDate
END
GO