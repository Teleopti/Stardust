IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[ReturnQueueId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[ReturnQueueId]
GO

-- =============================================
-- Author:		Ola
-- Create date: 2014-10-20
-- Description:	Return the Id of the queue, if it not exists in dim_queue it adds it first
-- EXEC mart.ReturnQueueId 'olas', 'datasource' , 6
-- =============================================
-- 
CREATE PROCEDURE [mart].[ReturnQueueId]
@queue_original_id nvarchar(50),
@queue_name nvarchar(50),
@datasource_id int
AS

DECLARE @retid int

SELECT @retid = queue_id FROM mart.dim_queue
WHERE (queue_name = @queue_name OR queue_original_id = @queue_original_id)
AND datasource_id = @datasource_id

IF @retid is null
BEGIN
INSERT mart.dim_queue(queue_name, queue_original_id, datasource_id)
VALUES (@queue_name, @queue_original_id, @datasource_id)

SELECT @retid = @@IDENTITY
END

select @retid as id
GO


