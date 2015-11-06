UPDATE Tenant.Tenant
SET 
	ApplicationConnectionString = 'Data Source=.;Initial Catalog=$(CCC7DB);Integrated Security=True;Current Language=us_english',
	AnalyticsConnectionString = 'Data Source=.;Initial Catalog=$(AnalyticsDB);Integrated Security=True;Current Language=us_english'
WHERE
	Name = 'Teleopti WFM'

GO
