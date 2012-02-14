/* 
Trunk initiated: 
2010-05-23 
20:54
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Devy Developer  
--Date: 2009-xx-xx  
--Desc: Because ...  
----------------  


----------------  
--Name: MathiasS
--Date: 2010-05-26
--Desc: New columns for preference and input period on workflowcontrolset
----------------  
ALTER TABLE [WorkflowControlSet] ADD [PreferencePeriodFromDate] datetime NULL
ALTER TABLE [WorkflowControlSet] ADD [PreferencePeriodToDate] datetime NULL
ALTER TABLE [WorkflowControlSet] ADD [PreferenceInputFromDate] datetime NULL
ALTER TABLE [WorkflowControlSet] ADD [PreferenceInputToDate] datetime NULL
GO
UPDATE [WorkflowControlSet] SET [PreferencePeriodFromDate] = [SchedulePreferenceDate]
GO
ALTER TABLE [WorkflowControlSet] DROP COLUMN [SchedulePreferenceDate]
GO
UPDATE [WorkflowControlSet] SET [PreferencePeriodFromDate] = '2010-02-01'
UPDATE [WorkflowControlSet] SET [PreferencePeriodToDate] = '2010-02-28'
UPDATE [WorkflowControlSet] SET [PreferenceInputFromDate] = '2010-01-01'
UPDATE [WorkflowControlSet] SET [PreferenceInputToDate] = '2010-01-31'
GO
ALTER TABLE [WorkflowControlSet] ALTER COLUMN [PreferencePeriodFromDate] datetime NOT NULL
ALTER TABLE [WorkflowControlSet] ALTER COLUMN [PreferencePeriodToDate] datetime NOT NULL
ALTER TABLE [WorkflowControlSet] ALTER COLUMN [PreferenceInputFromDate] datetime NOT NULL
ALTER TABLE [WorkflowControlSet] ALTER COLUMN [PreferenceInputToDate] datetime NOT NULL
GO

----------------  
--Name: claess
--Date: 2010-05-28 07:45 
--Desc: new table for allowed shiftcategories in preferences (wfc)
--		
----------------  

CREATE TABLE [dbo].[WorkflowControlSetAllowedShiftCategories](
	[WorkflowControlSet] [uniqueidentifier] NOT NULL,
	[ShiftCategory] [uniqueidentifier] NOT NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[WorkflowControlSetAllowedShiftCategories]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSetAllowedShiftCategories_ShiftCategory] FOREIGN KEY([ShiftCategory])
REFERENCES [dbo].[ShiftCategory] ([Id])
GO

ALTER TABLE [dbo].[WorkflowControlSetAllowedShiftCategories] CHECK CONSTRAINT [FK_WorkflowControlSetAllowedShiftCategories_ShiftCategory]
GO

ALTER TABLE [dbo].[WorkflowControlSetAllowedShiftCategories]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSetAllowedShiftCategories_WorkflowControlSet] FOREIGN KEY([WorkflowControlSet])
REFERENCES [dbo].[WorkflowControlSet] ([Id])
GO

ALTER TABLE [dbo].[WorkflowControlSetAllowedShiftCategories] CHECK CONSTRAINT [FK_WorkflowControlSetAllowedShiftCategories_WorkflowControlSet]
GO


----------------  
--Name: claess
--Date: 2010-05-28 07:45 
--Desc: new table for allowed dayoffs in preferences (wfc)
--		
----------------  
CREATE TABLE [dbo].[WorkflowControlSetAllowedDayOffs](
	[WorkflowControlSet] [uniqueidentifier] NOT NULL,
	[DayOffTemplate] [uniqueidentifier] NOT NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[WorkflowControlSetAllowedDayOffs]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSetAllowedDayOffs_DayOff] FOREIGN KEY([DayOffTemplate])
REFERENCES [dbo].[DayOffTemplate] ([Id])
GO

ALTER TABLE [dbo].[WorkflowControlSetAllowedDayOffs] CHECK CONSTRAINT [FK_WorkflowControlSetAllowedDayOffs_DayOff]
GO

ALTER TABLE [dbo].[WorkflowControlSetAllowedDayOffs]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSetAllowedDayOffs_WorkflowControlSet] FOREIGN KEY([WorkflowControlSet])
REFERENCES [dbo].[WorkflowControlSet] ([Id])
GO

ALTER TABLE [dbo].[WorkflowControlSetAllowedDayOffs] CHECK CONSTRAINT [FK_WorkflowControlSetAllowedDayOffs_WorkflowControlSet]
GO


 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (269,'7.1.269') 
