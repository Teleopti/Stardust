UPDATE Tenant.Tenant
SET 
	ApplicationConnectionString = 'Data Source=.;Initial Catalog=Test_SGISikuli_TeleoptiCCC7;Integrated Security=True;Current Language=us_english',
	AnalyticsConnectionString = 'Data Source=.;Initial Catalog=Test_SGISikuli_TeleoptiAnalytics;Integrated Security=True;Current Language=us_english'
WHERE
	Name = 'Teleopti WFM'

GO
