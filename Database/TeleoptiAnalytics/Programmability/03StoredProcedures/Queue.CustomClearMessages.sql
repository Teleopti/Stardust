IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Queue].[CustomClearMessages]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Queue].[CustomClearMessages]
GO
--[Queue].[CustomClearMessages] @QueueName='rta'
CREATE PROCEDURE [Queue].[CustomClearMessages]
	@QueueName nvarchar(50)
AS
BEGIN
	SET NOCOUNT ON;


	DELETE FROM Queue.Messages
	FROM Queue.Messages	m
	INNER JOIN
		(
			SELECT QueueId
			FROM Queue.Queues
			WHERE QueueName = @QueueName
			OR ParentQueueId in
				(
					SELECT QueueId 
					FROM Queue.Queues 
					WHERE QueueName = @QueueName
				)
		) q
	ON m.QueueId = q.QueueId
END
GO