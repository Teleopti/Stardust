DELETE FROM Tenant.AdminUser WHERE Email = 'admin@company.com'

DECLARE @admins int
SELECT @admins = count(*) FROM Tenant.AdminUser
IF @admins = 0
BEGIN
            UPDATE Tenant.Tenant SET Active = 0
END

