USE [$(DBNAME)]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AcquireQueuedJob]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Stardust].[AcquireQueuedJob]

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [Stardust].[AcquireQueuedJob]
AS
BEGIN

DECLARE 
@Idd nvarchar(100)

update Stardust.JobQueue
set lockTimestamp = null
where datediff(minute,GETDATE(),lockTimestamp  ) < 0

BEGIN TRAN
	SELECT TOP 1 @Idd = [JobId]
				FROM [Stardust].[JobQueue] 
				where locktimestamp is null
				ORDER BY Created

	update [Stardust].[JobQueue]
	set locktimestamp = dateadd(minute,10,GETDATE())
	where JobId = @Idd	

COMMIT TRAN

SELECT * FROM [Stardust].[JobQueue] WHERE JobId = @Idd
END 

