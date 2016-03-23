-- change Alive to bit
DROP TABLE [Stardust].[WorkerNodes]
GO
CREATE TABLE [Stardust].[WorkerNodes](
	[Id] [uniqueidentifier] NOT NULL,
	[Url] [nvarchar](450) NOT NULL,
	[Heartbeat] DateTime NULL,
	[Alive] Bit NULL
 CONSTRAINT [PK_WorkerNodes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))

GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_WorkerNodes_Url] ON [Stardust].[WorkerNodes]
(
	[Url] ASC
)
GO