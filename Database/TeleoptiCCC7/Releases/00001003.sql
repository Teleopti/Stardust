UPDATE [dbo].[ApplicationFunction] 
  SET FunctionDescription = 'xxResReportReadyTimeAdherencePerAgent'
  WHERE FunctionDescription = 'xxResReportAdherencePerAgent'

GO

UPDATE [dbo].[ApplicationFunction] 
  SET FunctionDescription = 'xxResReportReadyTimeAdherencePerDay'
  WHERE FunctionDescription = 'xxResReportAdherencePerDay'

GO

