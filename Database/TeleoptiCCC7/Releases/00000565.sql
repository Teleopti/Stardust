exec sp_rename @objname =N'Tenant.PK_PasswordPolicyForUser',@newname = N'PK_ApplicationLogonInfo', @objtype = N'OBJECT'
exec sp_rename @objname =N'Tenant.FK_PasswordPolicyForUser_PersonInfo',@newname = N'FK_ApplicationLogonInfo_PersonInfo', @objtype = N'OBJECT'
