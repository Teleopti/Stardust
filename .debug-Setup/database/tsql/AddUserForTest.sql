IF NOT EXISTS (SELECT * FROM Tenant.AdminUser WHERE Name = 'FirstAdmin')
BEGIN
	INSERT INTO Tenant.AdminUser (Name, Email, Password)
	VALUES ('FirstAdmin', 'first@admin.is', '###70D74A6BBA33B5972EADAD9DD449D273E1F4961D###')

	UPDATE Tenant.Tenant SET Active = 1
END