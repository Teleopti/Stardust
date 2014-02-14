--Create
ALTER TABLE [dbo].[ApplicationFunction]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationFunction_ApplicationFunction] FOREIGN KEY([Parent])
REFERENCES [dbo].[ApplicationFunction] ([Id])


ALTER TABLE [dbo].[ApplicationFunction] CHECK CONSTRAINT [FK_ApplicationFunction_ApplicationFunction]


ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_Person_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])


ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_Person_Person_UpdatedBy]


ALTER TABLE [dbo].[WorkflowControlSet]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSet_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])


ALTER TABLE [dbo].[WorkflowControlSet] CHECK CONSTRAINT [FK_WorkflowControlSet_Person_UpdatedBy]


ALTER TABLE [dbo].[WorkflowControlSet]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSet_Activity_AllowedPreferenceActivity] FOREIGN KEY([AllowedPreferenceActivity])
REFERENCES [dbo].[Activity] ([Id])


ALTER TABLE [dbo].[WorkflowControlSet] CHECK CONSTRAINT [FK_WorkflowControlSet_Activity_AllowedPreferenceActivity]


ALTER TABLE [dbo].[BusinessUnit]  WITH CHECK ADD  CONSTRAINT [FK_BusinessUnit_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])


ALTER TABLE [dbo].[BusinessUnit] CHECK CONSTRAINT [FK_BusinessUnit_Person_UpdatedBy]


ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_Person_WriteProtection_UpdatedBy] FOREIGN KEY([WriteProtectionUpdatedBy])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_Person_WriteProtection_UpdatedBy]

