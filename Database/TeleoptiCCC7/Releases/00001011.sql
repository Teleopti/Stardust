IF NOT EXISTS
        (
        SELECT 1
        FROM Tenant.ServerConfiguration
        WHERE [Key] = 'PayrollSourcePath'	
        )
		INSERT INTO Tenant.ServerConfiguration SELECT 'PayrollSourcePath', ''