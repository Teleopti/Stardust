DECLARE @ParentForeignId as varchar(255)
DECLARE @ParentId as uniqueidentifier

SELECT @ParentForeignId = '0080'	--Move Insights into "Web"
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')

UPDATE [dbo].[ApplicationFunction] SET [FunctionCode] = 'Insights', [FunctionDescription] = 'xxInsights', [Parent] = @ParentId WHERE [FunctionCode] = 'PmNextGen'
GO

UPDATE [dbo].[ApplicationFunction] SET [FunctionCode] = 'ViewInsightsReport', [FunctionDescription] = 'xxViewInsightsReport' WHERE [FunctionCode] = 'PmNextGenViewReport'
GO

UPDATE [dbo].[ApplicationFunction] SET [FunctionCode] = 'EditInsightsReport', [FunctionDescription] = 'xxEditInsightsReport' WHERE [FunctionCode] = 'PmNextGenEditReport'
GO
