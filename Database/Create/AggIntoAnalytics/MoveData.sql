--=============================
-- This script need to be executed in SQLCMD mode with two paramters:
--:setvar logobjectid 2
--:setvar SourceAgg TeleoptiCCC7Agg_Demo
--=============================
SET NOCOUNT ON
GO
--need to drop FKs in order to use delete+output clause
ALTER TABLE [dbo].[agent_logg] DROP CONSTRAINT [FK_agent_logg_agent_info]
ALTER TABLE [dbo].[agent_logg] DROP CONSTRAINT [FK_agent_logg_queues]
ALTER TABLE [dbo].[queue_logg] DROP CONSTRAINT [FK_queue_logg_queues]
GO

BEGIN TRY
	DECLARE @stopDate smalldatetime
	SET @stopDate = getdate()

	SELECT count(*) 'rows to be purge from queue_logg'
	FROM $(SourceAgg).dbo.queue_logg source
	INNER JOIN $(SourceAgg).dbo.queues q
		ON q.queue = source.queue
		AND q.log_object_id = $(logobjectid)
	WHERE source.date_from < @stopDate

	SELECT count(*) 'rows to be purge from agent_logg'
	FROM $(SourceAgg).dbo.agent_logg source
	INNER JOIN $(SourceAgg).dbo.agent_info a
		ON a.Agent_id = source.Agent_id
		AND a.log_object_id = $(logobjectid)
	WHERE source.date_from < @stopDate

	WHILE 1=1
	BEGIN

		--Issue a transaction
		BEGIN TRAN

		--purge agent_logg
		delete top (10000) source 
		output deleted.*
		into dbo.agent_logg
		from $(SourceAgg).dbo.agent_logg source
		INNER JOIN $(SourceAgg).dbo.agent_info a
			ON a.Agent_id = source.Agent_id
			AND a.log_object_id = $(logobjectid)
		where source.date_from < @stopDate
		
		--done?
		if @@rowcount = 0  break;

		--commit
		COMMIT TRAN

	END

	--end outstanding transaction from last loop
	IF @@trancount > 0 COMMIT TRAN

	WHILE 1=1
	BEGIN

		--Issue a transaction
		BEGIN TRAN

		--purge queue_logg
		delete top (10000) source
		output deleted.*
		into dbo.queue_logg
		from $(SourceAgg).dbo.queue_logg source
		INNER JOIN $(SourceAgg).dbo.queues q
			ON q.queue = source.queue
			AND q.log_object_id = $(logobjectid)
		WHERE source.date_from < @stopDate
		
		if @@rowcount = 0  break;

		--commit
		COMMIT TRAN

	END

	--end outstanding transaction from last loop
	IF @@trancount > 0 COMMIT TRAN

END TRY

BEGIN CATCH
    ROLLBACK TRAN
END CATCH;
GO

--Re-add Fks when done
ALTER TABLE [dbo].[agent_logg]  WITH CHECK ADD  CONSTRAINT [FK_agent_logg_agent_info] FOREIGN KEY([agent_id])
REFERENCES [dbo].[agent_info] ([Agent_id])
ALTER TABLE [dbo].[agent_logg] CHECK CONSTRAINT [FK_agent_logg_agent_info]

ALTER TABLE [dbo].[agent_logg]  WITH CHECK ADD  CONSTRAINT [FK_agent_logg_queues] FOREIGN KEY([queue])
REFERENCES [dbo].[queues] ([queue])
ALTER TABLE [dbo].[agent_logg] CHECK CONSTRAINT [FK_agent_logg_queues]

ALTER TABLE [dbo].[queue_logg]  WITH CHECK ADD  CONSTRAINT [FK_queue_logg_queues] FOREIGN KEY([queue])
REFERENCES [dbo].[queues] ([queue])
ALTER TABLE [dbo].[queue_logg] CHECK CONSTRAINT [FK_queue_logg_queues]
