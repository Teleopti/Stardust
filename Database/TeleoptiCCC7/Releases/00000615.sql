/*
Bug #34363 _might_ have caused incorrect data.
To be on the safe side, let's remove it.
At this time, no other then command_timout shouldn't be in the database.
*/
delete from tenant.TenantApplicationNhibernateConfig where configkey<>'command_timeout'
