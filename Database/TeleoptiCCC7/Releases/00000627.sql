IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PushMessageDialogue]') AND name = N'IX_PushMessageDialogue_Receiver')
DROP INDEX [IX_PushMessageDialogue_Receiver] ON [dbo].[PushMessageDialogue]

CREATE INDEX [IX_PushMessageDialogue_Receiver] ON [dbo].[PushMessageDialogue]
(
	[Receiver] ASC,
	[BusinessUnit] ASC
)