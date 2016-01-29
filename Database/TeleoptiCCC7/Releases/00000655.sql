----------------  
--Name: KJ
--Date: 2016-01-28
--Bug #36753: Check QueueSource for duplicates and try to add unique constraint on QueueMartId,QueueAggId,QueueOriginalId,DataSourceId
SELECT q.* 
FROM QueueSource q
INNER JOIN  
(SELECT QueueAggId, QueueOriginalId, DataSourceId ,count(*) 'count'
FROM [dbo].[QueueSource] 
GROUP BY QueueAggId, QueueOriginalId, DataSourceId
HAVING COUNT(*) >1) d ON q.QueueAggId = d.QueueAggId AND q.QueueOriginalId=d.QueueOriginalId AND q.DataSourceId = d.DataSourceId 
IF @@ROWCOUNT=0
BEGIN
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PK_QueueSourceIds]') AND type in (N'UQ'))
	BEGIN
		ALTER TABLE [dbo].[QueueSource]
		ADD CONSTRAINT [PK_QueueSourceIds] UNIQUE ([QueueMartId],[QueueAggId],[QueueOriginalId],[DataSourceId])
	END
END
ELSE --THERE ARE DUPLICATES
BEGIN
		--DELETE DUPLICATES IF NOT CONNECTED TO WORKLOAD
		DELETE dbo.QueueSource 
		FROM dbo.QueueSource
		LEFT OUTER JOIN (
		   SELECT CONVERT(uniqueidentifier, MIN(CONVERT(char(36), id))) as RowId, [QueueMartId],[QueueAggId],[QueueOriginalId],[DataSourceId]
		   FROM dbo.QueueSource 
		   GROUP BY [QueueMartId],[QueueAggId],[QueueOriginalId],[DataSourceId]
		) as KeepQueues ON
		   dbo.QueueSource.Id = KeepQueues.RowId
		WHERE
		   KeepQueues.RowId IS NULL
		AND dbo.QueueSource.Id NOT IN (SELECT QueueSource from QueueSourceCollection)
		--CHECK AGAIN FOR DUPLICATES
		SELECT q.* 
		FROM QueueSource q
		INNER JOIN  
		(SELECT QueueAggId, QueueOriginalId, DataSourceId ,count(*) 'count'
		FROM [dbo].[QueueSource] 
		GROUP BY QueueAggId, QueueOriginalId, DataSourceId
		HAVING COUNT(*) >1) d ON q.QueueAggId = d.QueueAggId AND q.QueueOriginalId=d.QueueOriginalId AND q.DataSourceId = d.DataSourceId 
		IF @@ROWCOUNT=0
		BEGIN
			--ADD CONSTRAINT IF POSSIBLE
			IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PK_QueueSourceIds]') AND type in (N'UQ'))
			BEGIN
				ALTER TABLE [dbo].[QueueSource]
				ADD CONSTRAINT [PK_QueueSourceIds] UNIQUE ([QueueMartId],[QueueAggId],[QueueOriginalId],[DataSourceId])
			END
		END
END