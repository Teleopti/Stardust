IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Stardust].[AcquireQueuedJob]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Stardust].[AcquireQueuedJob]

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Stardust].[AcquireQueuedJob]
AS
BEGIN

DECLARE 
@Idd uniqueidentifier

	update [Stardust].[JobQueue]
	set Tagged = '0', @Idd = JobId
	where JobId = (SELECT TOP 1 [JobId]
				FROM [Stardust].[JobQueue] 
				where Tagged is null
				ORDER BY Created)

SELECT * FROM [Stardust].[JobQueue] WHERE JobId = @Idd
END 