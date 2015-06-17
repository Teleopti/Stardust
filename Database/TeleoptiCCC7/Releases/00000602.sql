insert into tenant.TenantApplicationNhibernateConfig
(TenantId, ConfigKey, ConfigValue)
select id, 'command_timeout', '60'
from tenant.tenant
