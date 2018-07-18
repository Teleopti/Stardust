UPDATE [mart].[report]
	SET report_name = 'Ready Time Adherence per Agent', 
		report_name_resource_key = 'ResReportReadyTimeAdherencePerAgent' 
	WHERE report_name_resource_key = 'ResReportAdherencePerAgent'

GO

UPDATE [mart].[report]
	SET report_name = 'Ready Time Adherence per Day', 
		report_name_resource_key = 'ResReportReadyTimeAdherencePerDay' 
	WHERE report_name_resource_key = 'ResReportAdherencePerDay'

GO

UPDATE [mart].[report_control_collection]
   SET [control_name_resource_key] = 'ResReadyTimeAdherenceCalculationColon'
 WHERE control_name_resource_key = 'ResAdherenceCalculationColon'

GO