IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Queue].[CustomClearSubQueueMessages]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Queue].[CustomClearSubQueueMessages]
GO

--[Queue].[CustomClearSubQueueMessages] @QueueName='general' @Subqueue='Timeout'
CREATE PROCEDURE [Queue].[CustomClearSubQueueMessages]
	@QueueName nvarchar(50),
	@Subqueue nvarchar(50)
AS
BEGIN
	SET NOCOUNT ON;
	declare @queueId int;

	select @queueId = child.QueueId
	 from Queue.Queues child
	 Inner join Queue.Queues parent on child.ParentQueueId = parent.QueueId
	 WHERE parent.QueueName = @QueueName AND child.QueueName = @Subqueue
	 
	DELETE FROM Queue.Messages
	where QueueId = @queueId
END
GO