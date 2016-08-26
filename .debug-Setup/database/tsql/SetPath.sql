UPDATE Tenant.Tenant
SET 
	ApplicationConnectionString = 'Data Source=.;Initial Catalog=$(CCC7DB);Integrated Security=True;Current Language=us_english',
	AnalyticsConnectionString = 'Data Source=.;Initial Catalog=$(AnalyticsDB);Integrated Security=True;Current Language=us_english'
WHERE
	Name = 'Teleopti WFM'

GO

--demo as password
IF NOT EXISTS (SELECT * FROM Tenant.AdminUser WHERE Name = 'FirstAdmin')
BEGIN
	INSERT INTO Tenant.AdminUser (Name, Email, Password, AccessToken)
	VALUES ('FirstAdmin', 'first@admin.is', '###70D74A6BBA33B5972EADAD9DD449D273E1F4961D###', 'andaDummyToken')

	UPDATE Tenant.Tenant SET Active = 1
END
GO