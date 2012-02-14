

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DequeueNormalizeMessages]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DequeueNormalizeMessages]
GO

CREATE PROCEDURE [dbo].[DequeueNormalizeMessages]
(@BusinessUnit		uniqueidentifier)
AS
BEGIN
	SET NOCOUNT ON;

	CREATE TABLE #tempresult
	(
		Id uniqueidentifier not null,
		[Message] nvarchar(2000) not null,
		[Type] nvarchar(250) not null
	);

	INSERT INTO #tempresult SELECT id,[message],[type] FROM dbo.DenormalizationQueue
	
	DELETE FROM dbo.DenormalizationQueue WHERE Id IN (SELECT Id FROM #tempresult)
	
	SELECT * FROM #tempresult
END


GO

