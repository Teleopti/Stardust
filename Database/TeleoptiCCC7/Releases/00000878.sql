-- we need the tenant active when we create new databases
-- othewise the connectionstrings won't be updated
DECLARE @tenants int
SELECT @tenants = count(*) FROM Tenant.Tenant
IF @tenants = 1
BEGIN
            UPDATE Tenant.Tenant SET Active = 1
END