SELECT orig_queue_id, log_object_id,count(*)
FROM dbo.queues
GROUP BY orig_queue_id, log_object_id
HAVING COUNT(*) > 1
IF @@ROWCOUNT=0 
BEGIN
	--EASY FIX WHEN NO DUPLICATES
	IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[queues]') AND name = N'uix_orig_queue_id')
	BEGIN
		CREATE UNIQUE NONCLUSTERED INDEX [uix_orig_queue_id] ON [dbo].[queues]([log_object_id] ASC,[orig_queue_id] ASC)
	END
END
ELSE
BEGIN
	--WE HAVE DUPLICATES
	CREATE TABLE #duplicates(queue int ,orig_queue_id int, log_object_id int)

	INSERT #duplicates(queue ,orig_queue_id, log_object_id)
	SELECT [queue] ,q.[orig_queue_id], q.[log_object_id]
	FROM dbo.queues q
	LEFT OUTER JOIN (
		SELECT min([queue]) RowId, orig_queue_id, log_object_id
		FROM dbo.queues 
		GROUP BY orig_queue_id, log_object_id
	) AS KeepQueues ON
		q.[queue] = KeepQueues.RowId
	WHERE
		KeepQueues.RowId IS NULL

	--RENAME THE DUPLICATE QUEUES
	UPDATE dbo.queues
	SET [orig_queue_id]=-10000*[queue]+([orig_queue_id]),
	[orig_desc]='DUPLICATE:' + SUBSTRING([orig_desc],1,40) ,
	[display_desc]='DUPLICATE:' + SUBSTRING([display_desc],1,40) 
	FROM dbo.queues
	WHERE [queue] in (select [queue] FROM #duplicates)

	IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[queues]') AND name = N'uix_orig_queue_id')
	BEGIN
		CREATE UNIQUE NONCLUSTERED INDEX [uix_orig_queue_id] ON [dbo].[queues]([log_object_id] ASC,[orig_queue_id] ASC)
	END

	DROP TABLE #duplicates
END

